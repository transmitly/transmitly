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

using Moq;

namespace Transmitly.Tests.Pipeline.Configuration.Channel.Email;

[TestClass]
public class EmailChannelConfigurationExtensionsTests
{
	[TestMethod]
	public void ToEmailAddress_WithNullIdentityAddress_ThrowsArgumentNullException()
	{
		Assert.ThrowsExactly<ArgumentNullException>(() => ((IPlatformIdentityAddress)null!).ToEmailAddress());
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	public void ToEmailAddress_WithNullOrWhitespaceValue_ThrowsArgumentNullException(string? value)
	{
		var identityAddress = new Mock<IPlatformIdentityAddress>();
		identityAddress.SetupGet(address => address.Value).Returns(value!);

		Assert.ThrowsExactly<ArgumentNullException>(() => identityAddress.Object.ToEmailAddress());
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	public void ToEmailAddress_WithNullOrEmptyDisplay_ReturnsValue(string? display)
	{
		const string expectedValue = "test@transmit.ly";
		var identityAddress = new PlatformIdentityAddress(expectedValue, display);

		var result = identityAddress.ToEmailAddress();

		Assert.AreEqual(expectedValue, result);
	}

	[TestMethod]
	public void ToEmailAddress_WithDisplay_ReturnsFormattedAddress()
	{
		const string expectedValue = "test@transmit.ly";
		const string expectedDisplay = "Transmitly";
		const string expected = "Transmitly <test@transmit.ly>";
		var identityAddress = new PlatformIdentityAddress(expectedValue, expectedDisplay);

		var result = identityAddress.ToEmailAddress();

		Assert.AreEqual(expected, result);
	}
}
