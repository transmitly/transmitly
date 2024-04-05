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
namespace Transmitly.ChannelProvider.Configuration
{
	public interface IChannelProviderFactory
	{
		/// <summary>
		/// Retrieves all channel providers.
		/// </summary>
		/// <returns>A read-only list of channel providers.</returns>
		Task<IReadOnlyCollection<IChannelProviderRegistration>> GetAllAsync();
		/// <summary>
		/// Retrieves channel providers that support the specified channel providers.
		/// </summary>
		/// <param name="supportedChannelProviders">The array of supported channel providers.</param>
		/// <param name="channels">The list of supported channels.</param>
		/// <returns>A read-only list of channel providers.</returns>
		Task<IReadOnlyCollection<IChannelProviderRegistration>> GetAllAsync(IReadOnlyCollection<string> supportedChannelProviders, IReadOnlyCollection<IChannel> channels);
		/// <summary>
		/// Resolves the client for the specified channel provider.
		/// </summary>
		/// <param name="channelProvider">The channel provider registration.</param>
		/// <returns>The channel provider client.</returns>
		Task<IChannelProviderClient> ResolveClientAsync(IChannelProviderRegistration channelProvider);

		Task<IChannelProviderDeliveryReportRequestAdaptor> ResolveDeliveryReportRequestAdaptorAsync(IChannelProviderDeliveryReportRequestAdaptorRegistration channelProviderDeliveryReportRequestAdaptor);
		Task<IReadOnlyCollection<IChannelProviderDeliveryReportRequestAdaptorRegistration>> GetAllDeliveryReportRequestAdaptorsAsync();
	}
}