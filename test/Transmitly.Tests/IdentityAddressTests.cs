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

namespace Transmitly.Tests;

[TestClass]
public class IdentityAddressTests
{
	[TestMethod]
	public void IdentityAddressEquality()
	{
		var one = new PlatformIdentityAddress("test1@transmit.ly", "test");
		var two = new PlatformIdentityAddress("test1@transmit.ly", "Test");
		var three = new PlatformIdentityAddress("test2@transmit.ly", "test");
		var four = new PlatformIdentityAddress("test2@transmit.ly", "Test");

		Assert.IsTrue(one.Equals(one));
		Assert.IsTrue(one.Equals(two));
		Assert.IsFalse(one.Equals(three));
		Assert.IsFalse(one.Equals(four));
	}
}