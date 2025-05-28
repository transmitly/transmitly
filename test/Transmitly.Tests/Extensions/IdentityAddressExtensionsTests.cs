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

[TestClass()]
public class IdentityAddressExtensionsTests
{
	[TestMethod()]
	public void ShouldThrowIfAddressIsnNull()
	{
		Assert.ThrowsExactly<ArgumentNullException>(() => ((IIdentityAddress?)null!).IsType("fail"));
	}

	[TestMethod()]
	public void MatchShouldBeCaseInsensitive()
	{
		var expectedType = "unit-test";
		var sut = new IdentityAddress("test", type: expectedType);
		Assert.IsTrue(sut.IsType(expectedType));
		Assert.IsTrue(sut.IsType(expectedType.ToUpper()));
		Assert.IsTrue(sut.IsType(expectedType.ToLower()));
	}

	[TestMethod]
	public void ShouldReturnDefaultValue()
	{
		var expectedType = "unit-test";
		var expectedValue = "pass";
		var sut = new IdentityAddress("test", type: expectedType);
		Assert.AreEqual(expectedValue, sut.IfType(expectedType, expectedValue));
		Assert.AreNotEqual("anything-else", sut.IfType(expectedType, expectedValue));
	}
}