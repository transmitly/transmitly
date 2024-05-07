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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Transmitly.Exceptions;
using Transmitly.Verification;

namespace Transmitly.Tests.Verification
{
	[TestClass]
	public class SenderVerificationTests
	{
		[TestMethod]
		public async Task SenderVerificationSimpleFlowReturnsExpectedValues()
		{
			const string sender = "123456789";
			const string channelId = "unit-test-channel";
			const string channelProviderId = "unit-test-channel-provider";
			const string code = "012345";
			var commClient = new Mock<ISenderVerificationCommunicationsClient>();

			var requiredOption = new Mock<ISenderVerificationSupportedResult>();
			requiredOption.SetupGet(s => s.IsVerificationRequired).Returns(true);
			requiredOption.SetupGet(s => s.ChannelIds).Returns([]);
			requiredOption.SetupGet(s => s.ChannelProviderId).Returns(channelProviderId);

			var option = new Mock<ISenderVerificationSupportedResult>();
			option.SetupGet(s => s.IsVerificationRequired).Returns(false);
			option.SetupGet(s => s.ChannelIds).Returns([]);
			option.SetupGet(s => s.ChannelProviderId).Returns(channelProviderId);

			commClient.Setup(s => s.IsSenderVerifiedAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>())).ReturnsAsync(false).Verifiable();
			commClient.Setup(s =>
				s.InitiateSenderVerificationAsync(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>()
				)
			)
			.ReturnsAsync<string, string, string, ISenderVerificationCommunicationsClient, IInitiateSenderVerificationResult>((sender, cp, ch) =>
			{
				var result = new Mock<IInitiateSenderVerificationResult>();
				result.SetupGet(s => s.IsSuccessful).Returns(true);
				result.SetupGet(s => s.Code).Returns(code);
				result.SetupGet(s => s.Nonce).Returns((string?)null);
				result.SetupGet(s => s.ChannelProviderId).Returns(cp);
				result.SetupGet(s => s.ChannelId).Returns(ch);

				return result.Object;
			}).Verifiable();

			commClient.Setup(s =>
				s.ValidateSenderVerificationAsync(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.Is<string?>(x => x == null)
				)
			)
			.ReturnsAsync<string, string, string, string, string?, ISenderVerificationCommunicationsClient, ISenderVerificationValidationResult>((sender, cp, ch, code, token) =>
			{
				var result = new Mock<ISenderVerificationValidationResult>();
				result.SetupGet(r => r.IsVerified).Returns(true);
				result.SetupGet(r => r.IsSuccessful).Returns(true);
				result.SetupGet(r => r.ChannelId).Returns(ch);
				result.SetupGet(r => r.ChannelProviderId).Returns(cp);
				result.SetupGet(r => r.SenderAddress).Returns(sender);
				return result.Object;
			});

			commClient.Setup(s => s.GetSenderVerificationSupportedChannelProvidersAsync()).ReturnsAsync(() =>
				new List<ISenderVerificationSupportedResult>
				{
					option.Object,
					requiredOption.Object
				}.AsReadOnly()
			);

			var client = commClient.Object;


			var verificationProviders = await client.GetSenderVerificationSupportedChannelProvidersAsync();
			IInitiateSenderVerificationResult? result = null;
			var options = verificationProviders.Where(x => x.IsVerificationRequired);
			Assert.AreEqual(1, options.Count());
			foreach (var provider in options)
			{
				//are we validated with ANY channel or channel provider
				var isValidated = await client.IsSenderVerifiedAsync(sender);
				Assert.IsNotNull(isValidated);
				Assert.IsFalse(isValidated.Value);
				// null = provider does not support checking, no verification svc registered, or unknown
				// true = validated
				// false = not validated
				result = await client.InitiateSenderVerificationAsync(sender, provider.ChannelProviderId, "unit-test-channel");

				Assert.IsNotNull(result);
				Assert.AreEqual(channelProviderId, result.ChannelProviderId);
				Assert.AreEqual(channelId, result.ChannelId);
				Assert.AreEqual(code, result.Code);
				Assert.IsNull(result.Nonce);
			}

			var validateResult = await client.ValidateSenderVerificationAsync(sender, result.ChannelProviderId, result.ChannelId, result.Code, result.Nonce);

			Assert.IsNotNull(validateResult);
			Assert.IsTrue(validateResult.IsSuccessful);
			Assert.IsTrue(validateResult.IsVerified);
			Assert.AreEqual(sender, validateResult.SenderAddress);
			Assert.AreEqual(channelId, validateResult.ChannelId);
			Assert.AreEqual(channelProviderId, validateResult.ChannelProviderId);
		}

		[TestMethod]
		public async Task ClientShouldThrowIfSenderVerificationSupportNotAdded()
		{
			var builder = new CommunicationsClientBuilder();
			const string channelProviderId = "unit-test-channel-provider";
			const string channelId = "unit-test-channel";

			builder
				.ChannelProvider
				.Create(channelProviderId, null)
				.AddSenderVerificationClient<UnitTestSenderVerificationProviderClient>(channelId)
				.Register();

			var client = builder.BuildClient();
			await Assert.ThrowsExceptionAsync<CommunicationsException>(() => client.GetSenderVerificationSupportedChannelProvidersAsync());
		}

		[TestMethod]
		public async Task ChannelProviderCanRegisterSenderVerificationSupport()
		{
			var builder = new CommunicationsClientBuilder();
			const string channelProviderId = "unit-test-channel-provider";
			const string channelId = "unit-test-channel";

			builder
				.AddSenderVerificationSupport(config => { })
				.ChannelProvider
				.Create(channelProviderId, null)
				.AddSenderVerificationClient<UnitTestSenderVerificationProviderClient>(channelId)
				.Register();


			var client = builder.BuildClient();
			var verificationClients = await client.GetSenderVerificationSupportedChannelProvidersAsync();

			Assert.AreEqual(1, verificationClients.Count);
			Assert.AreEqual(false, verificationClients.First().IsVerificationRequired);
			Assert.AreEqual(channelProviderId, verificationClients.First().ChannelProviderId);
			CollectionAssert.AreEquivalent(verificationClients.First().ChannelIds.ToList(), new List<string> { channelId }.AsReadOnly());
		}

		[TestMethod]
		public async Task CanInitiateAndValidateRegisteredSenderVerificationClient()
		{
			var builder = new CommunicationsClientBuilder();
			const string channelProviderId = "unit-test-channel-provider";
			const string channelId = "unit-test-channel";
			const string senderAddress = "test-address";
			const string code = "123456";

			builder
				.AddSenderVerificationSupport(config => { })
				.ChannelProvider
				.Create(channelProviderId, null)
				.AddSenderVerificationClient<UnitTestSenderVerificationProviderClient>(channelId)
				.Register();

			var client = builder.BuildClient();
			var initiateResult = await client.InitiateSenderVerificationAsync(senderAddress, channelProviderId, channelId);

			Assert.IsNotNull(initiateResult);
			Assert.AreEqual(true, initiateResult.IsSuccessful);
			Assert.AreEqual(code, initiateResult.Code);

			var verificationResult = await client.ValidateSenderVerificationAsync(senderAddress, channelProviderId, channelId, initiateResult.Code, null);
			Assert.IsNotNull(verificationResult);
			Assert.AreEqual(true, verificationResult.IsSuccessful);
			Assert.AreEqual(true, verificationResult.IsVerified);
		}

		[TestMethod]
		public async Task OnIsVerifiedOverrideIsCalled()
		{
			var builder = new CommunicationsClientBuilder();
			const string channelProviderId = "unit-test-channel-provider";
			const string channelId = "unit-test-channel";
			const string senderAddress = "test-address";
			const string code = "123456";

			builder
				.AddSenderVerificationSupport(config => { })
				.ChannelProvider
				.Create(channelProviderId, null)
				.AddSenderVerificationClient<UnitTestSenderVerificationProviderClient>(channelId)
				.Register();
			
			var client = builder.BuildClient();
			var initiateResult = await client.InitiateSenderVerificationAsync(senderAddress, channelProviderId, channelId);

			Assert.IsNotNull(initiateResult);
			Assert.AreEqual(true, initiateResult.IsSuccessful);
			Assert.AreEqual(code, initiateResult.Code);

			var verificationResult = await client.ValidateSenderVerificationAsync(senderAddress, channelProviderId, channelId, initiateResult.Code, null);
			Assert.IsNotNull(verificationResult);
			Assert.AreEqual(true, verificationResult.IsSuccessful);
			Assert.AreEqual(true, verificationResult.IsVerified);
		}
	}
}