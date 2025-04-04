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
using Transmitly.Tests;

namespace Transmitly.Pipeline.Configuration.Tests
{
	[TestClass()]
	public class PipelineConfiguratorTests
	{
		private DefaultPipelineProviderConfiguration config;
		[TestInitialize]
		public void Setup()
		{
			config = new DefaultPipelineProviderConfiguration();
		}

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
		public void BuildShouldSetPipelineIdAndPlatformIdentityTypeFilter()
		{
			var expectedPipelineId = "ExpectedPipelineId";
			var expectedPlatformIdentityTypeFilter = "ExpectedPlatformIdentityFilter";
			config.Id(expectedPipelineId)
				  .AddPlatformIdentityTypeFilter(expectedPlatformIdentityTypeFilter);

			config.Build();

			Assert.AreEqual(expectedPipelineId, config.PipelineId, "PipelineId was not set correctly.");
			Assert.AreEqual(expectedPlatformIdentityTypeFilter, config.PlatformIdentityTypeFilters.First(), "PlatformIdentityTypeFilter was not set correctly.");
		}

		[TestMethod]
		public void AddChannelShouldConfigureToAddressAndToAddressPurpose()
		{
			var expectedToAddress = "ExpectedToAddress";
			var expectedToAddressPurpose = "ExpectedToAddressPurpose";

			var channelBuilder = config.AddChannel(new UnitTestChannel("unit-test-address"));
			channelBuilder.ToAddress(expectedToAddress)
						  .ToAddressPurpose(expectedToAddressPurpose);

			config.Build();

			var channel = config.ChannelRegistrations.FirstOrDefault(c => c.ToAddress == expectedToAddress);
			Assert.IsNotNull(channel, "Channel registration with the expected ToAddress was not found.");
			Assert.AreEqual(expectedToAddress, channel.ToAddress, "ToAddress was not configured correctly.");
			Assert.AreEqual(expectedToAddressPurpose, channel.ToAddressPurpose, "ToAddressPurpose was not configured correctly.");
		}

		[TestMethod]
		public void AddSmsShouldApplyChannelProviderFilterAndDispatchSetting()
		{
			var expectedChannelProviderFilter = "ExpectedProviderFilter";

			config.AddEmail("test".AsIdentityAddress(), e => { });

			var smsBuilder = config.AddSms("test-sms".AsIdentityAddress(), cfg => { });
			smsBuilder.ChannelProviderFilter(expectedChannelProviderFilter)
					  .CompleteOnDispatched();

			config.Build();

			var smsChannel = config.ChannelRegistrations.FirstOrDefault(c => c.FilterChannelProviderIds.Count > 0);
			Assert.IsNotNull(smsChannel, "SMS channel with a provider filter was not found.");
			Assert.AreEqual(expectedChannelProviderFilter, smsChannel.FilterChannelProviderIds.First(), "ChannelProviderFilter was not set correctly on the SMS channel.");
		}

		[TestMethod]
		public void ChannelRegistrationsShouldHaveExpectedCountAfterBuild()
		{
			config.AddChannel(new UnitTestChannel("unit-test-address"))
				  .ToAddress("Address1")
				  .ToAddressPurpose("Purpose1");
			config.AddEmail("email@test.com".AsIdentityAddress(), e => { });
			config.AddSms("sms@test.com".AsIdentityAddress(), cfg => { })
				  .ChannelProviderFilter("ProviderFilter")
				  .CompleteOnDispatched();

			config.Build();

			var expectedChannelRegistrationCount = 3;
			Assert.AreEqual(expectedChannelRegistrationCount, config.ChannelRegistrations.Count, "The number of channel registrations is not as expected.");
		}

		[TestMethod]
		public void ConfigurationShouldBeImmutableAfterBuild()
		{
			config.Id("InitialPipelineId");
			config.Build();

			Assert.ThrowsExactly<InvalidOperationException>(() => config.Id("AnotherPipelineId"), "Configuration should be immutable after Build is called.");
			Assert.ThrowsExactly<InvalidOperationException>(() => config.AddPlatformIdentityTypeFilter("AnotherPipelineId"), "Configuration should be immutable after Build is called.");
			Assert.ThrowsExactly<InvalidOperationException>(() => config.AddPersonaFilter("AnotherPipelineId"), "Configuration should be immutable after Build is called.");
			Assert.ThrowsExactly<InvalidOperationException>(() => config.UsePipelineDeliveryStrategy(new AnyMatchPipelineDeliveryStrategy()), "Configuration should be immutable after Build is called.");
			Assert.ThrowsExactly<InvalidOperationException>(() => config.AddChannel(new UnitTestChannel("test")), "Configuration should be immutable after Build is called.");
			Assert.ThrowsExactly<InvalidOperationException>(() => config.Build(), "Configuration should be immutable after Build is called.");
		}
	}
}