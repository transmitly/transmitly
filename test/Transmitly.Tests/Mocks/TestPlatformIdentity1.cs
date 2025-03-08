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

namespace Transmitly.Tests
{
	internal sealed class TestChannelPreferences : IChannelPreference
	{
		public ChannelPreferenceType Type { get; set; } = ChannelPreferenceType.Default;

		public string? Category { get; set; }

		public IReadOnlyCollection<string> Channels { get; set; } = [];
	}


	internal sealed record TestPlatformIdentity1 : IPlatformIdentityProfile
	{
		public const string DefaultPlatformIdentityType = "test-identity-type";

		public TestPlatformIdentity1(string? id, string platformIdentityAddressType = DefaultPlatformIdentityType, IEnumerable<string>? channelPreferences = null)
		{
			Id = id ?? Guid.Empty.ToString();
			Type = Guard.AgainstNullOrWhiteSpace(platformIdentityAddressType);

			ChannelPreferences = [new TestChannelPreferences { Channels = [.. (channelPreferences ?? [])] }];
		}


		public TestPlatformIdentity1() : this(Guid.NewGuid(), DefaultPlatformIdentityType)
		{

		}


		public TestPlatformIdentity1(Guid? id, string platformIdentityType = DefaultPlatformIdentityType) : this(id?.ToString(), platformIdentityType, null)
		{

		}

		public IReadOnlyCollection<IIdentityAddress> Addresses { get; set; } = new List<IIdentityAddress>();
		public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
		public string? Id { get; set; }
		public string? Type { get; set; }
		public bool IsPersona { get; set; }
		public IReadOnlyCollection<IChannelPreference>? ChannelPreferences { get; set; }
	}
}