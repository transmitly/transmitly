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
	public class DispatchPlatformIdentityAddress(string value) : IPlatformIdentityAddress
	{
		public string Value { get; set; } = value;
		public string? Display { get; set; }
		public string? Type { get; set; } = null;
		public IDictionary<string, string?> AddressParts { get; set; } = new Dictionary<string, string?>();
		public IDictionary<string, string?> Attributes { get; set; } = new Dictionary<string, string?>();
		public IReadOnlyCollection<string>? Purposes { get; set; } = new List<string>();
	}
}