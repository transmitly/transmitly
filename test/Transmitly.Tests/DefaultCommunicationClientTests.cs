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
using Transmitly.Tests.Integration;

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

			var client = new DefaultCommunicationsClient(pipeline.Object, channelProvider.Object, template.Object, settings.Object, reportHandler.Object/*, audience.Object*/);
			Assert.ThrowsExceptionAsync<ArgumentNullException>(() => client.DispatchAsync(value, "test", new { }));
		}

		[TestMethod]
		public async Task ShouldSendBasedOnAllowedChannelProviderRestrictions()
		{
			const string ChannelProvider0 = "channel-provider-0";
			const string ChannelProvider1 = "channel-provider-1";
			const string ChannelId = "unit-test-channel";
			var client = new CommunicationsClientBuilder()
					.AddChannelProvider<MinimalConfigurationTestChannelProviderClient, object>(ChannelProvider0, "unit-test-channel")
					.AddChannelProvider<MinimalConfigurationTestChannelProviderClient, object>(ChannelProvider1, "unit-test-channel")
					.AddPipeline("test-pipeline", options =>
					{
						options.AddChannel(new UnitTestChannel("c0-from", ChannelId, ChannelProvider0));
						options.AddChannel(new UnitTestChannel("c1-from", ChannelId, ChannelProvider1));
						options.AddChannel(new UnitTestChannel("c2-from", "channel-not-included", ChannelProvider0));
					})
					.BuildClient();
			var result = await client.DispatchAsync("test-pipeline", "unit-test-address-0", new { });
			Assert.IsTrue(result.IsSuccessful);
			Assert.AreEqual(1, result.Results.Count);
			Assert.AreEqual(ChannelId, result.Results?.First()?.ChannelId);
			Assert.AreEqual(ChannelProvider0, result.Results?.First()?.ChannelProviderId);

		}

		private static (
			Mock<ICommunicationsConfigurationSettings> settings,
			Mock<IPipelineFactory> pipeline,
			Mock<IChannelProviderFactory> channelProvider,
			Mock<ITemplateEngineFactory> template,
			Mock<IDeliveryReportProvider> deliveryReportHandler) GetStores()
		{
			return (
				new Mock<ICommunicationsConfigurationSettings>(),
				new Mock<IPipelineFactory>(),
				new Mock<IChannelProviderFactory>(),
				new Mock<ITemplateEngineFactory>(),
				new Mock<IDeliveryReportProvider>()
			);
		}

		[TestMethod]
		public async Task ShouldDispatchUsingCorrectChannelProviderClient()
		{
			var tly = new CommunicationsClientBuilder();

			tly.AddChannelProvider<MinimalConfigurationTestChannelProviderClient, ISms>("test-channel-provider");
			tly.AddChannelProvider<MinimalConfigurationTestChannelProviderClient, IEmail>("test-channel-provider");
			tly.AddChannelProvider<OptionalConfigurationTestChannelProviderClient, UnitTestCommunication>("test-channel-provider");

			tly.AddPipeline("test-pipeline", options =>
			{
				options.AddEmail("from@address.com".AsAudienceAddress(), email =>
				{
					email.Subject.AddStringTemplate("Test sub");
				});
			});

			var client = tly.BuildClient();
			Assert.IsNotNull(client);
			var result = await client.DispatchAsync("test-pipeline", "test@test.com", new { });
			Assert.IsTrue(result.IsSuccessful);
			Assert.AreEqual(1, result.Results.Count);
			Assert.AreEqual(DispatchStatus.Dispatched, result.Results.First()?.DispatchStatus);
			Assert.AreEqual("EmailCommunication", result.Results.First()?.ResourceId);
		}
	}
}