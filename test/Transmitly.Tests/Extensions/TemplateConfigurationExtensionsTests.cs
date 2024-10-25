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
using Transmitly.Template.Configuration;
using Transmitly.Exceptions;

namespace Transmitly.Tests
{
    [TestClass()]
    public class TemplateConfigurationExtensionsTests : BaseUnitTest
    {
        [TestMethod()]
        public void ShouldThrowIfCultureSpecificConfigurationIsRequired()
        {
            var config = fixture.Create<IContentTemplateConfiguration>();
            Assert.ThrowsException<CommunicationsException>(() => TemplateConfigurationExtensions.GetTemplateRegistration(config, System.Globalization.CultureInfo.GetCultureInfo("en-es"), true));
        }

        [TestMethod]
        public void ShouldThrowIfTemplateConfigIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => TemplateConfigurationExtensions.AddStringTemplate(null!, string.Empty));
            Assert.ThrowsException<ArgumentNullException>(() => TemplateConfigurationExtensions.AddEmbeddedResourceTemplate(null!, string.Empty));
            Assert.ThrowsException<ArgumentNullException>(() => TemplateConfigurationExtensions.AddTemplateResolver(null!, (ctx) => Task.FromResult<string?>(string.Empty)));
        }
    }
}