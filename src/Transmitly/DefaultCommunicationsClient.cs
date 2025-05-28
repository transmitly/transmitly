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

using Transmitly.Channel.Configuration;
using Transmitly.Delivery;
using Transmitly.Pipeline.Configuration;
using Transmitly.PlatformIdentity.Configuration;

namespace Transmitly;

public sealed class DefaultCommunicationsClient(
	IPipelineService pipelineService,
	IDispatchCoordinatorService dispatchCoordinatorService,
	IPlatformIdentityService platformIdentityService,
	IDeliveryReportService deliveryReportHandler)
	: ICommunicationsClient
{
	private readonly IDeliveryReportService _deliveryReportProvider = Guard.AgainstNull(deliveryReportHandler);
	private readonly IPlatformIdentityService _platformIdentityResolvers = Guard.AgainstNull(platformIdentityService);
	private readonly IPipelineService _pipelineLookupService = Guard.AgainstNull(pipelineService);
	private readonly IDispatchCoordinatorService _dispatchCoordinatorService = Guard.AgainstNull(dispatchCoordinatorService);

	public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineIntent, IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities, ITransactionModel transactionalModel, IReadOnlyCollection<string> dispatchChannelPreferences, string? pipelineId = null, string? cultureInfo = null, CancellationToken cancellationToken = default)
	{
		Guard.AgainstNullOrWhiteSpace(pipelineIntent);
		Guard.AgainstNull(transactionalModel);
		Guard.AgainstNull(platformIdentities);

		var culture = GuardCulture.AgainstNull(cultureInfo);

		var findMatchingPipelinesResult = await _pipelineLookupService.GetMatchingPipelinesAsync(pipelineIntent, pipelineId, dispatchChannelPreferences);

		if (findMatchingPipelinesResult.Errors.Count > 0)
			return new DispatchCommunicationResult(findMatchingPipelinesResult.Errors.Select(s => new DispatchResult(s)).ToList());

		var matchingPipelines = findMatchingPipelinesResult.Pipelines;

		var allResults = new List<IDispatchCommunicationResult>();

		var recipientContexts = await _dispatchCoordinatorService.CreateRecipientContexts(matchingPipelines, platformIdentities, transactionalModel, dispatchChannelPreferences).ConfigureAwait(false);

		foreach (var context in recipientContexts)
			allResults.Add(await context.Context.StrategyProvider.DispatchAsync([context], cancellationToken).ConfigureAwait(false));

		var allDispatchResults = allResults.SelectMany(r => r.Results).ToList().AsReadOnly();

		return new DispatchCommunicationResult(allDispatchResults);
	}

	public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IPlatformIdentityReference> identityReferences, ITransactionModel transactionalModel, IReadOnlyCollection<string> channelPreferences, string? pipelineId = null, string? cultureInfo = null, CancellationToken cancellationToken = default)
	{
		var resolvedIdentityProfiles = await _platformIdentityResolvers.ResolveIdentityProfilesAsync(identityReferences).ConfigureAwait(false);

		return await DispatchAsync(pipelineName, resolvedIdentityProfiles, transactionalModel, channelPreferences, pipelineId, cultureInfo, cancellationToken).ConfigureAwait(false);
	}

	public Task DispatchAsync(DeliveryReport report)
	{
		return _deliveryReportProvider.DispatchAsync(Guard.AgainstNull(report));
	}

	public Task DispatchAsync(IReadOnlyCollection<DeliveryReport> reports)
	{
		return _deliveryReportProvider.DispatchAsync(Guard.AgainstNull(reports));
	}
}