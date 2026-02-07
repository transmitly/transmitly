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
using System.Globalization;
using Transmitly.Channel.Configuration;
using Transmitly.Model.Configuration;
using Transmitly.Tests.Mocks;

namespace Transmitly.Tests.Configuration.Model;

[TestClass]
public sealed class ModelEnricherServiceTests
{
	[TestMethod]
	public async Task EnrichAsync_OrdersByOrderThenRegistrationIndex()
	{
		ModelEnricherRecorder.Reset();

		var registrations = new List<IModelEnricherRegistration>
		{
			new ModelEnricherRegistration(typeof(SecondEnricher), ModelEnricherScope.PerChannel, true, null, 2),
			new ModelEnricherRegistration(typeof(FirstEnricher), ModelEnricherScope.PerChannel, true, null, 1),
			new ModelEnricherRegistration(typeof(UnorderedEnricher), ModelEnricherScope.PerChannel, true, null, null)
		};

		var service = new DefaultModelEnricherService(new DefaultModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		await service.EnrichAsync(context, model, ModelEnricherScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "first", "second", "unordered" }, ModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_PreservesModelWhenEnricherReturnsNull()
	{
		ModelEnricherRecorder.Reset();

		var registrations = new List<IModelEnricherRegistration>
		{
			new ModelEnricherRegistration(typeof(NullEnricher), ModelEnricherScope.PerChannel, true, null, 0),
			new ModelEnricherRegistration(typeof(AfterEnricher), ModelEnricherScope.PerChannel, true, null, 1)
		};

		var service = new DefaultModelEnricherService(new DefaultModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);
		var bag = (IDictionary<string, object?>)model.Model;
		bag["value"] = "original";

		var enriched = await service.EnrichAsync(context, model, ModelEnricherScope.PerChannel);

		Assert.AreSame(model, enriched);
		Assert.AreEqual("after", bag["value"]);
		CollectionAssert.AreEqual(new[] { "null", "after" }, ModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_ReplacesModelAndDownstreamSeesNewModel()
	{
		ModelEnricherRecorder.Reset();

		var registrations = new List<IModelEnricherRegistration>
		{
			new ModelEnricherRegistration(typeof(ReplaceEnricher), ModelEnricherScope.PerChannel, true, null, 0),
			new ModelEnricherRegistration(typeof(InspectEnricher), ModelEnricherScope.PerChannel, true, null, 1)
		};

		var service = new DefaultModelEnricherService(new DefaultModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		var enriched = await service.EnrichAsync(context, model, ModelEnricherScope.PerChannel);
		var enrichedBag = (IDictionary<string, object?>)enriched.Model;
		var originalBag = (IDictionary<string, object?>)model.Model;

		Assert.AreNotSame(model, enriched);
		Assert.IsFalse(originalBag.ContainsKey("Marker"));
		Assert.AreEqual("replaced", enrichedBag["Marker"]);
		CollectionAssert.AreEqual(new[] { "replace", "inspect" }, ModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_AllowsAsyncExternalReplacement()
	{
		ModelEnricherRecorder.Reset();

		var registrations = new List<IModelEnricherRegistration>
		{
			new ModelEnricherRegistration(typeof(DelayedReplaceEnricher), ModelEnricherScope.PerChannel, true, null, 0),
			new ModelEnricherRegistration(typeof(ExternalInspectEnricher), ModelEnricherScope.PerChannel, true, null, 1)
		};

		var service = new DefaultModelEnricherService(new DefaultModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		var enriched = await service.EnrichAsync(context, model, ModelEnricherScope.PerChannel);
		var enrichedBag = (IDictionary<string, object?>)enriched.Model;

		Assert.AreEqual("external", enrichedBag["Marker"]);
		CollectionAssert.AreEqual(new[] { "delayed-replace", "external-inspect" }, ModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_RespectsPredicateUsingChannelId()
	{
		ModelEnricherRecorder.Reset();

		var registrations = new List<IModelEnricherRegistration>
		{
			new ModelEnricherRegistration(typeof(ChannelPredicateEnricher), ModelEnricherScope.PerChannel, true, ctx => ctx.ChannelId == "channel-a", null),
			new ModelEnricherRegistration(typeof(SecondEnricher), ModelEnricherScope.PerChannel, true, ctx => ctx.ChannelId == "channel-b", null)
		};

		var service = new DefaultModelEnricherService(new DefaultModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext(channelId: "channel-a");
		var model = CreateContentModel(context.PlatformIdentities);

		await service.EnrichAsync(context, model, ModelEnricherScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "channel" }, ModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_SkipsPredicateWhenFalse()
	{
		ModelEnricherRecorder.Reset();

		var registrations = new List<IModelEnricherRegistration>
		{
			new ModelEnricherRegistration(typeof(FirstEnricher), ModelEnricherScope.PerChannel, true, ctx => ctx.PipelineIntent == "match", null),
			new ModelEnricherRegistration(typeof(SecondEnricher), ModelEnricherScope.PerChannel, true, ctx => ctx.PipelineIntent == "nope", null)
		};

		var service = new DefaultModelEnricherService(new DefaultModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext(pipelineIntent: "match");
		var model = CreateContentModel(context.PlatformIdentities);

		await service.EnrichAsync(context, model, ModelEnricherScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "first" }, ModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_RespectsRegistrationOrderWhenOrderIsNull()
	{
		ModelEnricherRecorder.Reset();

		var registrations = new List<IModelEnricherRegistration>
		{
			new ModelEnricherRegistration(typeof(NullOrderedFirstEnricher), ModelEnricherScope.PerChannel, true, null, null),
			new ModelEnricherRegistration(typeof(NullOrderedSecondEnricher), ModelEnricherScope.PerChannel, true, null, null)
		};

		var service = new DefaultModelEnricherService(new DefaultModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		await service.EnrichAsync(context, model, ModelEnricherScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "null-first", "null-second" }, ModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_ShortCircuitsWhenConfigured()
	{
		ModelEnricherRecorder.Reset();

		var registrations = new List<IModelEnricherRegistration>
		{
			new ModelEnricherRegistration(typeof(ShortCircuitEnricher), ModelEnricherScope.PerChannel, false, null, 0),
			new ModelEnricherRegistration(typeof(SecondEnricher), ModelEnricherScope.PerChannel, true, null, 1)
		};

		var service = new DefaultModelEnricherService(new DefaultModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		await service.EnrichAsync(context, model, ModelEnricherScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "short" }, ModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task HasEnrichersAsync_ReportsByScope()
	{
		var registrations = new List<IModelEnricherRegistration>
		{
			new ModelEnricherRegistration(typeof(FirstEnricher), ModelEnricherScope.PerRecipient, true, null, null)
		};

		var service = new DefaultModelEnricherService(new DefaultModelEnricherRegistrationFactory(registrations));

		Assert.IsTrue(await service.HasEnrichersAsync(ModelEnricherScope.PerRecipient));
		Assert.IsFalse(await service.HasEnrichersAsync(ModelEnricherScope.PerChannel));
	}

	[TestMethod]
	public async Task EnrichAsync_ThrowsOnCanceledToken()
	{
		var registrations = new List<IModelEnricherRegistration>
		{
			new ModelEnricherRegistration(typeof(FirstEnricher), ModelEnricherScope.PerChannel, true, null, null)
		};

		var service = new DefaultModelEnricherService(new DefaultModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
			service.EnrichAsync(context, model, ModelEnricherScope.PerChannel, cts.Token));
	}

	private static DispatchCommunicationContext CreateDispatchContext(string pipelineIntent = "intent", string? channelId = null, string? channelProviderId = null)
	{
		var recipients = new[]
		{
			new PlatformIdentityProfile(
				"id",
				"type",
				new[] { "unit-test-address".AsIdentityAddress() })
		};

		var contentModel = CreateContentModel(recipients);

		return new DispatchCommunicationContext(
			contentModel,
			new MockPipelineConfiguration(),
			recipients,
			new UnitTestTemplateEngine(),
			new MockDeliveryReportService([]),
			CultureInfo.InvariantCulture,
			pipelineIntent,
			pipelineId: null,
			messagePriority: MessagePriority.Normal,
			transportPriority: TransportPriority.Normal,
			ChannelId: channelId,
			ChannelProviderId: channelProviderId);
	}

	private static ContentModel CreateContentModel(IReadOnlyCollection<IPlatformIdentityProfile> recipients)
	{
		return new ContentModel(
			TransactionModel.Create(new { }),
			recipients);
	}

	private static class ModelEnricherRecorder
	{
		private static readonly ConcurrentQueue<string> _calls = new();
		public static IReadOnlyCollection<string> Calls => _calls.ToArray();

		public static void Reset()
		{
			while (_calls.TryDequeue(out _)) {/*empty*/ }
		}

		public static void Record(string value) => _calls.Enqueue(value);
	}

	public sealed class FirstEnricher : IModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelEnricherRecorder.Record("first");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class SecondEnricher : IModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelEnricherRecorder.Record("second");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class UnorderedEnricher : IModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelEnricherRecorder.Record("unordered");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class NullEnricher : IModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelEnricherRecorder.Record("null");
			return Task.FromResult<IContentModel?>(null);
		}
	}

	public sealed class AfterEnricher : IModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			if (currentModel.Model is IDictionary<string, object?> bag)
			{
				bag["value"] = "after";
			}
			ModelEnricherRecorder.Record("after");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ReplaceEnricher : IModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelEnricherRecorder.Record("replace");
			var replacement = new ContentModel(TransactionModel.Create(new { Marker = "replaced" }), context.PlatformIdentities);
			return Task.FromResult<IContentModel?>(replacement);
		}
	}

	public sealed class DelayedReplaceEnricher : IModelEnricher
	{
		public async Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelEnricherRecorder.Record("delayed-replace");
			await Task.Delay(25, cancellationToken);
			var replacement = new ContentModel(TransactionModel.Create(new { Marker = "external" }), context.PlatformIdentities);
			return replacement;
		}
	}

	public sealed class InspectEnricher : IModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			var bag = (IDictionary<string, object?>)currentModel.Model;
			if (bag.TryGetValue("Marker", out var value) && value?.ToString() == "replaced")
			{
				ModelEnricherRecorder.Record("inspect");
			}
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ExternalInspectEnricher : IModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			var bag = (IDictionary<string, object?>)currentModel.Model;
			if (bag.TryGetValue("Marker", out var value) && value?.ToString() == "external")
			{
				ModelEnricherRecorder.Record("external-inspect");
			}
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ChannelPredicateEnricher : IModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelEnricherRecorder.Record("channel");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class NullOrderedFirstEnricher : IModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelEnricherRecorder.Record("null-first");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class NullOrderedSecondEnricher : IModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelEnricherRecorder.Record("null-second");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ShortCircuitEnricher : IModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelEnricherRecorder.Record("short");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}
}
