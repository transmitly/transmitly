﻿// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
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


using Transmitly.Verification;

namespace Transmitly.ChannelProvider.Configuration
{
	internal class ChannelProviderClientRegistration<TClient, TCommunication>(params string[]? supportedChannelIds) : IChannelProviderClientRegistration
		where TClient : IChannelProviderClient<TCommunication>
	{
		public Type ClientType => typeof(TClient);

		public Type CommunicationType => typeof(TCommunication);

		readonly string[] _supportedChannelIds = supportedChannelIds ?? [];

		public bool SupportsChannel(string channel)
		{
			if (_supportedChannelIds.Length == 0)
				return true;
			return Array.Exists(_supportedChannelIds, x => x.Equals(channel, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}