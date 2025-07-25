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

using Transmitly.PlatformIdentity.Configuration;

namespace Transmitly.Tests;

internal sealed class MockPlatformIdentityRepository : IPlatformIdentityResolver
{
	private static bool _isFirst = true;
	public Task<IReadOnlyCollection<IPlatformIdentityProfile>?> ResolveIdentityProfiles(IReadOnlyCollection<IPlatformIdentityReference> identityReferences)
	{
		var results = new List<IPlatformIdentityProfile>();
		foreach (var refs in identityReferences)
		{
			if (Guid.TryParse(refs.Id, out var parsedId))
			{
				results.Add(new MockPlatformIdentity1(parsedId) { IsPersona = _isFirst, Addresses = new List<PlatformIdentityAddress>() { new("unit-test-address"), new("unit-test-address2"), new("xxx") } });
				_isFirst = false;
			}
		}
		return Task.FromResult<IReadOnlyCollection<IPlatformIdentityProfile>?>(results.AsReadOnly());
	}
}