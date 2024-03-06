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

namespace Transmitly
{
	//todo: refactor namespaces. ChannelProvider is taken by a namespace
	sealed class ChannelProviderEntity(IChannelProviderRegistration channelProviderRegistration, Func<Task<IChannelProviderClient>> clientInstance) : IChannelProvider
	{
		public string Id => channelProviderRegistration.Id;

		public Func<Task<IChannelProviderClient>> ClientInstance => Guard.AgainstNull(clientInstance);

		public Type CommunicationType => channelProviderRegistration.CommunicationType;

		public bool SupportAudienceAddress(IAudienceAddress audienceAddress) =>
			channelProviderRegistration.SupportAudienceAddress(audienceAddress);


		public bool SupportsChannel(string channel) =>
			channelProviderRegistration.SupportsChannel(channel);
	}
}