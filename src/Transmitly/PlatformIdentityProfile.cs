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

using System.Collections.ObjectModel;

namespace Transmitly;

///<inheritdoc cref="IPlatformIdentityProfile"/>
public sealed class PlatformIdentityProfile : IPlatformIdentityProfile
{
	private ReadOnlyCollection<IPlatformIdentityAddress> _addresses;

	public PlatformIdentityProfile(string? id, string? type, IEnumerable<IPlatformIdentityAddress> identityAddresses)
	{
		Id = id;
		Type = type;
		Guard.AgainstNull(identityAddresses);
		_addresses = new ReadOnlyCollection<IPlatformIdentityAddress>([.. identityAddresses]);
	}

	public string? Id { get; set; }
	public string? Type { get; set; }
	public IReadOnlyCollection<IPlatformIdentityAddress> Addresses { get => _addresses; set => _addresses = new ReadOnlyCollection<IPlatformIdentityAddress>([.. value]); }
}