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
public class InternetAddressTests
{
	[TestMethod()]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow(null)]
	public void ShouldRequireValue(string input)
	{
		Assert.ThrowsException<ArgumentNullException>(() => new IdentityAddress(input, "notnull"));
	}

	[TestMethod()]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow(null)]
	public void ShouldNotRequireDisplayValue(string input)
	{
		const string expectedValue = "value";
		var address = new IdentityAddress(expectedValue, input);
		Assert.AreEqual(expectedValue, address.Value);
		Assert.AreEqual(input, address.Display);
	}

	[TestMethod()]
	public void InternetAddressImplicitFromStringShouldSetValue()
	{
		const string expectedValue = "test";
		IdentityAddress address = expectedValue;
		Assert.AreEqual(expectedValue, address.Value);
		Assert.AreEqual(null, address.Display);
	}

	[TestMethod()]
	public void FromInternetAddressShouldImplicitCastToString()
	{
		const string expectedDisplay = "test";
		string? address = new IdentityAddress(expectedDisplay);

		Assert.AreEqual(expectedDisplay, address);
	}
}