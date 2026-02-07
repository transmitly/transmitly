using System.Collections.Concurrent;
using Transmitly.Channel.Configuration;
using Transmitly.ChannelProvider;
using Transmitly.Model.Configuration;
using Transmitly.Tests.Mocks;

namespace Transmitly.Tests.Configuration.Model;

[TestClass]
public sealed class TransactionModelEnricherDispatchTests
{
	[TestMethod]
	public async Task TransactionModelEnrichers_RunOnceAndFeedContentEnrichers()
	{
		TransactionEnricherRecorder.Reset();
		ContentEnricherRecorder.Reset();
		RecordingDispatcher.Reset();

		var builder = new CommunicationsClientBuilder()
			.AddTransactionModelEnricher<SharedTransactionEnricher>()
			.AddContentModelEnricher<PerChannelEnricher>(options => options.Scope = ContentModelEnricherScope.PerChannel)
			.ChannelProvider.Add<RecordingDispatcher, UnitTestCommunication>("provider", "channel-a", "channel-b")
			.AddPipeline("intent", options =>
			{
				options.AddChannel(new SubjectChannel("channel-a"));
				options.AddChannel(new SubjectChannel("channel-b"));
				options.UseAnyMatchPipelineDeliveryStrategy();
			});

		var client = builder.BuildClient();

		var recipient = new PlatformIdentityProfile(
			"id",
			"type",
			new[] { "unit-test-address".AsIdentityAddress() });

		var transactionModel = TransactionModel.Create(new Dictionary<string, object?> { ["shared"] = "original" });

		await client.DispatchAsync(
			"intent",
			new[] { recipient },
			transactionModel,
			Array.Empty<string>());

		Assert.AreEqual(1, TransactionEnricherRecorder.Count);
		Assert.AreEqual(2, ContentEnricherRecorder.PerChannelCount);
		CollectionAssert.AreEquivalent(
			new[] { "transaction:channel-a", "transaction:channel-b" },
			RecordingDispatcher.Subjects.ToArray());
	}

	private static class TransactionEnricherRecorder
	{
		public static int Count { get; private set; }

		public static void Reset()
		{
			Count = 0;
		}

		public static void Record() => Count++;
	}

	private static class ContentEnricherRecorder
	{
		public static int PerChannelCount { get; private set; }

		public static void Reset()
		{
			PerChannelCount = 0;
		}

		public static void RecordPerChannel() => PerChannelCount++;
	}

	public sealed class SharedTransactionEnricher : ITransactionModelEnricher
	{
		public Task<ITransactionModel?> EnrichAsync(IDispatchCommunicationContext context, ITransactionModel currentModel, CancellationToken cancellationToken = default)
		{
			TransactionEnricherRecorder.Record();
			if (currentModel.Model is IDictionary<string, object?> bag)
			{
				bag["shared"] = "transaction";
			}
			return Task.FromResult<ITransactionModel?>(currentModel);
		}
	}

	public sealed class PerChannelEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ContentEnricherRecorder.RecordPerChannel();
			if (currentModel.Model is IDictionary<string, object?> bag)
			{
				var baseValue = bag.TryGetValue("shared", out var value) ? value?.ToString() : "missing";
				bag["shared"] = $"{baseValue}:{context.ChannelId}";
			}
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	private sealed class SubjectChannel(string channelId) : IChannel
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
