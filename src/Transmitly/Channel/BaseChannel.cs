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

using Transmitly.Channel.Configuration;

namespace Transmitly.Channel
{
	public abstract class BaseChannel<TCommunication>(params string[]? allowedChannelProviders) : IChannel<TCommunication>
	{
		public Type CommunicationType => typeof(TCommunication);
		public abstract string Id { get; }
		public virtual IEnumerable<string> AllowedChannelProviderIds { get; } = allowedChannelProviders ?? [];

		public ExtendedProperties ExtendedProperties { get; } = new ExtendedProperties();

		public abstract Task<TCommunication> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext);
		public abstract bool SupportsIdentityAddress(IIdentityAddress identityAddress);

		async Task<object> IChannel.GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
		{
			var result = await GenerateCommunicationAsync(communicationContext).ConfigureAwait(false);
			return result!;
		}
	}
}