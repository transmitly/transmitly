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

using Transmitly.ChannelProvider;

namespace Transmitly.Delivery
{
	internal class FirstMatchPipelineDeliveryStrategy : BasePipelineDeliveryStrategyProvider
	{
		public override async Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(IReadOnlyCollection<ChannelChannelProviderGroup> sendingGroups, IDispatchCommunicationContext context, CancellationToken cancellationToken)
		{
			var results = new List<IDispatchResult?>(sendingGroups.Count);
			foreach (var pair in sendingGroups)
			{
				var channel = pair.Channel;
				foreach (var provider in pair.ChannelProviderClients)
				{
					var result = await DispatchCommunicationAsync(channel, provider, context, cancellationToken);

					if (result == null || result.Count == 0)
					{
						continue;
					}

					results.AddRange(result);
					if (result.All(r => r != null && r.DispatchStatus == DispatchStatus.Error))
						continue;
					break;
				}
			}
			return results;
		}
	}
}