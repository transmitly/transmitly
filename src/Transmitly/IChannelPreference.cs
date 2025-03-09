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
		///// <summary>
		///// How the channel preferences should be applied.
		///// </summary>
		//ChannelPreferenceType Type { get; }

		/// <summary>
		/// Optional <see cref="IPipeline.Category">pipeline category</see> the channel preferences are tied to. 
		/// When empty will apply to all pipelines.
		/// </summary>
		string? Category { get; }
		/// <summary>
		/// Optional communication intent id the channel preferences are tied to.
		/// </summary>
		string? CommunicationIntentId { get; }

		///// <summary>
		///// An ordered list of channel id preferences. If a category is not provided 
		///// the preferences will apply to all pipeline categories.
		///// </summary>
		//IReadOnlyCollection<string> Channels { get; }


		/// <summary>
		/// An ordered list of channel id preferences. If a category is not provided 
		/// the preferences will apply to all pipeline categories.
		/// </summary>
		IReadOnlyCollection<string> OptInChannelWithPriorities { get; }
	}

}