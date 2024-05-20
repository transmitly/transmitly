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
	internal sealed class TestPlatformIdentityRepository
	{
#pragma warning disable CA1822 // Mark members as static
		public Task<TestPlatformIdentity1?> GetCustomerAsync(Guid? id)
#pragma warning restore CA1822 // Mark members as static
		{
			if (!id.HasValue || id == Guid.Empty)
				return Task.FromResult<TestPlatformIdentity1?>(null);
			return Task.FromResult<TestPlatformIdentity1?>(new TestPlatformIdentity1(id.Value));
		}
	}
}