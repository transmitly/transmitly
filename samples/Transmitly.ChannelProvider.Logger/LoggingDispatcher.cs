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

using Microsoft.Extensions.Logging;
using System.Text.Json;
using Transmitly.Delivery;
using Transmitly.Util;

namespace Transmitly.ChannelProvider.Debugging
{
	class LoggingDispatcher(LoggingOptions options, ILogger<LoggingDispatcher> logger) : IChannelProviderDispatcher<object>
	{
		private static readonly JsonSerializerOptions _serializerOptions;
		private readonly ILogger<LoggingDispatcher> _logger = Guard.AgainstNull(logger);
		static LoggingDispatcher()
		{
			_serializerOptions = new JsonSerializerOptions { WriteIndented = true };
		}

		public async Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(object communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
		{
			_logger.Log(options.LogLevel, "Dispatching to Channel: '{ChannelId}' Content: {Content}.", communicationContext.ChannelId, JsonSerializer.Serialize(communication, _serializerOptions));

			if (!options.SimulateDispatchResult)
			{

				return [];
			}
			else if (options.SimulateDispatchResultHandler == null)
			{
				communicationContext.DeliveryReportManager.DispatchAsync(
					new DeliveryReport(
						DeliveryReport.Event.Dispatched(),
						communicationContext.ChannelId,
						communicationContext.ChannelProviderId,
						communicationContext.PipelineIntent,
						communicationContext.PipelineId,
						Guid.NewGuid().ToString(),
						CommunicationsStatus.Success("Tly.Logger", "Dispatched"),
						communication,
						communicationContext.ContentModel,
						null
					)
				);
				return [new DispatchResult(CommunicationsStatus.Success("Tly.Logger", "Dispatched"), Guid.NewGuid().ToString())];
			}

			var result = await options.SimulateDispatchResultHandler(communication, communicationContext).ConfigureAwait(false);

			foreach (var r in result?.Where(x => x != null) ?? [])
				communicationContext.DeliveryReportManager.DispatchAsync(
						new DeliveryReport(
							DeliveryReport.Event.Dispatched(),
							r!.ChannelId,
							r.ChannelProviderId,
							communicationContext.PipelineIntent,
							communicationContext.PipelineId,
							r.ResourceId,
							r.Status,
							communication,
							communicationContext.ContentModel,
							r.Exception
						)
					);
			return result ?? [];
		}
	}
}
