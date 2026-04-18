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

using Transmitly.Delivery;

namespace Transmitly.ChannelProvider;

internal sealed class SimulationChannelProviderDispatcher(ChannelProviderSimulationOptions options) : IChannelProviderDispatcher<object>
{
	private readonly ChannelProviderSimulationOptions _options = Guard.AgainstNull(options);

	public async Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(object communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
	{
		Guard.AgainstNull(communicationContext);

		if (!_options.SimulateDispatchResult)
			return [];

		var results = _options.SimulateDispatchResultHandler == null
			? CreateDefaultResults()
			: await _options.SimulateDispatchResultHandler(communication, communicationContext).ConfigureAwait(false) ?? [];

		DispatchDeliveryReports(communicationContext, communication, results);
		return results;
	}

	private static IReadOnlyCollection<IDispatchResult?> CreateDefaultResults() =>
		[new DispatchResult(CommunicationsStatus.Success("Dispatched"))];

	private static void DispatchDeliveryReports(IDispatchCommunicationContext context, object communication, IReadOnlyCollection<IDispatchResult?> dispatchResults)
	{
		context.LoggerFactory.CreateLogger<SimulationChannelProviderDispatcher>()
			.LogDebug(
				LogEvents.DeliveryReportDispatch,
				"Dispatching provider delivery report.",
				(EventName: DeliveryReport.Event.Dispatched(), context.ChannelId, context.ChannelProviderId, DispatchResultCount: dispatchResults.Count),
				static state => new Dictionary<string, object?>
				{
					["eventName"] = state.EventName,
					["channelId"] = state.ChannelId ?? string.Empty,
					["channelProviderId"] = state.ChannelProviderId ?? string.Empty,
					["dispatchResultCount"] = state.DispatchResultCount
				});

		foreach (var result in dispatchResults.Where(r => r != null))
		{
			context.DeliveryReportManager.DispatchAsync(
				new DeliveryReport(
					DeliveryReport.Event.Dispatched(),
					context.ChannelId,
					context.ChannelProviderId,
					context.PipelineIntent,
					context.PipelineId,
					result!.ResourceId,
					result.Status,
					communication,
					context.ContentModel,
					result.Exception));
		}
	}
}
