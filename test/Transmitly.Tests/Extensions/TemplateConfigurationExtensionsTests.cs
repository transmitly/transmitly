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

using AutoFixture;
using Transmitly.Exceptions;
using Transmitly.Template.Configuration;

namespace Transmitly.Tests;

[TestClass()]
public class TemplateConfigurationExtensionsTests : BaseUnitTest
{
	[TestMethod()]
	public void ShouldThrowIfCultureSpecificConfigurationIsRequired()
	{
		var config = fixture.Create<IContentTemplateConfiguration>();
		Assert.ThrowsExactly<CommunicationsException>(() => TemplateConfigurationExtensions.GetTemplateRegistration(config, System.Globalization.CultureInfo.GetCultureInfo("en-es"), true));
	}

	[TestMethod]
	public void ShouldThrowIfTemplateConfigIsNull()
	{
		Assert.ThrowsExactly<ArgumentNullException>(() => TemplateConfigurationExtensions.AddStringTemplate(null!, string.Empty));
		Assert.ThrowsExactly<ArgumentNullException>(() => TemplateConfigurationExtensions.AddEmbeddedResourceTemplate(null!, string.Empty));
		Assert.ThrowsExactly<ArgumentNullException>(() => TemplateConfigurationExtensions.AddTemplateResolver(null!, (ctx) => Task.FromResult<string?>(string.Empty)));
	}

	[TestMethod]
	public async Task ShouldMatchTemplateCultureInfo()
	{
		var context = fixture.Create<IDispatchCommunicationContext>();
		var esContent = fixture.Freeze<string>();
		var config = new ContentTemplateConfiguration()
			.AddStringTemplate("invariant")
			.AddStringTemplate(esContent, "es-MX");

		var content = config.GetTemplateRegistration(new System.Globalization.CultureInfo("es-MX"), true);
		Assert.IsNotNull(content);
		var templateContent = await content.GetContentAsync(context);
		Assert.AreEqual(esContent, templateContent);


	}
}