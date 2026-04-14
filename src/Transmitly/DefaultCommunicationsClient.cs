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

using System.Globalization;
using Transmitly.Channel.Configuration;
using Transmitly.Delivery;
using Transmitly.Pipeline.Configuration;
using Transmitly.PlatformIdentity.Configuration;

namespace Transmitly;

public sealed class DefaultCommunicationsClient : ICommunicationsClient
{
	private readonly IDeliveryReportService _deliveryReportProvider;
	private readonly IPlatformIdentityService _platformIdentityResolvers;
	private readonly IPlatformIdentityProfileEnricherService _platformIdentityProfileEnrichers;
	private readonly IPipelineService _pipelineLookupService;
	private readonly IDispatchCoordinatorService _dispatchCoordinatorService;
	private readonly ILogger _logger;

	public DefaultCommunicationsClient(
		IPipelineService pipelineService,
		IDispatchCoordinatorService dispatchCoordinatorService,
		IPlatformIdentityService platformIdentityService,
		IPlatformIdentityProfileEnricherService platformIdentityProfileEnricherService,
		IDeliveryReportService deliveryReportHandler,
		ILoggerFactory loggerFactory)
		: this(
			pipelineService,
			dispatchCoordinatorService,
			platformIdentityService,
			platformIdentityProfileEnricherService,
			deliveryReportHandler,
			Guard.AgainstNull(loggerFactory).CreateLogger<DefaultCommunicationsClient>())
	{
	}

	internal DefaultCommunicationsClient(
		IPipelineService pipelineService,
		IDispatchCoordinatorService dispatchCoordinatorService,
		IPlatformIdentityService platformIdentityService,
		IPlatformIdentityProfileEnricherService platformIdentityProfileEnricherService,
		IDeliveryReportService deliveryReportHandler,
		ILogger logger)
	{
		_deliveryReportProvider = Guard.AgainstNull(deliveryReportHandler);
		_platformIdentityResolvers = Guard.AgainstNull(platformIdentityService);
		_platformIdentityProfileEnrichers = Guard.AgainstNull(platformIdentityProfileEnricherService);
		_pipelineLookupService = Guard.AgainstNull(pipelineService);
		_dispatchCoordinatorService = Guard.AgainstNull(dispatchCoordinatorService);
		_logger = Guard.AgainstNull(logger);
	}

	public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineIntent, IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities, ITransactionModel transactionalModel, IReadOnlyCollection<string> dispatchChannelPreferences, string? pipelineId = null, string? cultureInfo = null, CancellationToken cancellationToken = default)
	{
		Guard.AgainstNullOrWhiteSpace(pipelineIntent);
		Guard.AgainstNull(transactionalModel);
		Guard.AgainstNull(platformIdentities);
		_logger.LogInformation(
			LogEvents.DispatchStarted,
			"Dispatch started for identity profiles.",
			(PipelineIntent: pipelineIntent, IdentityCount: platformIdentities.Count, ChannelPreferenceCount: dispatchChannelPreferences.Count),
			static state => new Dictionary<string, object?>
			{
				["pipelineIntent"] = state.PipelineIntent,
				["identityCount"] = state.IdentityCount,
				["channelPreferenceCount"] = state.ChannelPreferenceCount
			});

		var (matchingPipelines, errors) = await LookupPipelinesAsync(pipelineIntent, pipelineId, dispatchChannelPreferences).ConfigureAwait(false);

		if (errors.Count > 0)
		{
			_logger.LogWarning(
				LogEvents.PipelineLookupEmpty,
				"Dispatch aborted because no pipelines matched.",
				(PipelineIntent: pipelineIntent, ErrorCount: errors.Count),
				static state => new Dictionary<string, object?>
				{
					["pipelineIntent"] = state.PipelineIntent,
					["errorCount"] = state.ErrorCount
				});
			return new DispatchCommunicationResult(errors.Select(s => new DispatchResult(s)).ToList());
		}

		return await DispatchAsync(matchingPipelines, platformIdentities, transactionalModel, dispatchChannelPreferences, GuardCulture.AgainstNull(cultureInfo), cancellationToken);
	}

	public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineIntent, IReadOnlyCollection<IPlatformIdentityReference> identityReferences, ITransactionModel transactionalModel, IReadOnlyCollection<string> dispatchChannelPreferences, string? pipelineId = null, string? cultureInfo = null, CancellationToken cancellationToken = default)
	{
		_logger.LogInformation(
			LogEvents.IdentityResolutionStarted,
			"Dispatch started for identity references.",
			(PipelineIntent: pipelineIntent, ReferenceCount: identityReferences.Count),
			static state => new Dictionary<string, object?>
			{
				["pipelineIntent"] = state.PipelineIntent,
				["referenceCount"] = state.ReferenceCount
			});
		var resolvedIdentityProfiles = await _platformIdentityResolvers.ResolveIdentityProfilesAsync(identityReferences).ConfigureAwait(false);

		var (matchingPipelines, errors) = await LookupPipelinesAsync(pipelineIntent, pipelineId, dispatchChannelPreferences).ConfigureAwait(false);

		if (errors.Count > 0)
		{
			_logger.LogWarning(
				LogEvents.PipelineLookupEmpty,
				"Dispatch aborted because no pipelines matched after identity resolution.",
				(PipelineIntent: pipelineIntent, ErrorCount: errors.Count),
				static state => new Dictionary<string, object?>
				{
					["pipelineIntent"] = state.PipelineIntent,
					["errorCount"] = state.ErrorCount
				});
			return new DispatchCommunicationResult(errors.Select(s => new DispatchResult(s)).ToList());
		}

		return await DispatchAsync(matchingPipelines, resolvedIdentityProfiles, transactionalModel, dispatchChannelPreferences, GuardCulture.AgainstNull(cultureInfo), cancellationToken).ConfigureAwait(false);
	}

	public Task DispatchAsync(DeliveryReport report)
	{
		_logger.LogDebug(
			LogEvents.DeliveryReportDispatch,
			"Dispatching delivery report.",
			(report.ChannelId, report.ChannelProviderId),
			static state => new Dictionary<string, object?>
			{
				["channelId"] = state.ChannelId ?? string.Empty,
				["channelProviderId"] = state.ChannelProviderId ?? string.Empty
			});
		return _deliveryReportProvider.DispatchAsync(Guard.AgainstNull(report));
	}

	public Task DispatchAsync(IReadOnlyCollection<DeliveryReport> reports)
	{
		_logger.LogDebug(
			LogEvents.DeliveryReportDispatch,
			"Dispatching delivery reports.",
			reports.Count,
			static reportCount => new Dictionary<string, object?>
			{
				["reportCount"] = reportCount
			});
		return _deliveryReportProvider.DispatchAsync(Guard.AgainstNull(reports));
	}

	private async Task<IDispatchCommunicationResult> DispatchAsync(IReadOnlyCollection<IPipeline> pipelines, IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities, ITransactionModel transactionalModel, IReadOnlyCollection<string> dispatchChannelPreferences, CultureInfo? cultureInfo = null, CancellationToken cancellationToken = default)
	{
		var cultureInfoSafe = GuardCulture.AgainstNull(cultureInfo);

		try
		{
			_logger.LogDebug(
				LogEvents.IdentityProfileEnrichmentStarted,
				"Enriching platform identity profiles.",
				platformIdentities.Count,
				static identityCount => new Dictionary<string, object?>
				{
					["identityCount"] = identityCount
				});
			await _platformIdentityProfileEnrichers.EnrichIdentityProfilesAsync(platformIdentities).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			_logger.LogError(LogEvents.DispatchFailed, "Platform identity profile enrichment failed.", e);
			return new DispatchCommunicationResult([new DispatchResult(PredefinedCommunicationStatuses.PlatformIdentityProfileEnrichmentFailed(e))]);
		}

		var recipientContexts = await _dispatchCoordinatorService.CreateRecipientContexts(pipelines, platformIdentities, transactionalModel, dispatchChannelPreferences, cultureInfoSafe).ConfigureAwait(false);

		var allResults = new List<IDispatchCommunicationResult>(recipientContexts.Count);
		foreach (var context in recipientContexts)
		{
			allResults.Add(await context.Context.StrategyProvider.DispatchAsync([context], cancellationToken).ConfigureAwait(false));

			if (cancellationToken.IsCancellationRequested)
			{
				_logger.LogWarning(LogEvents.DispatchCancelled, "Dispatch cancelled.");
				allResults.Add(new DispatchCommunicationResult([new DispatchResult(PredefinedCommunicationStatuses.OperationCancelled)]));
				break;
			}
		}

		var allDispatchResults = allResults.SelectMany(r => r.Results).ToList().AsReadOnly();

		var dispatchResult = new DispatchCommunicationResult(allDispatchResults);
		_logger.LogInformation(
			LogEvents.DispatchCompleted,
			"Dispatch completed.",
			(ResultCount: allDispatchResults.Count, dispatchResult.IsSuccessful),
			static state => new Dictionary<string, object?>
			{
				["resultCount"] = state.ResultCount,
				["isSuccessful"] = state.IsSuccessful
			});
		return dispatchResult;
	}

	private async Task<PipelineLookupResult> LookupPipelinesAsync(
		string pipelineIntent,
		string? pipelineId,
		IReadOnlyCollection<string> dispatchChannelPreferences)
	{
		_logger.LogDebug(
			LogEvents.PipelineLookupStarted,
			"Looking up pipelines.",
			(PipelineIntent: pipelineIntent, PipelineId: pipelineId, ChannelPreferenceCount: dispatchChannelPreferences.Count),
			static state => new Dictionary<string, object?>
			{
				["pipelineIntent"] = state.PipelineIntent,
				["pipelineId"] = state.PipelineId ?? string.Empty,
				["channelPreferenceCount"] = state.ChannelPreferenceCount
			});
		var result = await _pipelineLookupService.GetMatchingPipelinesAsync(pipelineIntent, pipelineId, dispatchChannelPreferences).ConfigureAwait(false);
		_logger.LogDebug(
			LogEvents.PipelineLookupCompleted,
			"Pipeline lookup completed.",
			(PipelineIntent: pipelineIntent, PipelineCount: result.Pipelines.Count, StatusCount: result.Errors.Count),
			static state => new Dictionary<string, object?>
			{
				["pipelineIntent"] = state.PipelineIntent,
				["pipelineCount"] = state.PipelineCount,
				["statusCount"] = state.StatusCount
			});
		return result;
	}
}
