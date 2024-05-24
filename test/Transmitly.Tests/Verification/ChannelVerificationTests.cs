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
using Transmitly.Verification;

namespace Transmitly.Tests.Verification
{
    [TestClass]
	public class ChannelVerificationTests
	{
		[TestMethod]
		public async Task ChannelVerificationSimpleFlowReturnsExpectedValues()
		{
			const string sender = "123456789";
			const string channelId = "unit-test-channel";
			const string channelProviderId = "unit-test-channel-provider";
			const string code = "012345";
			var commClient = new Mock<IChannelVerificationCommunicationsClient>();

			var option = new Mock<IChannelVerificationSupportedResult>();
			
			option.SetupGet(s => s.ChannelIds).Returns([]);
			option.SetupGet(s => s.ChannelProviderId).Returns(channelProviderId);

			
			commClient.Setup(s =>
				s.StartChannelVerificationAsync(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>()
				)
			)
			.ReturnsAsync<string, string, string, IChannelVerificationCommunicationsClient, IReadOnlyCollection<IStartChannelVerificationResult>>((sender, cp, ch) =>
			{
				var result = new Mock<IStartChannelVerificationResult>();
				result.SetupGet(s => s.Status).Returns(ChannelVerificationStatus.Delivered);
				result.SetupGet(s => s.Token).Returns((string?)null);
				result.SetupGet(s => s.ChannelProviderId).Returns(cp);
				result.SetupGet(s => s.ChannelId).Returns(ch);

				return [result.Object];
			}).Verifiable();

			commClient.Setup(s =>
				s.CheckChannelVerificationAsync(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.Is<string?>(x => x == null)
				)
			)
			.ReturnsAsync<string, string, string, string, string?, IChannelVerificationCommunicationsClient, IChannelVerificationValidationResult>((sender, cp, ch, code, token) =>
			{
				var result = new Mock<IChannelVerificationValidationResult>();
				result.SetupGet(r => r.IsVerified).Returns(true);
				result.SetupGet(r => r.IsSuccessful).Returns(true);
				result.SetupGet(r => r.ChannelId).Returns(ch);
				result.SetupGet(r => r.ChannelProviderId).Returns(cp);
				result.SetupGet(r => r.Recipient).Returns(sender);
				return result.Object;
			});

			commClient.Setup(s => s.GetChannelVerificationSupportedChannelProvidersAsync()).ReturnsAsync(() =>
				new List<IChannelVerificationSupportedResult>
				{
					option.Object
					
				}.AsReadOnly()
			);

			var client = commClient.Object;


			var verificationProviders = await client.GetChannelVerificationSupportedChannelProvidersAsync();
			IReadOnlyCollection<IStartChannelVerificationResult>? result = null;
			var options = verificationProviders;
			Assert.AreEqual(1, options.Count);
			foreach (var provider in options)
			{
				result = await client.StartChannelVerificationAsync(sender, provider.ChannelProviderId, "unit-test-channel");

				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(channelProviderId, result.First().ChannelProviderId);
				Assert.AreEqual(channelId, result.First().ChannelId);
				Assert.IsNull(result.First().Token);

                var validateResult = await client.CheckChannelVerificationAsync(sender, result.First()!.ChannelProviderId, result.First().ChannelId, code, result.First().Token);

                Assert.IsNotNull(validateResult);
                Assert.IsTrue(validateResult.IsSuccessful);
                Assert.IsTrue(validateResult.IsVerified);
                Assert.AreEqual(sender, validateResult.Recipient);
                Assert.AreEqual(channelId, validateResult.ChannelId);
                Assert.AreEqual(channelProviderId, validateResult.ChannelProviderId);
            }
		}

		//[TestMethod]
		//public async Task ClientShouldThrowIfChannelVerificationSupportNotAdded()
		//{
		//	var builder = new CommunicationsClientBuilder();
		//	const string channelProviderId = "unit-test-channel-provider";
		//	const string channelId = "unit-test-channel";

		//	builder
		//		.ChannelProvider
		//		.Build(channelProviderId, null)
		//		.AddChannelVerificationClient<UnitTestChannelVerificationProviderClient>(null, channelId)
		//		.Register();

		//	var client = builder.BuildClient();
		//	await Assert.ThrowsExceptionAsync<CommunicationsException>(() => client.GetChannelVerificationSupportedChannelProvidersAsync());
		//}

		//[TestMethod]
		//public async Task ChannelProviderCanRegisterChannelVerificationSupport()
		//{
		//	var builder = new CommunicationsClientBuilder();
		//	const string channelProviderId = "unit-test-channel-provider";
		//	const string channelId = "unit-test-channel";

		//	builder
		//		.AddChannelVerificationSupport(config => { })
		//		.ChannelProvider
		//		.Build(channelProviderId, null)
		//		.AddChannelVerificationClient<UnitTestChannelVerificationProviderClient>(channelId)
		//		.Register();


		//	var client = builder.BuildClient();
		//	var verificationClients = await client.GetChannelVerificationSupportedChannelProvidersAsync();

		//	Assert.AreEqual(1, verificationClients.Count);
		//	Assert.AreEqual(false, verificationClients.First().IsVerificationRequired);
		//	Assert.AreEqual(channelProviderId, verificationClients.First().ChannelProviderId);
		//	CollectionAssert.AreEquivalent(verificationClients.First().ChannelIds.ToList(), new List<string> { channelId }.AsReadOnly());
		//}

		//[TestMethod]
		//public async Task CanInitiateAndValidateRegisteredChannelVerificationClient()
		//{
		//	var builder = new CommunicationsClientBuilder();
		//	const string channelProviderId = "unit-test-channel-provider";
		//	const string channelId = "unit-test-channel";
		//	const string senderAddress = "test-address";
		//	const string code = "012345";

		//	builder
		//		.AddChannelVerificationSupport(config => { })
		//		.ChannelProvider
		//		.Build(channelProviderId, null)
		//		.AddChannelVerificationClient<UnitTestChannelVerificationProviderClient>(channelId)
		//		.Register();

		//	var client = builder.BuildClient();
		//	var initiateResult = await client.InitiateChannelVerificationAsync(senderAddress, channelProviderId, channelId);

		//	Assert.IsNotNull(initiateResult);
		//	Assert.AreEqual(1, initiateResult.Count);
		//	Assert.AreEqual(ChannelVerificationStatus.Delivered, initiateResult.First().Status);

		//	var verificationResult = await client.ValidateChannelVerificationAsync(senderAddress, channelProviderId, channelId, code, null);
		//	Assert.IsNotNull(verificationResult);
		//	Assert.AreEqual(true, verificationResult.IsSuccessful);
		//	Assert.AreEqual(true, verificationResult.IsVerified);
		//}

		//[TestMethod]
		//public async Task OnIsVerifiedOverrideIsCalled()
		//{
		//	var builder = new CommunicationsClientBuilder();
		//	const string channelProviderId = "unit-test-channel-provider";
		//	const string channelId = "unit-test-channel";
		//	const string senderAddress = "test-address";
		//	bool verified = false;

		//	builder
		//		.AddChannelVerificationSupport(config =>
		//		{
		//			config.OnIsSenderVerified = (context) =>
		//			{
		//				Assert.AreEqual(channelId, context.ChannelId);
		//				Assert.AreEqual(channelProviderId, context.ChannelProviderId);
		//				Assert.AreEqual(senderAddress, context.SenderAddress.Value);
		//				verified = true;
		//				return Task.FromResult<IChannelVerificationStatusResult>(new SenderVerifiedStatus(verified, channelProviderId, channelId));
		//			};
		//		})
		//		.ChannelProvider
		//		.Build(channelProviderId, null)
		//		.AddChannelVerificationClient<UnitTestChannelVerificationProviderClient>(channelId)
		//		.Register();

		//	var client = builder.BuildClient();
		//	var initiateResult = await client.IsSenderVerifiedAsync(senderAddress, channelProviderId, channelId);

		//	Assert.IsNotNull(initiateResult);
		//	Assert.AreEqual(true, initiateResult.HasValue);
		//	Assert.AreEqual(true, initiateResult.Value);
		//	Assert.IsTrue(verified);
		//}

		//[TestMethod]
		//public async Task OnIsVerifiedOverrideIsCalledWithChannelProviderClient()
		//{
		//	var builder = new CommunicationsClientBuilder();
		//	const string channelProviderId = "unit-test-channel-provider";
		//	const string channelId = "unit-test-channel";
		//	const string senderAddress = "test-address";
		//	bool verified = false;
		//	UnitTestChannelVerificationProviderClient.SenderVerified = null;
		//	builder
		//		.AddChannelVerificationSupport(config =>
		//		{
		//			config.OnIsSenderVerified = (context) =>
		//			{
		//				Assert.AreEqual(channelId, context.ChannelId);
		//				Assert.AreEqual(channelProviderId, context.ChannelProviderId);
		//				Assert.AreEqual(senderAddress, context.SenderAddress.Value);
		//				verified = true;
		//				return Task.FromResult<IChannelVerificationStatusResult>(new SenderVerifiedStatus(verified, null, channelId));
		//			};
		//		})
		//		.ChannelProvider
		//		.Build(channelProviderId, null)
		//		.AddChannelVerificationClient<UnitTestChannelVerificationProviderClient>(channelId)
		//		.Register();

		//	var client = builder.BuildClient();
		//	var initiateResult = await client.GetChannelVerificationStatusAsync(senderAddress, channelProviderId, channelId);

		//	Assert.IsNotNull(initiateResult);
		//	Assert.AreEqual(2, initiateResult.Count);
		//	Assert.IsTrue(verified);

		//	var overrideResult = initiateResult.First();
		//	Assert.IsNotNull(overrideResult);
		//	Assert.IsNotNull(overrideResult.IsVerified);
		//	Assert.AreEqual(true, overrideResult.IsVerified.Value);
		//	Assert.AreEqual(channelId, overrideResult.ChannelId);
		//	Assert.AreEqual(null, overrideResult.ChannelProviderId);

		//	var providerResult = initiateResult.Skip(1).First();
		//	Assert.IsNotNull(providerResult);
		//	Assert.IsNull(providerResult.IsVerified);
		//	Assert.AreEqual(channelProviderId, providerResult.ChannelProviderId);
		//	Assert.AreEqual(channelId, providerResult.ChannelId);
		//}
	}
}