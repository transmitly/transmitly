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

namespace Transmitly.Template.Configuration.Tests;

[TestClass()]
public class LocalFileContentTemplateRegistrationTests
{
	[TestMethod()]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow(null)]
	public void ShouldThrowIfResourceIdIsEmpty(string? value)
	{
		LocalFileContentTemplateRegistration? instance = null;
#pragma warning disable CS8604 // Possible null reference argument.
		void create() => instance = new LocalFileContentTemplateRegistration(value);
#pragma warning restore CS8604 // Possible null reference argument.
		Assert.ThrowsExactly<ArgumentNullException>(create);

	}

	[TestMethod]
	public void ShouldThrowIfResourceCannotBeFound()
	{
		LocalFileContentTemplateRegistration? instance = null;
		void create() => instance = new LocalFileContentTemplateRegistration("not-exist.txt");
		Assert.ThrowsExactly<FileNotFoundException>(create);
	}

	[TestMethod]
	public async Task ShouldNotThrowIfResourceCannotBeFound()
	{
		var template = new LocalFileContentTemplateRegistration("not-exist.txt", false);
		var context = new Mock<IDispatchCommunicationContext>();
		Assert.IsNull(await template.GetContentAsync(context.Object));
	}

	[TestMethod()]
	[DeploymentItem(@"FileResource/file-content.txt")]
	[DeploymentItem(@"FileResource\file-content.txt")]
	public async Task ShouldReturnFileResourceContent()
	{
		var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileResource", "file-content.txt");
#if NETFRAMEWORK
		// Some .NET Framework test runners may not honor DeploymentItem.
		// Create file manually if not present.

		if (!File.Exists(filePath))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(filePath));
			File.WriteAllText(filePath, "OK");
		}
#endif
		var template = new LocalFileContentTemplateRegistration(filePath);
		var context = new Mock<IDispatchCommunicationContext>();
		Assert.AreEqual("OK", await template.GetContentAsync(context.Object));
	}
}