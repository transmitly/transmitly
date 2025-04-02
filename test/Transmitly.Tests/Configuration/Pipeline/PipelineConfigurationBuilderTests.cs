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
using Transmitly.Tests;

namespace Transmitly.Pipeline.Configuration.Tests
{
	[TestClass()]
	public class PipelineConfiguratorTests
	{
		[TestMethod]
		public void PipelineConfiguratorShouldConfigureChannelConfig()
		{
			const string expectedPipelineName = "test-name";
			const string expectedPipelineCategory = "test-category";

			var config = new DefaultPipelineProviderConfiguration();
			var configurator = new Mock<BasePipelineConfigurator>()
			{
				CallBase = true
			};
			configurator.Setup(x => x.Name).Returns(expectedPipelineName);
			configurator.Setup(x => x.Category).Returns(expectedPipelineCategory);

			var channel = new UnitTestChannel("unit-test-address");
			configurator.Setup(x => x.Configure(It.IsAny<IPipelineConfiguration>())).Callback<IPipelineConfiguration>((config) =>
			{
				Assert.IsNotNull(config);
				config.AddChannel(channel);
				config.MessagePriority = MessagePriority.Highest;
				config.TransportPriority = TransportPriority.Lowest;
			}).Verifiable();

			configurator.Object.Configure(config);
			config.Build();

			Assert.AreEqual(1, config.ChannelRegistrations.Count);
			Assert.AreSame(channel, config.ChannelRegistrations.First().Channel);
			Assert.AreEqual(MessagePriority.Highest, config.MessagePriority);
			Assert.AreEqual(TransportPriority.Lowest, config.TransportPriority);
		}

		[TestMethod()]
		public async Task ShouldAddPipelineConfigurator()
		{
			const string expectedPipelineName = "test-name";
			const string expectedPipelineCategory = "test-category";
			const string expectedChannelProviderId = "test-channel-provider";
			const string expectedChannelId = "unit-test-channel";

			var configurator = new Mock<BasePipelineConfigurator>()
			{
				CallBase = true
			};

			configurator.Setup(x => x.Name).Returns(expectedPipelineName);

			configurator.Setup(x => x.Category).Returns(expectedPipelineCategory);
			configurator.Setup(x => x.Configure(It.IsAny<IPipelineConfiguration>())).Callback<IPipelineConfiguration>((config) =>
			{
				Assert.IsNotNull(config);
				config.AddChannel(new UnitTestChannel("unit-test-address"));
				config.MessagePriority = MessagePriority.Highest;
				config.TransportPriority = TransportPriority.Lowest;
			}).Verifiable();


			var builder = new CommunicationsClientBuilder().ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, object>(expectedChannelProviderId);
			var result = builder.AddPipelineConfigurator(configurator.Object);
			Assert.IsNotNull(result);
			Assert.AreSame(builder, result);
			var client = result.BuildClient();
			var sentResult = await client.DispatchAsync(expectedPipelineName, "unit-test-address", new { });
			Assert.AreEqual(1, sentResult.Results.Count);
			Assert.IsTrue(sentResult.IsSuccessful);
			Assert.IsTrue(sentResult.Results.All(x => x?.DispatchStatus == DispatchStatus.Dispatched));
			var deliveryResult = sentResult.Results.First();

			Assert.AreEqual(expectedChannelId, sentResult.Results?.First()?.ChannelId);
			Assert.AreEqual(expectedChannelProviderId, deliveryResult?.ChannelProviderId);
		}

		[TestMethod]
		public void FluentConfigurationTest()
		{
			var config = new DefaultPipelineProviderConfiguration();
			config
				.AddChannel(new UnitTestChannel("unit-test-address"))
					.ToAddress("1")
					.ToAddressPurpose("2")
				.AddEmail("test".AsIdentityAddress(), e => { })
				.AddSms("test-sms".AsIdentityAddress(), cfg => { })
					.ChannelProviderFilter("InfoBip")
					.CompleteOnDispatched()
				.Id("3")
				.AddPlatformIdentityTypeFilter("x");
			config.Build();
			Assert.AreEqual(3, config.ChannelRegistrations.Count);
			Assert.AreEqual("3", config.PipelineId);
		}
	}
}