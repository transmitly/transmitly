// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
//  
//  Licensed under the Apache License, Version 2.0 (the "License")
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

namespace Transmitly.Pipeline.Configuration;

/// <summary>
/// Base implementation of a pipeline service that manages pipeline lookups and filtering.
/// </summary>
/// <remarks>
/// Base implementation of a pipeline service that manages pipeline lookups and filtering.
/// </remarks>
/// <param name="pipelineFactory">The factory responsible for pipeline creation and retrieval.</param>
/// <param name="logger">The logger instance to use.</param>
public abstract class BasePipelineService(IPipelineFactory pipelineFactory, ILogger logger) : IPipelineService
{
	private readonly IReadOnlyCollection<IPipeline> EmptyPipelines = Array.Empty<IPipeline>();
	private readonly IReadOnlyCollection<CommunicationsStatus> EmptyStatuses = Array.Empty<CommunicationsStatus>();
	private readonly IPipelineFactory _pipelineFactory = Guard.AgainstNull(pipelineFactory);
	private readonly ILogger _logger = Guard.AgainstNull(logger);

	/// <summary>
	/// Base implementation of a pipeline service that manages pipeline lookups and filtering.
	/// </summary>
	/// <param name="pipelineFactory">The factory responsible for pipeline creation and retrieval.</param>
	/// <param name="loggerFactory">The logger factory used to create pipeline service loggers.</param>
	protected BasePipelineService(IPipelineFactory pipelineFactory, ILoggerFactory loggerFactory)
		: this(pipelineFactory, Guard.AgainstNull(loggerFactory).CreateLogger<BasePipelineService>())
	{
	}

	/// <summary>
	/// Retrieves pipelines matching the specified criteria and filters.
	/// </summary>
	/// <param name="pipelineIntent">The intent identifier for the pipeline.</param>
	/// <param name="pipelineId">Optional unique identifier for the pipeline.</param>
	/// <param name="allowedChannelFilter">Collection of allowed channel identifiers to filter by.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A <see cref="PipelineLookupResult"/> containing matching pipelines and any status messages.</returns>
	public virtual async Task<PipelineLookupResult> GetMatchingPipelinesAsync(string pipelineIntent, string? pipelineId, IReadOnlyCollection<string> allowedChannelFilter, CancellationToken cancellationToken = default)
	{
		Guard.AgainstNullOrWhiteSpace(pipelineIntent);
		Guard.AgainstNull(allowedChannelFilter);
		_logger.LogDebug(
			LogEvents.PipelineLookupStarted,
			"Starting pipeline lookup.",
			(PipelineIntent: pipelineIntent, PipelineId: pipelineId),
			static state => new Dictionary<string, object?>
			{
				["pipelineIntent"] = state.PipelineIntent,
				["pipelineId"] = state.PipelineId ?? string.Empty
			});

		var pipelines = await _pipelineFactory.GetAsync(pipelineIntent, pipelineId).ConfigureAwait(false);

		if (pipelines.Count == 0)
			return new PipelineLookupResult(EmptyPipelines, [PredefinedCommunicationStatuses.PipelineNotFound]);

		if (allowedChannelFilter.Count == 0)
			return new PipelineLookupResult(pipelines, EmptyStatuses);

		if (cancellationToken.IsCancellationRequested)
			return new(EmptyPipelines, [PredefinedCommunicationStatuses.OperationCancelled]);

		var (disallowedPipelines, nonMatchingChannels) = pipelines.Aggregate(
			(Disallowed: new List<IPipeline>(), NonMatching: new List<IPipeline>()),
			(acc, pipeline) =>
			{
				if (!pipeline.Configuration.IsDispatchRequirementsAllowed)
					acc.Disallowed.Add(pipeline);
				else if (!HasMatchingChannels(pipeline, allowedChannelFilter))
					acc.NonMatching.Add(pipeline);
				return acc;
			});

		if (disallowedPipelines.Count > 0)
		{
			var results = disallowedPipelines
				.Select(pipeline => new CommunicationsStatus(
					pipeline.Id ?? pipeline.Intent,
					PredefinedCommunicationStatuses.DispatchRequirementsNotAllowed.Type,
					PredefinedCommunicationStatuses.DispatchRequirementsNotAllowed.Code))
				.ToList()
				.AsReadOnly();

			return new(EmptyPipelines, results);
		}

		if (nonMatchingChannels.Count > 0)
		{
			var results = pipelines.Select(pipeline => new CommunicationsStatus(
				pipeline.Id ?? pipeline.Intent,
				PredefinedCommunicationStatuses.DispatchChannelFilterMismatch.Type,
				PredefinedCommunicationStatuses.DispatchChannelFilterMismatch.Code))
				.ToList()
				.AsReadOnly();

			return new(EmptyPipelines, results);
		}

		_logger.LogDebug(
			LogEvents.PipelineLookupCompleted,
			"Pipeline lookup finished.",
			(PipelineIntent: pipelineIntent, PipelineCount: pipelines.Count),
			static state => new Dictionary<string, object?>
			{
				["pipelineIntent"] = state.PipelineIntent,
				["pipelineCount"] = state.PipelineCount
			});
		return new PipelineLookupResult(pipelines, EmptyStatuses);
	}

	private static bool HasMatchingChannels(IPipeline pipeline, IReadOnlyCollection<string> allowedChannelFilter)
	{
		return allowedChannelFilter.Count == 0 ||
			   allowedChannelFilter.All(filter => pipeline.Configuration.Channels.Any(c => c.Id == filter));
	}
}
