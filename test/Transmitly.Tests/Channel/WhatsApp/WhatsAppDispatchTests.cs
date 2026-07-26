// Copyright (c) Code Impressions, LLC. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Transmitly.Channel.WhatsApp;

namespace Transmitly.Tests;

[TestClass]
public class WhatsAppDispatchTests : BaseUnitTest
{
	[TestMethod]
	public async Task ShouldDispatchWhatsAppWhenRecipientIsTyped()
	{
		const string pipelineIntent = "test-pipeline";
		IReadOnlyCollection<IPlatformIdentityAddress> typedWhatsAppRecipients =
		[
			new PlatformIdentityAddress("+18885556666", type: PlatformIdentityAddress.Types.WhatsApp())
		];

		var client = BuildWhatsAppClient(pipelineIntent);

		var result = await client.DispatchAsync(pipelineIntent, typedWhatsAppRecipients, TransactionModel.Create(new { }), [Id.Channel.WhatsApp()]);

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
		Assert.AreEqual(Id.Channel.WhatsApp(), result.Results.First()?.ChannelId);
	}

	[TestMethod]
	public async Task ShouldDispatchWhatsAppWhenRecipientUsesPrefix()
	{
		const string pipelineIntent = "test-pipeline";
		IReadOnlyCollection<IPlatformIdentityAddress> prefixedWhatsAppRecipients =
		[
			"whatsapp:+18885556666".AsIdentityAddress()
		];

		var client = BuildWhatsAppClient(pipelineIntent);

		var result = await client.DispatchAsync(pipelineIntent, prefixedWhatsAppRecipients, TransactionModel.Create(new { }), [Id.Channel.WhatsApp()]);

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
		Assert.AreEqual(Id.Channel.WhatsApp(), result.Results.First()?.ChannelId);
	}

	[TestMethod]
	public async Task ShouldNotDispatchWhatsAppForUntypedPhoneNumber()
	{
		const string pipelineIntent = "test-pipeline";
		IReadOnlyCollection<IPlatformIdentityAddress> untypedPhoneRecipients =
		[
			"+18885556666".AsIdentityAddress()
		];

		var client = BuildWhatsAppClient(pipelineIntent);

		var result = await client.DispatchAsync(pipelineIntent, untypedPhoneRecipients, TransactionModel.Create(new { }), [Id.Channel.WhatsApp()]);

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(0, result.Results.Count);
	}

	[TestMethod]
	public async Task ShouldDispatchSmsButNotWhatsAppForUntypedPhoneNumber()
	{
		const string pipelineIntent = "test-pipeline";
		IReadOnlyCollection<IPlatformIdentityAddress> untypedPhoneRecipients =
		[
			"+18885556666".AsIdentityAddress()
		];

		var client = new CommunicationsClientBuilder()
			.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, ISms>("sms-provider")
			.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, IWhatsApp>("whatsapp-provider")
			.AddPipeline(pipelineIntent, options =>
			{
				options
					.AddSms(sms =>
					{
						sms.Message.AddStringTemplate("SmsText");
					})
					.AddWhatsApp(whatsApp =>
					{
						whatsApp.Message.AddStringTemplate("WhatsAppText");
					})
					.UseAnyMatchPipelineDeliveryStrategy();
			})
			.BuildClient();

		var result = await client.DispatchAsync(pipelineIntent, untypedPhoneRecipients, TransactionModel.Create(new { }), [Id.Channel.Sms(), Id.Channel.WhatsApp()]);

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
		Assert.AreEqual(Id.Channel.Sms(), result.Results.First()?.ChannelId);
	}

	[TestMethod]
	public async Task ShouldDispatchSmsAndWhatsAppWhenRecipientHasBothAddressKinds()
	{
		const string pipelineIntent = "test-pipeline";
		IReadOnlyCollection<IPlatformIdentityAddress> recipients =
		[
			"+18885556666".AsIdentityAddress(),
			new PlatformIdentityAddress("+18885556666", type: PlatformIdentityAddress.Types.WhatsApp())
		];

		var client = new CommunicationsClientBuilder()
			.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, ISms>("sms-provider")
			.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, IWhatsApp>("whatsapp-provider")
			.AddPipeline(pipelineIntent, options =>
			{
				options
					.AddSms(sms =>
					{
						sms.Message.AddStringTemplate("SmsText");
					})
					.AddWhatsApp(whatsApp =>
					{
						whatsApp.Message.AddStringTemplate("WhatsAppText");
					})
					.UseAnyMatchPipelineDeliveryStrategy();
			})
			.BuildClient();

		var result = await client.DispatchAsync(pipelineIntent, recipients, TransactionModel.Create(new { }), [Id.Channel.Sms(), Id.Channel.WhatsApp()]);

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(2, result.Results.Count);
		CollectionAssert.AreEquivalent(new[] { Id.Channel.Sms(), Id.Channel.WhatsApp() }, result.Results.Select(r => r?.ChannelId).ToArray());
	}

	private static ICommunicationsClient BuildWhatsAppClient(string pipelineIntent)
	{
		return new CommunicationsClientBuilder()
			.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, IWhatsApp>("whatsapp-provider")
			.AddPipeline(pipelineIntent, options =>
			{
				options.AddWhatsApp(whatsApp =>
				{
					whatsApp.Message.AddStringTemplate("WhatsAppText");
				});
			})
			.BuildClient();
	}
}
