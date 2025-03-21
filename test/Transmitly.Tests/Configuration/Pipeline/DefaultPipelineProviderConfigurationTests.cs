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
using Transmitly.Delivery;
using Transmitly.Pipeline.Configuration;

namespace Transmitly.Tests.Configuration.Pipeline
{
	[TestClass]
	public class DefaultPipelineProviderConfigurationTests
	{
		[TestMethod]
		public void DefaultPipelineStrategyShouldBeFirstMatch()
		{
			var expectedType = typeof(FirstMatchPipelineDeliveryStrategy);
			var sut = new DefaultPipelineProviderConfiguration();
			Assert.AreEqual(expectedType, sut.PipelineDeliveryStrategyProvider.GetType());
		}

		[TestMethod]
		public void ExtensionsShouldSetExpectedStrategyProvider()
		{
			var expectedFirstMatchType = typeof(FirstMatchPipelineDeliveryStrategy);
			var expectedAnyMatchType = typeof(AnyMatchPipelineDeliveryStrategy);

			var sut = new DefaultPipelineProviderConfiguration();
			Assert.AreEqual(expectedFirstMatchType, sut.PipelineDeliveryStrategyProvider.GetType());

			PipelineDeliveryStrategyConfigurationExtensions.UseDefaultPipelineDeliveryStrategy(sut);
			Assert.AreEqual(expectedFirstMatchType, sut.PipelineDeliveryStrategyProvider.GetType());

			PipelineDeliveryStrategyConfigurationExtensions.UseAnyMatchPipelineDeliveryStrategy(sut);
			Assert.AreEqual(expectedAnyMatchType, sut.PipelineDeliveryStrategyProvider.GetType());

			PipelineDeliveryStrategyConfigurationExtensions.UseDefaultPipelineDeliveryStrategy(sut);
			Assert.AreEqual(expectedFirstMatchType, sut.PipelineDeliveryStrategyProvider.GetType());

			var customStrategy = new Mock<BasePipelineDeliveryStrategyProvider>();
			var customStrategyType = customStrategy.Object.GetType();
			PipelineDeliveryStrategyConfigurationExtensions.UsePipelineDeliveryStrategy(sut, customStrategy.Object);
			Assert.AreEqual(customStrategyType, sut.PipelineDeliveryStrategyProvider.GetType());
		}
	}
}
