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
using Transmitly.Exceptions;

namespace Transmitly.Template.Configuration.Tests;

[TestClass()]
public class EmbeddedResourceContentTemplateRegistrationTests
{
#pragma warning disable CS8604 // Possible null reference argument.
	[TestMethod()]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow(null)]
	public void ShouldThrowIfResourceIdIsEmpty(string? value)
	{
		EmbeddedResourceContentTemplateRegistration? instance = null;
		void create() => instance = new EmbeddedResourceContentTemplateRegistration(value);
		Assert.ThrowsExactly<ArgumentNullException>(create);
	}

	[TestMethod]
	public void ShouldThrowIfResourceCannotBeFound()
	{
		EmbeddedResourceContentTemplateRegistration? instance = null;
		void create() => instance = new EmbeddedResourceContentTemplateRegistration("test", resourceAssembly: typeof(EmbeddedResourceContentTemplateRegistrationTests).Assembly);
		Assert.ThrowsExactly<CommunicationsException>(create);
	}

#pragma warning restore CS8604 // Possible null reference argument.

	[TestMethod()]
	public async Task ShouldReturnEmbeddedResourceContent()
	{
		var template = new EmbeddedResourceContentTemplateRegistration("Transmitly.Tests.EmbeddedResources.has-content.txt", null, typeof(EmbeddedResourceContentTemplateRegistrationTests).Assembly);
		var context = new Mock<IDispatchCommunicationContext>();
		Assert.AreEqual("has content!", await template.GetContentAsync(context.Object));
	}
}