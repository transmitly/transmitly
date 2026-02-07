using System.Collections.Concurrent;
using System.Globalization;
using Transmitly.Channel.Configuration;
using Transmitly.Model.Configuration;
using Transmitly.Tests.Mocks;

namespace Transmitly.Tests.Configuration.Model;

[TestClass]
public sealed class TransactionModelEnricherServiceTests
{
	[TestMethod]
	public async Task EnrichAsync_OrdersByOrderThenRegistrationIndex()
	{
		TransactionModelEnricherRecorder.Reset();

		var registrations = new List<ITransactionModelEnricherRegistration>
		{
			new TransactionModelEnricherRegistration(typeof(SecondEnricher), true, null, 2),
			new TransactionModelEnricherRegistration(typeof(FirstEnricher), true, null, 1),
			new TransactionModelEnricherRegistration(typeof(UnorderedEnricher), true, null, null)
		};

		var service = new DefaultTransactionModelEnricherService(new DefaultTransactionModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = TransactionModel.Create(new { });

		await service.EnrichAsync(context, model);

		CollectionAssert.AreEqual(new[] { "first", "second", "unordered" }, TransactionModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_PreservesModelWhenEnricherReturnsNull()
	{
		TransactionModelEnricherRecorder.Reset();

		var registrations = new List<ITransactionModelEnricherRegistration>
		{
			new TransactionModelEnricherRegistration(typeof(NullEnricher), true, null, 0),
			new TransactionModelEnricherRegistration(typeof(AfterEnricher), true, null, 1)
		};

		var service = new DefaultTransactionModelEnricherService(new DefaultTransactionModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = TransactionModel.Create(new Dictionary<string, object?> { ["value"] = "original" });

		var enriched = await service.EnrichAsync(context, model);

		Assert.AreSame(model, enriched);
		var bag = (IDictionary<string, object?>)model.Model;
		Assert.AreEqual("after", bag["value"]);
		CollectionAssert.AreEqual(new[] { "null", "after" }, TransactionModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_ShortCircuitsWhenConfigured()
	{
		TransactionModelEnricherRecorder.Reset();

		var registrations = new List<ITransactionModelEnricherRegistration>
		{
			new TransactionModelEnricherRegistration(typeof(ShortCircuitEnricher), false, null, 0),
			new TransactionModelEnricherRegistration(typeof(SecondEnricher), true, null, 1)
		};

		var service = new DefaultTransactionModelEnricherService(new DefaultTransactionModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = TransactionModel.Create(new { });

		await service.EnrichAsync(context, model);

		CollectionAssert.AreEqual(new[] { "short" }, TransactionModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task HasEnrichersAsync_ReportsIfAnyExist()
	{
		var registrations = new List<ITransactionModelEnricherRegistration>
		{
			new TransactionModelEnricherRegistration(typeof(FirstEnricher), true, null, null)
		};

		var service = new DefaultTransactionModelEnricherService(new DefaultTransactionModelEnricherRegistrationFactory(registrations));

		Assert.IsTrue(await service.HasEnrichersAsync());
	}

	[TestMethod]
	public async Task EnrichAsync_ThrowsOnCanceledToken()
	{
		var registrations = new List<ITransactionModelEnricherRegistration>
		{
			new TransactionModelEnricherRegistration(typeof(FirstEnricher), true, null, null)
		};

		var service = new DefaultTransactionModelEnricherService(new DefaultTransactionModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = TransactionModel.Create(new { });
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
			service.EnrichAsync(context, model, cts.Token));
	}

	private static DispatchCommunicationContext CreateDispatchContext(string pipelineIntent = "intent")
	{
		var recipients = new[]
		{
			new PlatformIdentityProfile(
				"id",
				"type",
				new[] { "unit-test-address".AsIdentityAddress() })
		};

		var contentModel = new ContentModel(TransactionModel.Create(new { }), recipients);

		return new DispatchCommunicationContext(
			contentModel,
			new MockPipelineConfiguration(),
			recipients,
			new UnitTestTemplateEngine(),
			new MockDeliveryReportService([]),
			CultureInfo.InvariantCulture,
			pipelineIntent,
			pipelineId: null);
	}

	private static class TransactionModelEnricherRecorder
	{
		private static readonly ConcurrentQueue<string> _calls = new();
		public static IReadOnlyCollection<string> Calls => _calls.ToArray();

		public static void Reset()
		{
			while (_calls.TryDequeue(out _)) { }
		}

		public static void Record(string value) => _calls.Enqueue(value);
	}

	public sealed class FirstEnricher : ITransactionModelEnricher
	{
		public Task<ITransactionModel?> EnrichAsync(IDispatchCommunicationContext context, ITransactionModel currentModel, CancellationToken cancellationToken = default)
		{
			TransactionModelEnricherRecorder.Record("first");
			return Task.FromResult<ITransactionModel?>(currentModel);
		}
	}

	public sealed class SecondEnricher : ITransactionModelEnricher
	{
		public Task<ITransactionModel?> EnrichAsync(IDispatchCommunicationContext context, ITransactionModel currentModel, CancellationToken cancellationToken = default)
		{
			TransactionModelEnricherRecorder.Record("second");
			return Task.FromResult<ITransactionModel?>(currentModel);
		}
	}

	public sealed class UnorderedEnricher : ITransactionModelEnricher
	{
		public Task<ITransactionModel?> EnrichAsync(IDispatchCommunicationContext context, ITransactionModel currentModel, CancellationToken cancellationToken = default)
		{
			TransactionModelEnricherRecorder.Record("unordered");
			return Task.FromResult<ITransactionModel?>(currentModel);
		}
	}

	public sealed class NullEnricher : ITransactionModelEnricher
	{
		public Task<ITransactionModel?> EnrichAsync(IDispatchCommunicationContext context, ITransactionModel currentModel, CancellationToken cancellationToken = default)
		{
			TransactionModelEnricherRecorder.Record("null");
			return Task.FromResult<ITransactionModel?>(null);
		}
	}

	public sealed class AfterEnricher : ITransactionModelEnricher
	{
		public Task<ITransactionModel?> EnrichAsync(IDispatchCommunicationContext context, ITransactionModel currentModel, CancellationToken cancellationToken = default)
		{
			if (currentModel.Model is IDictionary<string, object?> bag)
			{
				bag["value"] = "after";
			}
			TransactionModelEnricherRecorder.Record("after");
			return Task.FromResult<ITransactionModel?>(currentModel);
		}
	}

	public sealed class ShortCircuitEnricher : ITransactionModelEnricher
	{
		public Task<ITransactionModel?> EnrichAsync(IDispatchCommunicationContext context, ITransactionModel currentModel, CancellationToken cancellationToken = default)
		{
			TransactionModelEnricherRecorder.Record("short");
			return Task.FromResult<ITransactionModel?>(currentModel);
		}
	}
}
