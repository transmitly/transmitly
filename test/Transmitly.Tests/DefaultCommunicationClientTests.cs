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
using Transmitly.ChannelProvider;
using Transmitly.ChannelProvider.Configuration;
using Transmitly.Pipeline.Configuration;
using Transmitly.Settings.Configuration;
using Transmitly.Template.Configuration;

namespace Transmitly.Tests
{
	[TestClass]
	public class DefaultCommunicationClientTests
	{


		[TestMethod]
		[DataRow("")]
		[DataRow(" ")]
		[DataRow(null)]
		public void ShouldGuardEmptyPipelineName(string value)
		{
			var (settings, pipeline, channelProvider, template, reportHandler) = GetStores();

			var client = new DefaultCommunicationsClient(settings.Object, pipeline.Object, channelProvider.Object, template.Object, reportHandler.Object/*, audience.Object*/);
			Assert.ThrowsExceptionAsync<ArgumentNullException>(() => client.DispatchAsync(value, "test", new { }));
		}

		//[TestMethod]
		//public async Task ShouldThrowIfNoSupportAudienceAddressAreProvided()
		//{
		//	const string expectedPass = "PASS";
		//	var store = GetStores();
		//	var pipeline = new Mock<IPipeline>();
		//	var pipelineConfig = new Mock<IPipelineChannelConfiguration>();
		//	var channel = new Mock<IChannel>();

		//	channel.Setup(x => x.SupportsAudienceAddress(It.IsAny<IAudienceAddress>())).Returns<IAudienceAddress>((aud) => aud.Value != expectedPass);
		//	pipelineConfig.Setup(x => x.Channels).Returns(() => [channel.Object]);
		//	pipeline.Setup(x => x.ChannelConfiguration).Returns(pipelineConfig.Object);
		//	store.pipeline.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(pipeline.Object);

		//	var client = new DefaultCommunicationsClient(store.settings.Object, store.pipeline.Object, store.channelProvider.Object, store.template.Object, store.deliveryReportHandler.Object);

		//	await Assert.ThrowsExceptionAsync<CommunicationsException>(() => client.DispatchAsync("test", expectedPass, new { }));
		//}

		[TestMethod]
		public async Task ShouldSendBasedOnAllowedChannelProviderRestrictions()
		{
			const string ChannelProvider0 = "channel-provider-0";
			const string ChannelProvider1 = "channel-provider-1";
			const string ChannelId = "unit-test-channel";
			var client = new CommunicationsClientBuilder()
					.ChannelProvider.Add(ChannelProvider0, new MinimalConfigurationTestChannelProviderClient(), "unit-test-channel")
					.ChannelProvider.Add(ChannelProvider1, new MinimalConfigurationTestChannelProviderClient(), "unit-test-channel")
					.Pipeline.Add("test-pipeline", options =>
					{
						options.AddChannel(new UnitTestChannel("c0-from", ChannelId, ChannelProvider0));
						options.AddChannel(new UnitTestChannel("c1-from", ChannelId, ChannelProvider1));
						options.AddChannel(new UnitTestChannel("c2-from", "channel-not-included", ChannelProvider0));
					})
					.BuildClient();
			var result = await client.DispatchAsync("test-pipeline", "unit-test-address-0", new { });
			Assert.AreEqual(2, result.Results.Count);
			Assert.AreEqual(ChannelId, result.Results.First().ChannelId);
			Assert.AreEqual(ChannelId, result.Results.Skip(1).First().ChannelId);
			Assert.AreEqual(ChannelProvider0, result.Results.First().ChannelProviderId);
			Assert.AreEqual(ChannelProvider1, result.Results.Skip(1).First().ChannelProviderId);
		}

		private static (
			Mock<ICommunicationsConfigurationSettings> settings,
			Mock<IPipelineRegistrationStore> pipeline,
			Mock<IChannelProviderRegistrationStore> channelProvider,
			Mock<ITemplateEngineRegistrationStore> template,
			Mock<IDeliveryReportProvider> deliveryReportHandler) GetStores()
		{
			return (
				new Mock<ICommunicationsConfigurationSettings>(),
				new Mock<IPipelineRegistrationStore>(),
				new Mock<IChannelProviderRegistrationStore>(),
				new Mock<ITemplateEngineRegistrationStore>(),
				new Mock<IDeliveryReportProvider>()
			);
		}

	}
}