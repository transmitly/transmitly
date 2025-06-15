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
using Transmitly.Delivery;
using Transmitly.Pipeline.Configuration;

namespace Transmitly.Tests.Pipeline.Configuration;

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

		sut.UseDefaultPipelineDeliveryStrategy();
		Assert.AreEqual(expectedFirstMatchType, sut.PipelineDeliveryStrategyProvider.GetType());

		sut.UseAnyMatchPipelineDeliveryStrategy();
		Assert.AreEqual(expectedAnyMatchType, sut.PipelineDeliveryStrategyProvider.GetType());

		sut.UseDefaultPipelineDeliveryStrategy();
		Assert.AreEqual(expectedFirstMatchType, sut.PipelineDeliveryStrategyProvider.GetType());

		var customStrategy = new Mock<BasePipelineDeliveryStrategyProvider>();
		var customStrategyType = customStrategy.Object.GetType();
		PipelineDeliveryStrategyConfigurationExtensions.UsePipelineDeliveryStrategy(sut, customStrategy.Object);
		Assert.AreEqual(customStrategyType, sut.PipelineDeliveryStrategyProvider.GetType());
	}

	[TestMethod]
	public void IsDispatchChannelPriorityPreferenceAllowed_DefaultValueIsTrue()
	{
		var config = new DefaultPipelineProviderConfiguration();

		Assert.IsTrue(config.IsDispatchChannelPriorityPreferenceAllowed);
	}

	[TestMethod]
	public void AllowDispatchChannelPriorityPreference_SetsFalse_UpdatesPropertyCorrectly()
	{
		var config = new DefaultPipelineProviderConfiguration();

		var result = config.AllowDispatchChannelPriorityPreference(false);

		Assert.IsFalse(config.IsDispatchChannelPriorityPreferenceAllowed);
		Assert.AreSame(config, result); 
	}

	[TestMethod]
	public void AllowDispatchChannelPriorityPreference_SetsTrue_UpdatesPropertyCorrectly()
	{
		var config = new DefaultPipelineProviderConfiguration();
		config.AllowDispatchChannelPriorityPreference(false); 

		var result = config.AllowDispatchChannelPriorityPreference(true);

		Assert.IsTrue(config.IsDispatchChannelPriorityPreferenceAllowed);
		Assert.AreSame(config, result); 
	}

	[TestMethod]
	public void PipelineId_DefaultValueIsNull()
	{
		var config = new DefaultPipelineProviderConfiguration();

		Assert.IsNull(config.PipelineId);
	}

	[TestMethod]
	public void Id_WithValidValue_SetsPipelineIdCorrectly()
	{
		var config = new DefaultPipelineProviderConfiguration();
		const string expectedId = "test-pipeline";

		var result = config.Id(expectedId);

		Assert.AreEqual(expectedId, config.PipelineId);
		Assert.AreSame(config, result); 
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow("\t")]
	public void Id_WithInvalidValue_ThrowsArgumentException(string? invalidId)
	{
		var config = new DefaultPipelineProviderConfiguration();

		Assert.ThrowsExactly<ArgumentNullException>(() => config.Id(invalidId!));
	}

	[TestMethod]
	public void Id_CanBeUpdatedMultipleTimes()
	{
		var config = new DefaultPipelineProviderConfiguration();
		const string initialId = "initial-pipeline";
		const string updatedId = "updated-pipeline";

		config.Id(initialId);
		config.Id(updatedId);

		Assert.AreEqual(updatedId, config.PipelineId);
	}
}