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

using Transmitly.Pipeline.Configuration;

namespace Transmitly
{
	/// <summary>
	/// Describes the channel preferences for a platform identity.
	/// </summary>
	public interface IChannelPreference
	{
		/// <summary>
		/// Optional <see cref="IPipeline.Category">pipeline category</see> the channel preferences are applied to. 
		/// When empty will apply to all pipelines.
		/// </summary>
		string? Category { get; }
		/// <summary>
		/// Optional <see cref="IPipeline.CommunicationIntentId">communication intent id</see> the channel preferences are applied to.
		/// </summary>
		string? CommunicationIntentId { get; }
		/// <summary>
		/// An ordered list of channel id preferences and opt in to the provided channels. 
		/// If <see cref="Category"/> and <see cref="CommunicationIntentId"/> are not provided,
		/// the preferences and opt in will apply to all pipelines.
		/// </summary>
		IReadOnlyCollection<string> ChannelIds { get; }
	}
}