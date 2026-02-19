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

using Transmitly.PlatformIdentity.Configuration;
using Transmitly.Tests.Integration;

namespace Transmitly.Tests.Identity;

[TestClass]
public class PlatformIdentityProfileEnricherTests
{
	private static readonly List<string> OrderedEnricherExecution = [];

	private const string PipelineIntent = "unit-test-profile-enricher-pipeline";
	private const string ChannelProviderId = "unit-test-profile-enricher-provider";
	private const string ChannelId = "unit-test-profile-enricher-channel";

	[TestInitialize]
	public void Initialize()
	{
		OrderedEnricherExecution.Clear();
		ResolvedIdentityEnricher.ExecutionCount = 0;
	}

	[TestMethod]
	public async Task IdentityProfileShouldBeEnrichedBeforeDispatch()
	{
		var client = new CommunicationsClientBuilder()
			.ChannelProvider.Add<OptionalConfigurationTestChannelProviderDispatcher, UnitTestCommunication>(
				ChannelProviderId,
				ChannelId,
				ChannelId
			)
			.AddPipeline(PipelineIntent, options =>
			{
				var channel = new UnitTestChannel("unit-test-address-from", ChannelId, ChannelProviderId);
				channel.Configuration.Subject.AddStringTemplate("Your OTP Code: {{Code}}");
				options.AddChannel(channel);
			})
			.AddPlatformIdentityProfileEnricher<ChangeAddressProfileEnricher>()
			.BuildClient();

		var profile = new PlatformIdentityProfile(Guid.NewGuid().ToString(), null, [new PlatformIdentityAddress("does-not-match")]);

		var result = await client.DispatchAsync(PipelineIntent, [profile], TransactionModel.Create(new { Code = "1234" }));

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
	}

	[TestMethod]
	public async Task EnrichersShouldRunInOrderAndRespectTypeFilter()
	{
		string[] expected = ["first", "second"];
		var client = new CommunicationsClientBuilder()
			.ChannelProvider.Add<OptionalConfigurationTestChannelProviderDispatcher, UnitTestCommunication>(
				ChannelProviderId,
				ChannelId,
				ChannelId
			)
			.AddPipeline(PipelineIntent, options =>
			{
				var channel = new UnitTestChannel("unit-test-address-from", ChannelId, ChannelProviderId);
				channel.Configuration.Subject.AddStringTemplate("Your OTP Code: {{Code}}");
				options.AddChannel(channel);
			})
			.AddPlatformIdentityProfileEnricher<SecondOrderedEnricher>("member-type", 20)
			.AddPlatformIdentityProfileEnricher<FirstOrderedEnricher>("member-type", 10)
			.AddPlatformIdentityProfileEnricher<WrongTypeOrderedEnricher>("other-type", 0)
			.BuildClient();

		var profile = new MockPlatformIdentity1(Guid.NewGuid(), "member-type")
		{
			Addresses = [new PlatformIdentityAddress("unit-test-address")]
		};

		var result = await client.DispatchAsync(PipelineIntent, [profile], TransactionModel.Create(new { Code = "1234" }));

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual("second", profile.Attributes["priority"]);
		CollectionAssert.AreEqual(expected, OrderedEnricherExecution.ToArray());
	}

	[TestMethod]
	public async Task EnricherFailureShouldFailWholeOperation()
	{
		var client = new CommunicationsClientBuilder()
			.ChannelProvider.Add<OptionalConfigurationTestChannelProviderDispatcher, UnitTestCommunication>(
				ChannelProviderId,
				ChannelId,
				ChannelId
			)
			.AddPipeline(PipelineIntent, options =>
			{
				var channel = new UnitTestChannel("unit-test-address-from", ChannelId, ChannelProviderId);
				channel.Configuration.Subject.AddStringTemplate("Your OTP Code: {{Code}}");
				options.AddChannel(channel);
			})
			.AddPlatformIdentityProfileEnricher<ThrowingProfileEnricher>()
			.BuildClient();

		var profile = new PlatformIdentityProfile(Guid.NewGuid().ToString(), null, [new PlatformIdentityAddress("unit-test-address")]);

		var result = await client.DispatchAsync(PipelineIntent, [profile], TransactionModel.Create(new { Code = "1234" }));

		Assert.IsFalse(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);
		Assert.AreEqual(PredefinedCommunicationStatuses.PlatformIdentityProfileEnrichmentFailed, result.Results.Single()?.Status);
	}

	[TestMethod]
	public async Task ResolvedIdentityProfilesShouldAlsoBeEnriched()
	{
		var client = new CommunicationsClientBuilder()
			.ChannelProvider.Add<OptionalConfigurationTestChannelProviderDispatcher, UnitTestCommunication>(
				ChannelProviderId,
				ChannelId,
				ChannelId
			)
			.AddPipeline(PipelineIntent, options =>
			{
				var channel = new UnitTestChannel("unit-test-address-from", ChannelId, ChannelProviderId);
				channel.Configuration.Subject.AddStringTemplate("Your OTP Code: {{Code}}");
				options.AddChannel(channel);
			})
			.AddPlatformIdentityResolver<MockPlatformIdentityRepository>()
			.AddPlatformIdentityProfileEnricher<ResolvedIdentityEnricher>()
			.BuildClient();

		var result = await client.DispatchAsync(
			PipelineIntent,
			[new IdentityReference("test-identity", Guid.NewGuid().ToString())],
			TransactionModel.Create(new { Code = "1234" }));

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(1, ResolvedIdentityEnricher.ExecutionCount);
	}



	private sealed class ChangeAddressProfileEnricher : IPlatformIdentityProfileEnricher
	{
		public Task EnrichIdentityProfileAsync(IPlatformIdentityProfile identityProfile)
		{
			if (identityProfile is PlatformIdentityProfile profile)
				profile.Addresses = [new PlatformIdentityAddress("unit-test-address-enriched")];

			return Task.CompletedTask;
		}
	}

	private sealed class FirstOrderedEnricher : IPlatformIdentityProfileEnricher
	{
		public Task EnrichIdentityProfileAsync(IPlatformIdentityProfile identityProfile)
		{
			if (identityProfile is MockPlatformIdentity1 profile)
				profile.Attributes["priority"] = "first";

			OrderedEnricherExecution.Add("first");
			return Task.CompletedTask;
		}
	}

	private sealed class SecondOrderedEnricher : IPlatformIdentityProfileEnricher
	{
		public Task EnrichIdentityProfileAsync(IPlatformIdentityProfile identityProfile)
		{
			if (identityProfile is MockPlatformIdentity1 profile)
				profile.Attributes["priority"] = "second";

			OrderedEnricherExecution.Add("second");
			return Task.CompletedTask;
		}
	}

	private sealed class WrongTypeOrderedEnricher : IPlatformIdentityProfileEnricher
	{
		public Task EnrichIdentityProfileAsync(IPlatformIdentityProfile identityProfile)
		{
			OrderedEnricherExecution.Add("wrong-type");
			return Task.CompletedTask;
		}
	}

	private sealed class ThrowingProfileEnricher : IPlatformIdentityProfileEnricher
	{
		public Task EnrichIdentityProfileAsync(IPlatformIdentityProfile identityProfile)
		{
			throw new InvalidOperationException("Expected test failure.");
		}
	}

	private sealed class ResolvedIdentityEnricher : IPlatformIdentityProfileEnricher
	{
		internal static int ExecutionCount { get; set; }

		public Task EnrichIdentityProfileAsync(IPlatformIdentityProfile identityProfile)
		{
			ExecutionCount++;
			return Task.CompletedTask;
		}
	}
}

