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

namespace Transmitly.ChannelProvider.Configuration
{
	/// <summary>
	/// Represents a channel provider.
	/// </summary>
	public interface IChannelProvider
	{
		/// <summary>
		/// Gets the ID of the channel provider.
		/// </summary>
		string Id { get; }
		/// <summary>
		/// Checks if the channel provider supports the specified channel.
		/// </summary>
		/// <param name="channel">The channel to check.</param>
		/// <returns>True if the channel is supported, otherwise false.</returns>
		bool SupportsChannel(string channel);

		bool SupportAudienceAddress(IAudienceAddress audienceAddress);
		/// <summary>
		/// Gets the client of the channel provider.
		/// </summary>
		IChannelProviderClient GetClient();
	}
}