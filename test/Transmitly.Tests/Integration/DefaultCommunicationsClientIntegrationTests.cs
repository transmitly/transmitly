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

namespace Transmitly.Tests.Integration
{
	[TestClass]
	public partial class DefaultCommunicationsClientTests
	{
		[TestMethod]
		public async Task MultipleChannelPipelineChannelCommunicationClientTest()
		{
			//Scenario
			//OTP Code to specific email address
			const string FromAddress = "unit-test-address-from";
			const string PipelineName = "unit-test-pipeline";
			const string ChannelProviderId = "unit-test-channel-provider";
			const string ExpectedMessage = "Your OTP Code: {{Code}}";
			const string ChannelId = "unit-test-channel";

			IReadOnlyCollection<IAudienceAddress> RecipientAddresses = new AudienceAddress[] { new("unit-test-address-recipient") };
			string[] SupportedChannels = [ChannelProviderId];

			var client = new CommunicationsClientBuilder()
				.AddChannelProvider(
					ChannelProviderId,
					new OptionalConfigurationTestChannelProviderClient(),
					ChannelId, ChannelId + "-2"
				 )
				.AddPipeline(PipelineName, options =>
				{
					var channel = new UnitTestChannel(FromAddress, ChannelId, ChannelProviderId);
					channel.Subject.AddStringTemplate("Skip me!");

					options.AddChannel(channel);

					var channel2 = new UnitTestChannel(FromAddress + "-2", ChannelId + "-2", ChannelProviderId);
					channel2.Subject.AddStringTemplate(ExpectedMessage);

					options.AddChannel(channel2);


					options.UseFirstMatchPipelineDeliveryStrategy();
				})
				.BuildClient();

			var model = ContentModel.Create(new { Code = "123456" });
			var result = await client.DispatchAsync(PipelineName, RecipientAddresses, ContentModel.Create(new { Code = "123546" }));

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Results);
			Assert.AreEqual(1, result.Results.Count);
			var singleResult = result.Results.First();
			Assert.AreEqual(DispatchStatus.Delivered, singleResult.DispatchStatus);
		}


		[TestMethod]
		public async Task MinimalConfigurationTest()
		{
			//Scenario
			//OTP Code to specific email address
			const string FromAddress = "unit-test-address-from";
			const string PipelineName = "unit-test-pipeline";
			const string AudienceTypeIdentifier = "unit-test-audience-identifier-0";
			const string ChannelProviderId = "unit-test-channel-provider";
			//const string ExpectedMessage = "Sent Mock Message!";
			const string ChannelId = "unit-test-channel";
			IReadOnlyCollection<IAudienceAddress> RecipientAddresses = new AudienceAddress[] { new("unit-test-address-recipient") };
			string[] SupportedChannels = [ChannelId];
			//MinimalConfigurationTestChannelProviderClient.ExpectedMessage = ExpectedMessage;


			var client = new CommunicationsClientBuilder()
				//Channel Provider = Provides the services for a particular channel or channels
				//   - MailKit - Email
				//   - SendGrid - Email, SMS
				//   - Twilio - Voice
				//   - FortuneCookie - Print
				.AddChannelProvider<UnitTestCommunication>(
					ChannelProviderId,
					new MinimalConfigurationTestChannelProviderClient(),
					ChannelId)

				//Pipeline = Defines the available channels of a communication
				//   - Can define allowed channels of communications, Email/SMS
				//   - (Imperative) Must communicate/send.
				//     - OTP Codes, Fraud Messages
				//   - (Observed) Signaled activity, may trigger a communication (single or grouped signals) or multiple communications
				//     - HSA Contributions, Welcome Kit
				.AddPipeline(PipelineName, AudienceTypeIdentifier, pipeline =>
				{
					//Channel = Defines the structure of the communication for a channel provider
					//   - Email - Subject, Body, Recipients
					//   - SMS - Recipient, Message
					//   - Voice - Recipient, Content, Callbacks
					var channel = new UnitTestChannel(FromAddress, ChannelId, ChannelProviderId);
					channel.Subject.AddStringTemplate("Your OTP Code: {{Code}}");
					pipeline.AddChannel(channel);
				})
				.BuildClient();

			var result = await client.DispatchAsync(PipelineName, RecipientAddresses, ContentModel.Create(new { Code = "123546" }));

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Results);
			Assert.AreEqual(1, result.Results.Count);
			var singleResult = result.Results.First();
			Assert.AreEqual(DispatchStatus.Delivered, singleResult.DispatchStatus);
		}
	}
}
