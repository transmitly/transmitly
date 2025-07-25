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

using Transmitly.Util;

namespace Transmitly.Tests;

internal sealed record MockPlatformIdentity1 : IPlatformIdentityProfile
{
	public string SomeValue => Id + "_Test";
	public const string DefaultPlatformIdentityType = "test-identity-type";

	public MockPlatformIdentity1(string? id, string platformIdentityAddressType = DefaultPlatformIdentityType)
	{
		Id = id ?? Guid.Empty.ToString();
		Type = Guard.AgainstNullOrWhiteSpace(platformIdentityAddressType);
	}


	public MockPlatformIdentity1() : this(Guid.NewGuid(), DefaultPlatformIdentityType)
	{

	}


	public MockPlatformIdentity1(Guid? id, string platformIdentityType = DefaultPlatformIdentityType) : this(id?.ToString(), platformIdentityType)
	{

	}

	public IReadOnlyCollection<IPlatformIdentityAddress> Addresses { get; set; } = new List<IPlatformIdentityAddress>();
	public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
	public string? Id { get; set; }
	public string? Type { get; set; }
	public bool IsPersona { get; set; }
}