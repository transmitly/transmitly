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

namespace Transmitly
{
	/// <summary>
	/// Describes how to apply channel preferences.
	/// </summary>
	public enum ChannelPreferenceType
	{
		/// <summary>
		/// All of the channels are allowed but order is prioritized by the preferred order of the platform identity.
		/// </summary>
		Priority,
		/// <summary>
		/// Only the channels explicitly preferred by the platform identity are allowed.
		/// </summary>
		Filter,
		/// <summary>
		/// All of the channels are allowed but order is prioritized by the preferred order of the platform identity.
		/// </summary>
		Default = Priority
	}
}
