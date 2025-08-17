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

namespace Transmitly.KitchenSink.AspNetCoreWebApi.Controllers
{
	public class DispatchPlatformIdentityProfile : IPlatformIdentityProfile
	{
		public string? Id { get; set; }
		public string? Type { get; set; }
                public List<DispatchPlatformIdentityAddress> Addresses { get; set; } = [];
                IReadOnlyCollection<IPlatformIdentityAddress> IPlatformIdentityProfile.Addresses { get => Addresses.AsReadOnly(); }
                public List<string> ChannelPreferences { get; set; } = [];
                IReadOnlyCollection<string> IPlatformIdentityProfile.ChannelPreferences { get => ChannelPreferences.AsReadOnly(); }
                public IDictionary<string, string>? Attributes { get; set; } = null;
        }
}