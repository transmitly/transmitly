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

using Moq;
using Transmitly.Util;

namespace Transmitly.Template.Configuration.Tests;

[TestClass()]
public class DelegateContentTemplateRegistrationTests
{
	[TestMethod()]
	public void ShouldThrowIfDelegateNull()
	{
		Assert.ThrowsExactly<ArgumentNullException>(() => _ = new DelegateContentTemplateRegistration(null!));
	}

	[TestMethod()]
	public async Task ShouldReturnResourceContent()
	{
		const string expected = "OK";
		var template = new DelegateContentTemplateRegistration((context) =>
		{
			Guard.AgainstNull(context);
			return Task.FromResult<string?>(expected);
		});
		var context = new Mock<IDispatchCommunicationContext>();
		Assert.AreEqual(expected, await template.GetContentAsync(context.Object));
	}
}