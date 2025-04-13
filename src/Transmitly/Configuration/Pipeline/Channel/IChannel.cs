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

namespace Transmitly.Channel.Configuration
{
	public interface IChannel
	{
		/// <summary>
		/// Gets the concrete communication type.
		/// </summary>
		Type CommunicationType { get; }
		/// <summary>
		/// Gets the Id of the channel.
		/// </summary>
		string Id { get; }
		/// <summary>
		/// Gets the allowed channel providers.
		/// </summary>
		IEnumerable<string> AllowedChannelProviderIds { get; }
		/// <summary>
		/// Checks if the channel supports the given internet address.
		/// </summary>
		/// <param name="identityAddress">The identity address to check.</param>
		/// <returns>True if the channel supports the address; otherwise, false.</returns>
		bool SupportsIdentityAddress(IIdentityAddress identityAddress);
		/// <summary>
		/// Generates a channel communication given the provided dispatch context.
		/// </summary>
		/// <param name="communicationContext">The communication context.</param>
		/// <returns>The generated communication.</returns>
		Task<object> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext);
		/// <summary>
		/// Gets the extended properties for the channel.
		/// </summary>
		IExtendedProperties ExtendedProperties { get; }
	}
}