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

namespace Transmitly
{
	/// <summary>
	/// See <see cref="Id"/>
	/// </summary>
	//[DebuggerStepThrough]
	public sealed class ChannelProviders
	{
		private const string DefaultProviderId = "";

		internal ChannelProviders() { }

#pragma warning disable CA1822 // Mark members as static
		public string GetId(string providerId, string? clientId = DefaultProviderId)
#pragma warning restore CA1822 // Mark members as static
		{
			Guard.AgainstNullOrWhiteSpace(providerId);
			return string.Join(".", providerId, !string.IsNullOrWhiteSpace(clientId) ? clientId : DefaultProviderId).Trim('.');
		}
	}
}
