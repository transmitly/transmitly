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

using AutoFixture;
using Moq;
using Transmitly.Delivery;
using Transmitly.Pipeline.Configuration;
using Transmitly.PlatformIdentity.Configuration;
using Transmitly.Tests.Integration;
namespace Transmitly.Tests;

[TestClass]
public class DefaultCommunicationClientTests : BaseUnitTest
{
	[TestMethod]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow(null)]
	public void ShouldGuardEmptyPipelineIntent(string value)
	{
		var (pipeline, coordinator, identity, reportHandler) = GetStores();

		var client = new DefaultCommunicationsClient(pipeline.Object, coordinator.Object, identity.Object, reportHandler.Object);
		Assert.ThrowsExactlyAsync<ArgumentNullException>(() => client.DispatchAsync(value, "test", new { }));
	}

	[TestMethod]
	public async Task ShouldSendBasedOnAllowedChannelProviderRestrictions()
	{
		const string ChannelProvider0 = "channel-provider-0";
		const string ChannelProvider1 = "channel-provider-1";
		const string ChannelId = "unit-test-channel";
		var client = new CommunicationsClientBuilder()
				.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, object>(ChannelProvider0, "unit-test-channel")
				.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, object>(ChannelProvider1, "unit-test-channel")
				.AddPipeline("test-pipeline", options =>
				{
					options

						.AddChannel(new UnitTestChannel("c0-from", ChannelId, ChannelProvider0))
						.AddChannel(new UnitTestChannel("c1-from", ChannelId, ChannelProvider1))
						.AddChannel(new UnitTestChannel("c2-from", "channel-not-included", ChannelProvider0));
				})
				.BuildClient();
		var result = await client.DispatchAsync("test-pipeline", "unit-test-address-0", new { });
		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
		Assert.AreEqual(ChannelId, result.Results?.First()?.ChannelId);
		Assert.AreEqual(ChannelProvider0, result.Results?.First()?.ChannelProviderId);
	}

	private static (
		Mock<IPipelineService> pipeline,
		Mock<IDispatchCoordinatorService> coordinator,
		Mock<IPlatformIdentityService> identity,
		Mock<IDeliveryReportService> deliveryReportHandler
		) GetStores()
	{

		return (
			new Mock<IPipelineService>(),
			new Mock<IDispatchCoordinatorService>(),
			new Mock<IPlatformIdentityService>(),
			new Mock<IDeliveryReportService>()
		);
	}

	[TestMethod]
	public async Task ShouldDispatchUsingCorrectChannelProviderDispatcher()
	{
		var tly = new CommunicationsClientBuilder();

		tly.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, ISms>("test-channel-provider");
		tly.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, IEmail>("test-channel-provider");
		tly.ChannelProvider.Add<OptionalConfigurationTestChannelProviderDispatcher, UnitTestCommunication>("test-channel-provider");

		tly.AddPipeline("test-pipeline", options =>
		{
			options.AddEmail("from@address.com".AsIdentityAddress(), email =>
			{
				email.AddRecipientAddressPurpose("Test").AddDeliveryReportCallbackUrlResolver(_ => Task.FromResult<string?>("https://test.com"));
				email.Subject.AddStringTemplate("Test sub");
			});
		});

		var client = tly.BuildClient();
		Assert.IsNotNull(client);
		var result = await client.DispatchAsync("test-pipeline", "test@test.com", new { });
		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
		Assert.IsTrue(result.Results.First()?.Status.IsSuccess());
		Assert.AreEqual("EmailCommunication", result.Results.First()?.ResourceId);
	}

	[TestMethod]
	public async Task ShouldRespectAllowedChannelProviderPreference()
	{
		const string PipelineIntent = "test-pipeline";
		IReadOnlyCollection<IIdentityAddress> testRecipients = ["8885556666".AsIdentityAddress()];
		var model = TransactionModel.Create(new { });

		var tly = new CommunicationsClientBuilder()
			.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, ISms>("sms-provider")
			.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, IVoice>("voice-provider")
			.AddPipeline(PipelineIntent, options =>
			{
				options
					.AddSms(sms =>
					{
						sms.Message.AddStringTemplate("SmsText");
					})
					.AddVoice(voice =>
					{
						voice.Message.AddStringTemplate("Voice");
					});
			})
			.BuildClient();

		var result = await tly.DispatchAsync(PipelineIntent, testRecipients, model, [Id.Channel.Voice()]);

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
		Assert.AreEqual(Id.Channel.Voice(), result.Results?.First()?.ChannelId);

		result = await tly.DispatchAsync(PipelineIntent, testRecipients, model, [Id.Channel.Sms()]);
		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
		Assert.AreEqual(Id.Channel.Sms(), result.Results?.First()?.ChannelId);

		result = await tly.DispatchAsync(PipelineIntent, testRecipients, model, [Id.Channel.Email()]);
		Assert.IsFalse(result.IsSuccessful);

		result = await tly.DispatchAsync(PipelineIntent, testRecipients, model, [Id.Channel.Voice(), Id.Channel.Sms()]);
		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
		Assert.AreEqual(Id.Channel.Sms(), result.Results?.First()?.ChannelId);
	}

	[TestMethod]
	public async Task ShouldRespectAllowedChannelProviderPreferenceAnyDeliveryStrategy()
	{
		const string PipelineIntent = "test-pipeline";
		IReadOnlyCollection<IIdentityAddress> testRecipients = ["8885556666".AsIdentityAddress()];
		var model = TransactionModel.Create(new { });

		var tly = new CommunicationsClientBuilder()
			.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, ISms>("sms-provider")
			.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, IVoice>("voice-provider")
			.AddPipeline(PipelineIntent, options =>
			{
				options
					.AddSms(sms =>
					{
						sms.Message.AddStringTemplate("SmsText");
					})
					.AddVoice(voice =>
					{
						voice.Message.AddStringTemplate("Voice");
					})
					.UseAnyMatchPipelineDeliveryStrategy();
			})
			.BuildClient();

		var result = await tly.DispatchAsync(PipelineIntent, testRecipients, model, [Id.Channel.Voice()]);

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
		Assert.AreEqual(Id.Channel.Voice(), result.Results?.First()?.ChannelId);

		result = await tly.DispatchAsync(PipelineIntent, testRecipients, model, [Id.Channel.Sms()]);
		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
		Assert.AreEqual(Id.Channel.Sms(), result.Results?.First()?.ChannelId);

		result = await tly.DispatchAsync(PipelineIntent, testRecipients, model, [Id.Channel.Email()]);
		Assert.IsFalse(result.IsSuccessful);

		result = await tly.DispatchAsync(PipelineIntent, testRecipients, model, [Id.Channel.Voice(), Id.Channel.Sms()]);
		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(2, result.Results.Count);
		Assert.AreEqual(Id.Channel.Sms(), result.Results?.First()?.ChannelId);
#pragma warning disable S2589 // Boolean expressions should not be gratuitous
		Assert.AreEqual(Id.Channel.Voice(), result.Results?.Skip(1)?.First()?.ChannelId);
#pragma warning restore S2589 // Boolean expressions should not be gratuitous
	}


	[TestMethod]
	public async Task ShouldReturnPipelineNotFoundResultCode()
	{
		const string PipelineIntent = "test-pipeline";
		IReadOnlyCollection<IIdentityAddress> testRecipients = ["8885556666".AsIdentityAddress()];
		var model = TransactionModel.Create(new { });

		var tly = new CommunicationsClientBuilder()
			.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, IVoice>("voice-provider")
			.AddPipeline(PipelineIntent, options => { })
			.BuildClient();


		var result = await tly.DispatchAsync("DoesNotExist", testRecipients, model, [Id.Channel.Voice()]);

		Assert.IsFalse(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
		var firstResult = result.Results.First();
		Assert.IsNotNull(firstResult);
		Assert.AreEqual(PredefinedCommunicationStatuses.PipelineNotFound, firstResult.Status);
	}

	[TestMethod]
	public async Task ShouldReturnChannelFiltersNotAllowedWhenChannelFiltersDisabled()
	{
		const string PipelineIntent = "test-pipeline";
		IReadOnlyCollection<IIdentityAddress> testRecipients = ["8885556666".AsIdentityAddress()];
		var model = TransactionModel.Create(new { });

		var tly = new CommunicationsClientBuilder()
			.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, IVoice>("voice-provider")
			.AddPipeline(PipelineIntent, options => { options.AllowDispatchRequirements(false); })
			.BuildClient();

		var result = await tly.DispatchAsync(PipelineIntent, testRecipients, model, [Id.Channel.Voice()]);

		Assert.IsFalse(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
		var firstResult = result.Results.First();
		Assert.IsNotNull(firstResult);
		Assert.AreEqual(4005, firstResult.Status.Code);
	}

	[TestMethod()]
	public async Task ShouldThrowIfNullIdentityAddressCollection()
	{
		var sut = fixture.Create<DefaultCommunicationsClient>();

		await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
			sut.DispatchAsync("test", (IReadOnlyCollection<IIdentityAddress>)null!, null!, null!, CancellationToken.None)
		);

		await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
			sut.DispatchAsync("test", (string)null!, (object)null!, null, CancellationToken.None)
		);

		await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
			sut.DispatchAsync("test", (string)null!, (object)null!, null, CancellationToken.None)
		);
	}
}