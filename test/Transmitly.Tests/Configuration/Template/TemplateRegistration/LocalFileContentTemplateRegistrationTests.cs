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

namespace Transmitly.Template.Configuration.Tests
{
	[TestClass()]
	public class LocalFileContentTemplateRegistrationTests
	{
		[TestMethod()]
		[DataRow("")]
		[DataRow(" ")]
		[DataRow(null)]
		public void ShouldThrowIfResourceIdIsEmpty(string? value)
		{
#pragma warning disable CS8604 // Possible null reference argument.
			Assert.ThrowsException<ArgumentNullException>(() => new LocalFileContentTemplateRegistration(value));
#pragma warning restore CS8604 // Possible null reference argument.
		}

		[TestMethod]
		public void ShouldThrowIfResourceCannotBeFound()
		{
			Assert.ThrowsException<FileNotFoundException>(() => new LocalFileContentTemplateRegistration("not-exist.txt"));
		}

		[TestMethod]
		public async Task ShouldNotThrowIfResourceCannotBeFound()
		{
			var template = new LocalFileContentTemplateRegistration("not-exist.txt", false);
			var context = new Mock<IDispatchCommunicationContext>();
			Assert.IsNull(await template.GetContentAsync(context.Object));
		}

#if !NETFRAMEWORK //todo: look into why this isn't working for NETFramework
		[TestMethod()]
		[DeploymentItem(@"FileResource/file-content.txt")]
		[DeploymentItem(@"FileResource\file-content.txt")]
		public async Task ShouldReturnEmbeddedResourceContent()
		{
			var template = new LocalFileContentTemplateRegistration($"FileResource{Path.DirectorySeparatorChar}file-content.txt");
			var context = new Mock<IDispatchCommunicationContext>();
			Assert.AreEqual("OK", await template.GetContentAsync(context.Object));
		}
#endif
	}
}