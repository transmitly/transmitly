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
using Transmitly.ChannelProvider;
using Transmitly.ChannelProvider.Configuration;

namespace Transmitly.Delivery
{
	public abstract class PipelineDeliveryStrategyProvider
	{
		public abstract Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(IReadOnlyCollection<ChannelChannelProviderGroup> sendingGroups, IDispatchCommunicationContext context, CancellationToken cancellationToken);

		protected async Task<IReadOnlyCollection<IDispatchResult?>> DispatchCommunicationAsync(IChannel channel, IChannelProvider provider, IDispatchCommunicationContext context, CancellationToken cancellationToken)
		{
			var client = GetChannelProviderClient(provider);

			var internalContext = new DispatchCommunicationContext(context, channel, provider)
			{
				RecipientAudiences = FilterRecipientAddresses(channel, provider, context.RecipientAudiences)
			};

			var communication = await GetChannelCommunicationAsync(channel, internalContext);
			IReadOnlyCollection<IDispatchResult?> results;
			if (context.Settings.IsDeliveryEnabled)
			{
				results = await client.DispatchAsync(communication, internalContext, cancellationToken);

				if (results == null || results.Count == 0)
					return [];

				return results.Where(r => r != null).Select(r => new DispatchResult(r!, provider.Id, channel.Id)).ToList();
				//context.DispatchResults.Add(new DispatchResult(result, provider.Id, channel.Id));
			}
			else
			{
				results = [new DispatchNotEnabledResult()];
				//context.DispatchResults.Add(result);
			}
			return results;
		}

		private static IReadOnlyCollection<IAudience> FilterRecipientAddresses(IChannel channel, IChannelProvider provider, IReadOnlyCollection<IAudience> audiences)
		{
			return audiences.Select(x =>
			   new AudienceRecord(
				   x.Addresses.Where(a =>
						   channel.SupportsAudienceAddress(a) && provider.SupportAudienceAddress(a)
					   )
				   )
			   ).ToList();
		}

		protected virtual IChannelProviderClient GetChannelProviderClient(IChannelProvider provider)
		{
			return provider.GetClient();
		}

		protected virtual async Task<object> GetChannelCommunicationAsync(IChannel channel, IDispatchCommunicationContext context)
		{
			return await channel.GenerateCommunicationAsync(context);
		}
	}
}