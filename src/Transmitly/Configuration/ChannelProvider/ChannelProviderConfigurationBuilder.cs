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
using Transmitly.ChannelProvider.Configuration;

namespace Transmitly.Channel.Configuration
{
	/// <summary>
	/// Represents a builder for configuring channel providers in the communications configuration.
	/// </summary>
	public sealed class ChannelProviderConfigurationBuilder
	{
		private readonly CommunicationsClientBuilder _communicationsConfiguration;
		private readonly Action<IChannelProvider> _addProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="ChannelProviderConfigurationBuilder"/> class.
		/// </summary>
		/// <param name="communicationsConfiguration">The communications configuration builder.</param>
		/// <param name="addProvider">The action to add a channel provider.</param>
		internal ChannelProviderConfigurationBuilder(CommunicationsClientBuilder communicationsConfiguration, Action<IChannelProvider> addProvider)
		{
			_communicationsConfiguration = Guard.AgainstNull(communicationsConfiguration);
			_addProvider = Guard.AgainstNull(addProvider);
		}

		/// <summary>
		/// Adds a channel provider to the communications configuration.
		/// </summary>
		/// <param name="providerId">The ID of the provider.</param>
		/// <param name="channelProviderInstance">The instance of the channel provider.</param>
		/// <param name="supportedChannelIds">The IDs of the supported channels.</param>
		/// <returns>The communications configuration builder.</returns>
		public CommunicationsClientBuilder Add<TCommunication>(string providerId, IChannelProviderClient<TCommunication> channelProviderInstance, params string[]? supportedChannelIds)
		{
			var providerClientType = Guard.AgainstNull(channelProviderInstance);

			_addProvider(
				new ChannelProvider<TCommunication>(
					Guard.AgainstNullOrWhiteSpace(providerId),
					channelProviderInstance,
					supportedChannelIds ?? [])
			);
			return _communicationsConfiguration;
		}
	}
}