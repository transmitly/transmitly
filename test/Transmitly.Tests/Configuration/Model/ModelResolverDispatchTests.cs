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

using System.Collections.Concurrent;
using Transmitly.Channel.Configuration;
using Transmitly.ChannelProvider;
using Transmitly.Model.Configuration;
using Transmitly.Tests.Mocks;

namespace Transmitly.Tests.Configuration.Model;

[TestClass]
public sealed class ModelResolverDispatchTests
{
	[TestMethod]
	public async Task ModelResolvers_RunPerRecipientAndPerChannel()
	{
		ModelResolverRecorder.Reset();
		RecordingDispatcher.Reset();

		var builder = new CommunicationsClientBuilder()
			.AddModelResolver<PerRecipientResolver>(options => options.Scope = ModelResolverScope.PerRecipient)
			.AddModelResolver<PerChannelResolver>(options => options.Scope = ModelResolverScope.PerChannel)
			.ChannelProvider.Add<RecordingDispatcher, UnitTestCommunication>("provider", "channel-a", "channel-b")
			.AddPipeline("intent", options =>
			{
				options.AddChannel(new ModelSubjectChannel("channel-a"));
				options.AddChannel(new ModelSubjectChannel("channel-b"));
				options.UseAnyMatchPipelineDeliveryStrategy();
			});

		var client = builder.BuildClient();

		var recipient = new PlatformIdentityProfile(
			"id",
			"type",
			new[] { "unit-test-address".AsIdentityAddress() });

		await client.DispatchAsync(
			"intent",
			new[] { recipient },
			TransactionModel.Create(new { }),
			Array.Empty<string>());

		Assert.AreEqual(1, ModelResolverRecorder.PerRecipientCount);
		Assert.AreEqual(2, ModelResolverRecorder.PerChannelCount);
		CollectionAssert.AreEquivalent(
			new[] { "recipient:channel-a", "recipient:channel-b" },
			RecordingDispatcher.Subjects.ToArray());
	}

	[TestMethod]
	public async Task ModelResolvers_PerRecipientRunsOnceWithMultipleProviders()
	{
		ModelResolverRecorder.Reset();

		var builder = new CommunicationsClientBuilder()
			.AddModelResolver<PerRecipientResolver>(options => options.Scope = ModelResolverScope.PerRecipient)
			.AddModelResolver<PerChannelResolver>(options => options.Scope = ModelResolverScope.PerChannel)
			.ChannelProvider.Add<SuccessChannelProviderDispatcher, object>("provider-1", "channel-a")
			.ChannelProvider.Add<SuccessChannelProviderDispatcher, object>("provider-2", "channel-a")
			.AddPipeline("intent", options =>
			{
				options.AddChannel(new ModelSubjectChannel("channel-a"));
				options.UseAnyMatchPipelineDeliveryStrategy();
			});

		var client = builder.BuildClient();

		var recipient = new PlatformIdentityProfile(
			"id",
			"type",
			new[] { "unit-test-address".AsIdentityAddress() });

		await client.DispatchAsync(
			"intent",
			new[] { recipient },
			TransactionModel.Create(new { }),
			Array.Empty<string>());

		Assert.AreEqual(1, ModelResolverRecorder.PerRecipientCount);
		Assert.AreEqual(2, ModelResolverRecorder.PerChannelCount);
	}

	[TestMethod]
	public async Task ModelResolvers_PreserveResourcesAndLinkedResourcesAcrossScopes()
	{
		RecordingDispatcher.Reset();

		var builder = new CommunicationsClientBuilder()
			.AddModelResolver<NoopPerRecipientResolver>(options => options.Scope = ModelResolverScope.PerRecipient)
			.ChannelProvider.Add<RecordingDispatcher, UnitTestCommunication>("provider", "channel-resources")
			.AddPipeline("intent", options =>
			{
				options.AddChannel(new ResourceSubjectChannel("channel-resources"));
				options.UseAnyMatchPipelineDeliveryStrategy();
			});

		var client = builder.BuildClient();

		var recipient = new PlatformIdentityProfile(
			"id",
			"type",
			new[] { "unit-test-address".AsIdentityAddress() });

		using var resourceStream = new MemoryStream();
		using var linkedStream = new MemoryStream();

		var transactionModel = TransactionModel.Create(
			new { },
			new Resource("res", "text/plain", resourceStream),
			new LinkedResource("lnk", linkedStream));

		await client.DispatchAsync(
			"intent",
			new[] { recipient },
			transactionModel,
			Array.Empty<string>());

		CollectionAssert.AreEqual(new[] { "res:1-lnk:1" }, RecordingDispatcher.Subjects.ToArray());
	}

	[TestMethod]
	public async Task ModelResolvers_ReplaceContentModelForDispatchContext()
	{
		ModelResolverRecorder.Reset();
		ModelReplacementRecorder.Reset();
		RecordingDispatcher.Reset();
		ModelObserverChannel.Reset();

		var builder = new CommunicationsClientBuilder()
			.AddModelResolver<ReplacePerRecipientResolver>(options => options.Scope = ModelResolverScope.PerRecipient)
			.AddModelResolver<ReplacePerChannelResolver>(options => options.Scope = ModelResolverScope.PerChannel)
			.ChannelProvider.Add<RecordingDispatcher, UnitTestCommunication>("provider", "channel-a")
			.AddPipeline("intent", options =>
			{
				options.AddChannel(new ModelObserverChannel("channel-a"));
				options.UseAnyMatchPipelineDeliveryStrategy();
			});

		var client = builder.BuildClient();

		var recipient = new PlatformIdentityProfile(
			"id",
			"type",
			new[] { "unit-test-address".AsIdentityAddress() });

		await client.DispatchAsync(
			"intent",
			new[] { recipient },
			TransactionModel.Create(new { shared = "original" }),
			Array.Empty<string>());

		Assert.AreEqual(1, ModelResolverRecorder.PerRecipientCount);
		Assert.AreEqual(1, ModelResolverRecorder.PerChannelCount);
		Assert.IsNotNull(ModelReplacementRecorder.PerRecipientInput);
		Assert.IsNotNull(ModelReplacementRecorder.PerRecipientOutput);
		Assert.IsNotNull(ModelReplacementRecorder.PerChannelInput);
		Assert.IsNotNull(ModelReplacementRecorder.PerChannelOutput);
		Assert.AreNotSame(ModelReplacementRecorder.PerRecipientInput, ModelReplacementRecorder.PerRecipientOutput);
		Assert.AreNotSame(ModelReplacementRecorder.PerChannelInput, ModelReplacementRecorder.PerChannelOutput);
		Assert.AreSame(ModelReplacementRecorder.PerChannelOutput, ModelObserverChannel.LastContentModel);
		CollectionAssert.AreEqual(new[] { "per-recipient:channel-a" }, RecordingDispatcher.Subjects.ToArray());
	}

	[TestMethod]
	public async Task ModelResolvers_AllowAsyncExternalModelReplacement()
	{
		ModelResolverRecorder.Reset();
		ModelReplacementRecorder.Reset();
		RecordingDispatcher.Reset();
		ModelObserverChannel.Reset();

		var builder = new CommunicationsClientBuilder()
			.AddModelResolver<DelayedExternalPerChannelResolver>(options => options.Scope = ModelResolverScope.PerChannel)
			.ChannelProvider.Add<RecordingDispatcher, UnitTestCommunication>("provider", "channel-a")
			.AddPipeline("intent", options =>
			{
				options.AddChannel(new ModelObserverChannel("channel-a"));
				options.UseAnyMatchPipelineDeliveryStrategy();
			});

		var client = builder.BuildClient();

		var recipient = new PlatformIdentityProfile(
			"id",
			"type",
			new[] { "unit-test-address".AsIdentityAddress() });

		await client.DispatchAsync(
			"intent",
			new[] { recipient },
			TransactionModel.Create(new { paymentId = "pay_123" }),
			Array.Empty<string>());

		Assert.AreEqual(1, ModelResolverRecorder.PerChannelCount);
		Assert.IsNotNull(ModelReplacementRecorder.PerChannelInput);
		Assert.IsNotNull(ModelReplacementRecorder.PerChannelOutput);
		Assert.AreNotSame(ModelReplacementRecorder.PerChannelInput, ModelReplacementRecorder.PerChannelOutput);
		Assert.AreSame(ModelReplacementRecorder.PerChannelOutput, ModelObserverChannel.LastContentModel);
		CollectionAssert.AreEqual(new[] { "payment:pay_123" }, RecordingDispatcher.Subjects.ToArray());
	}

	private static class ModelResolverRecorder
	{
		public static int PerRecipientCount { get; private set; }
		public static int PerChannelCount { get; private set; }

		public static void Reset()
		{
			PerRecipientCount = 0;
			PerChannelCount = 0;
		}

		public static void RecordPerRecipient() => PerRecipientCount++;

		public static void RecordPerChannel() => PerChannelCount++;
	}

	private static class ModelReplacementRecorder
	{
		public static IContentModel? PerRecipientInput { get; set; }
		public static IContentModel? PerRecipientOutput { get; set; }
		public static IContentModel? PerChannelInput { get; set; }
		public static IContentModel? PerChannelOutput { get; set; }

		public static void Reset()
		{
			PerRecipientInput = null;
			PerRecipientOutput = null;
			PerChannelInput = null;
			PerChannelOutput = null;
		}
	}

	public sealed class PerRecipientResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.RecordPerRecipient();
			if (currentModel.Model is IDictionary<string, object?> bag)
			{
				bag["shared"] = "recipient";
			}
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ReplacePerRecipientResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.RecordPerRecipient();
			ModelReplacementRecorder.PerRecipientInput = currentModel;

			var replacement = new ContentModel(
				TransactionModel.Create(new { shared = "per-recipient" }),
				context.PlatformIdentities);

			ModelReplacementRecorder.PerRecipientOutput = replacement;
			return Task.FromResult<IContentModel?>(replacement);
		}
	}

	public sealed class PerChannelResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.RecordPerChannel();
			if (currentModel.Model is IDictionary<string, object?> bag)
			{
				var baseValue = bag.TryGetValue("shared", out var value) ? value?.ToString() : "missing";
				bag["shared"] = $"{baseValue}:{context.ChannelId}";
			}
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ReplacePerChannelResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.RecordPerChannel();
			ModelReplacementRecorder.PerChannelInput = currentModel;

			var bag = (IDictionary<string, object?>)currentModel.Model;
			var baseValue = bag.TryGetValue("shared", out var value) ? value?.ToString() : "missing";
			var replacement = new ContentModel(
				TransactionModel.Create(new { shared = $"{baseValue}:{context.ChannelId}" }),
				context.PlatformIdentities);

			ModelReplacementRecorder.PerChannelOutput = replacement;
			return Task.FromResult<IContentModel?>(replacement);
		}
	}

	public sealed class DelayedExternalPerChannelResolver : IModelResolver
	{
		public async Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.RecordPerChannel();
			ModelReplacementRecorder.PerChannelInput = currentModel;

			string paymentId = "missing";
			if (currentModel.Model is IDictionary<string, object?> bag &&
				bag.TryGetValue("paymentId", out var value) &&
				value != null)
			{
				paymentId = value.ToString() ?? "missing";
			}

			await Task.Delay(25, cancellationToken);

			var replacement = new ContentModel(
				TransactionModel.Create(new { payment = new { id = paymentId } }),
				context.PlatformIdentities);

			ModelReplacementRecorder.PerChannelOutput = replacement;
			return replacement;
		}
	}

	public sealed class NoopPerRecipientResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	private sealed class ModelObserverChannel(string channelId) : IChannel
	{
		public static IContentModel? LastContentModel { get; private set; }

		public static void Reset()
		{
			LastContentModel = null;
		}

		public IEnumerable<string> AllowedChannelProviderIds => Array.Empty<string>();
		public IExtendedProperties ExtendedProperties { get; } = new ExtendedProperties();
		public Type CommunicationType => typeof(UnitTestCommunication);
		public string Id { get; } = channelId;

		public bool SupportsIdentityAddress(IPlatformIdentityAddress identityAddress)
		{
			return identityAddress.Value?.StartsWith("unit-test", StringComparison.OrdinalIgnoreCase) ?? false;
		}

		public Task<object> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
		{
			LastContentModel = communicationContext.ContentModel;

			var subject = "missing";
			if (communicationContext.ContentModel?.Model is IDictionary<string, object?> bag &&
				bag.TryGetValue("shared", out var value) &&
				value != null)
			{
				subject = value.ToString() ?? "missing";
			}
			else if (communicationContext.ContentModel?.Model is IDictionary<string, object?> fallbackBag &&
				fallbackBag.TryGetValue("payment", out var paymentObj) &&
				paymentObj is IDictionary<string, object?> paymentBag &&
				paymentBag.TryGetValue("id", out var paymentId) &&
				paymentId != null)
			{
				subject = $"payment:{paymentId}";
			}
			return Task.FromResult<object>(new UnitTestCommunication(subject));
		}
	}

	private sealed class ModelSubjectChannel(string channelId) : IChannel
	{
		public IEnumerable<string> AllowedChannelProviderIds => Array.Empty<string>();
		public IExtendedProperties ExtendedProperties { get; } = new ExtendedProperties();
		public Type CommunicationType => typeof(UnitTestCommunication);
		public string Id { get; } = channelId;

		public bool SupportsIdentityAddress(IPlatformIdentityAddress identityAddress)
		{
			return identityAddress.Value?.StartsWith("unit-test", StringComparison.OrdinalIgnoreCase) ?? false;
		}

		public Task<object> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
		{
			var subject = "missing";
			if (communicationContext.ContentModel?.Model is IDictionary<string, object?> bag &&
				bag.TryGetValue("shared", out var value) &&
				value != null)
			{
				subject = value.ToString() ?? "missing";
			}
			return Task.FromResult<object>(new UnitTestCommunication(subject));
		}
	}

	private sealed class ResourceSubjectChannel(string channelId) : IChannel
	{
		public IEnumerable<string> AllowedChannelProviderIds => Array.Empty<string>();
		public IExtendedProperties ExtendedProperties { get; } = new ExtendedProperties();
		public Type CommunicationType => typeof(UnitTestCommunication);
		public string Id { get; } = channelId;

		public bool SupportsIdentityAddress(IPlatformIdentityAddress identityAddress)
		{
			return identityAddress.Value?.StartsWith("unit-test", StringComparison.OrdinalIgnoreCase) ?? false;
		}

		public Task<object> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
		{
			var resources = communicationContext.ContentModel?.Resources?.Count ?? 0;
			var linked = communicationContext.ContentModel?.LinkedResources?.Count ?? 0;
			return Task.FromResult<object>(new UnitTestCommunication($"res:{resources}-lnk:{linked}"));
		}
	}

	public sealed class RecordingDispatcher : IChannelProviderDispatcher<UnitTestCommunication>
	{
		public static readonly ConcurrentQueue<string> Subjects = new();

		public static void Reset()
		{
			while (Subjects.TryDequeue(out _)) { }
		}

		public Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(UnitTestCommunication communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
		{
			Subjects.Enqueue(communication.Subject);
			IReadOnlyCollection<IDispatchResult?> results = [new DispatchResult(CommunicationsStatus.Success("ok"))];
			return Task.FromResult(results);
		}

		public Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(object communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
		{
			return DispatchAsync((UnitTestCommunication)communication, communicationContext, cancellationToken);
		}
	}
}
