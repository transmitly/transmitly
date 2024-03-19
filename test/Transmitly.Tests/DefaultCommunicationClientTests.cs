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

		[TestMethod]
		public async Task ShouldRespectAllowedChannelProviderPreference()
		{
			const string PipelineName = "test-pipeline";
			IReadOnlyCollection<IAudienceAddress> testRecipients = ["8885556666".AsAudienceAddress()];
			var model = ContentModel.Create(new { });
			
			var tly = new CommunicationsClientBuilder()
				.AddChannelProvider<MinimalConfigurationTestChannelProviderClient, ISms>("sms-provider")
				.AddChannelProvider<MinimalConfigurationTestChannelProviderClient, IVoice>("voice-provider")
				.AddPipeline(PipelineName, options =>
				{
					options.AddSms(sms =>
					{
						sms.Text.AddStringTemplate("SmsText");
					});

					options.AddVoice(voice =>
					{
						voice.Message.AddStringTemplate("Voice");
					});
				})
				.BuildClient();
			
			var result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Voice()]);
			
			Assert.IsTrue(result.IsSuccessful);
			Assert.AreEqual(1, result.Results.Count);
			Assert.AreEqual(Id.Channel.Voice(), result.Results?.First()?.ChannelId);

			result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Sms()]);
			Assert.IsTrue(result.IsSuccessful);
			Assert.AreEqual(1, result.Results.Count);
			Assert.AreEqual(Id.Channel.Sms(), result.Results?.First()?.ChannelId);

			result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Email()]);
			Assert.IsTrue(result.IsSuccessful);
			Assert.AreEqual(0, result.Results.Count);

			result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Voice(), Id.Channel.Sms()]);
			Assert.IsTrue(result.IsSuccessful);
			Assert.AreEqual(1, result.Results.Count);
			Assert.AreEqual(Id.Channel.Sms(), result.Results?.First()?.ChannelId);
		}

		[TestMethod]
		public async Task ShouldRespectAllowedChannelProviderPreferenceAnyDeliveryStrategy()
		{
			const string PipelineName = "test-pipeline";
			IReadOnlyCollection<IAudienceAddress> testRecipients = ["8885556666".AsAudienceAddress()];
			var model = ContentModel.Create(new { });

			var tly = new CommunicationsClientBuilder()
				.AddChannelProvider<MinimalConfigurationTestChannelProviderClient, ISms>("sms-provider")
				.AddChannelProvider<MinimalConfigurationTestChannelProviderClient, IVoice>("voice-provider")
				.AddPipeline(PipelineName, options =>
				{
					options.AddSms(sms =>
					{
						sms.Text.AddStringTemplate("SmsText");
					});

					options.AddVoice(voice =>
					{
						voice.Message.AddStringTemplate("Voice");
					});
					options.UseAnyMatchPipelineDeliveryStrategy();
				})
				.BuildClient();

			var result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Voice()]);

			Assert.IsTrue(result.IsSuccessful);
			Assert.AreEqual(1, result.Results.Count);
			Assert.AreEqual(Id.Channel.Voice(), result.Results?.First()?.ChannelId);

			result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Sms()]);
			Assert.IsTrue(result.IsSuccessful);
			Assert.AreEqual(1, result.Results.Count);
			Assert.AreEqual(Id.Channel.Sms(), result.Results?.First()?.ChannelId);

			result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Email()]);
			Assert.IsTrue(result.IsSuccessful);
			Assert.AreEqual(0, result.Results.Count);

			result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Voice(), Id.Channel.Sms()]);
			Assert.IsTrue(result.IsSuccessful);
			Assert.AreEqual(2, result.Results.Count);
			Assert.AreEqual(Id.Channel.Sms(), result.Results?.First()?.ChannelId);
#pragma warning disable S2589 // Boolean expressions should not be gratuitous
			Assert.AreEqual(Id.Channel.Voice(), result.Results?.Skip(1)?.First()?.ChannelId);
#pragma warning restore S2589 // Boolean expressions should not be gratuitous
		}
	}
}