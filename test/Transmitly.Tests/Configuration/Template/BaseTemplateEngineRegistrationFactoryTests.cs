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

using Transmitly.Tests;
using Moq;

namespace Transmitly.Template.Configuration.Tests
{
	[TestClass()]
	public class BaseTemplateEngineRegistrationFactoryTests
	{
		private class TestTemplateEngineRegistrationFactory(IEnumerable<ITemplateEngineRegistration> registrations) : BaseTemplateEngineRegistrationFactory(registrations)
		{
			public override ITemplateEngine Get() => Registrations.First().Instance;
		}

		[TestMethod()]
		public void BaseTemplateEngineFactoryEnforcesSingleTemplateRegistration()
		{
			var engine1Mock = new Mock<ITemplateEngineRegistration>();
			engine1Mock.SetupGet(x => x.Instance).Returns(new MockTemplateEngine1()).Verifiable();
			var engine2Mock = new Mock<ITemplateEngineRegistration>();
			engine2Mock.SetupGet(x => x.Instance).Returns(new MockTemplateEngine1()).Verifiable();

			TestTemplateEngineRegistrationFactory? instance = null;
			Assert.ThrowsExactly<NotSupportedException>(() => instance = new TestTemplateEngineRegistrationFactory([]));

			instance = null;
			void create() => instance = new TestTemplateEngineRegistrationFactory([engine1Mock.Object, engine2Mock.Object]);
			Assert.ThrowsExactly<NotSupportedException>(create);

			var engine = new TestTemplateEngineRegistrationFactory([engine1Mock.Object]);
			Assert.IsNotNull(engine.Get());
		}
	}
}