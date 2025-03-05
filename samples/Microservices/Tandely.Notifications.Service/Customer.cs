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

using Transmitly;
using Transmitly.Channel.Push;

namespace Tandely.Notifications.Service
{
	sealed class Customer : IPlatformIdentity
	{
		public Guid Id { get; set; }
		public string? EmailAddress { get; set; }
		public string? MobilePhone { get; set; }
		public string? DeviceToken { get; set; }
		public string? Name { get => $"{FirstName} {LastName}"; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public int LoyaltyPoints { get; set; }
		public IReadOnlyCollection<string> ChannelPreferences { get; set; } = [];


		string? IPlatformIdentity.Id { get => Id.ToString();  }
		string? IPlatformIdentity.Type { get => nameof(Customer); }
		IReadOnlyCollection<IIdentityAddress> IPlatformIdentity.Addresses
		{
			get
			{
				var result = new List<IIdentityAddress>();
				if (!string.IsNullOrWhiteSpace(MobilePhone))
					result.Add(new IdentityAddress(MobilePhone));
				if (!string.IsNullOrWhiteSpace(EmailAddress))
					result.Add(new IdentityAddress(EmailAddress, Name));
				if (!string.IsNullOrWhiteSpace(DeviceToken))
					result.Add(new IdentityAddress(DeviceToken, type: IdentityAddress.Types.DeviceToken()));
				return result;
			}
		}
	}
}
