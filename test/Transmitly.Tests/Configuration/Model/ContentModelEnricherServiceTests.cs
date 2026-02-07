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
public sealed class ContentModelEnricherServiceTests
{
	[TestMethod]
	public async Task EnrichAsync_OrdersByOrderThenRegistrationIndex()
	{
		ContentModelEnricherRecorder.Reset();

		var registrations = new List<IContentModelEnricherRegistration>
		{
			new ContentModelEnricherRegistration(typeof(SecondEnricher), ContentModelEnricherScope.PerChannel, true, null, 2),
			new ContentModelEnricherRegistration(typeof(FirstEnricher), ContentModelEnricherScope.PerChannel, true, null, 1),
			new ContentModelEnricherRegistration(typeof(UnorderedEnricher), ContentModelEnricherScope.PerChannel, true, null, null)
		};

		var service = new DefaultContentModelEnricherService(new DefaultContentModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		await service.EnrichAsync(context, model, ContentModelEnricherScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "first", "second", "unordered" }, ContentModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_PreservesModelWhenEnricherReturnsNull()
	{
		ContentModelEnricherRecorder.Reset();

		var registrations = new List<IContentModelEnricherRegistration>
		{
			new ContentModelEnricherRegistration(typeof(NullEnricher), ContentModelEnricherScope.PerChannel, true, null, 0),
			new ContentModelEnricherRegistration(typeof(AfterEnricher), ContentModelEnricherScope.PerChannel, true, null, 1)
		};

		var service = new DefaultContentModelEnricherService(new DefaultContentModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);
		var bag = (IDictionary<string, object?>)model.Model;
		bag["value"] = "original";

		var enriched = await service.EnrichAsync(context, model, ContentModelEnricherScope.PerChannel);

		Assert.AreSame(model, enriched);
		Assert.AreEqual("after", bag["value"]);
		CollectionAssert.AreEqual(new[] { "null", "after" }, ContentModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_ReplacesModelAndDownstreamSeesNewModel()
	{
		ContentModelEnricherRecorder.Reset();

		var registrations = new List<IContentModelEnricherRegistration>
		{
			new ContentModelEnricherRegistration(typeof(ReplaceEnricher), ContentModelEnricherScope.PerChannel, true, null, 0),
			new ContentModelEnricherRegistration(typeof(InspectEnricher), ContentModelEnricherScope.PerChannel, true, null, 1)
		};

		var service = new DefaultContentModelEnricherService(new DefaultContentModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		var enriched = await service.EnrichAsync(context, model, ContentModelEnricherScope.PerChannel);
		var enrichedBag = (IDictionary<string, object?>)enriched.Model;
		var originalBag = (IDictionary<string, object?>)model.Model;

		Assert.AreNotSame(model, enriched);
		Assert.IsFalse(originalBag.ContainsKey("Marker"));
		Assert.AreEqual("replaced", enrichedBag["Marker"]);
		CollectionAssert.AreEqual(new[] { "replace", "inspect" }, ContentModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_AllowsAsyncExternalReplacement()
	{
		ContentModelEnricherRecorder.Reset();

		var registrations = new List<IContentModelEnricherRegistration>
		{
			new ContentModelEnricherRegistration(typeof(DelayedReplaceEnricher), ContentModelEnricherScope.PerChannel, true, null, 0),
			new ContentModelEnricherRegistration(typeof(ExternalInspectEnricher), ContentModelEnricherScope.PerChannel, true, null, 1)
		};

		var service = new DefaultContentModelEnricherService(new DefaultContentModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		var enriched = await service.EnrichAsync(context, model, ContentModelEnricherScope.PerChannel);
		var enrichedBag = (IDictionary<string, object?>)enriched.Model;

		Assert.AreEqual("external", enrichedBag["Marker"]);
		CollectionAssert.AreEqual(new[] { "delayed-replace", "external-inspect" }, ContentModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_RespectsPredicateUsingChannelId()
	{
		ContentModelEnricherRecorder.Reset();

		var registrations = new List<IContentModelEnricherRegistration>
		{
			new ContentModelEnricherRegistration(typeof(ChannelPredicateEnricher), ContentModelEnricherScope.PerChannel, true, ctx => ctx.ChannelId == "channel-a", null),
			new ContentModelEnricherRegistration(typeof(SecondEnricher), ContentModelEnricherScope.PerChannel, true, ctx => ctx.ChannelId == "channel-b", null)
		};

		var service = new DefaultContentModelEnricherService(new DefaultContentModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext(channelId: "channel-a");
		var model = CreateContentModel(context.PlatformIdentities);

		await service.EnrichAsync(context, model, ContentModelEnricherScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "channel" }, ContentModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_SkipsPredicateWhenFalse()
	{
		ContentModelEnricherRecorder.Reset();

		var registrations = new List<IContentModelEnricherRegistration>
		{
			new ContentModelEnricherRegistration(typeof(FirstEnricher), ContentModelEnricherScope.PerChannel, true, ctx => ctx.PipelineIntent == "match", null),
			new ContentModelEnricherRegistration(typeof(SecondEnricher), ContentModelEnricherScope.PerChannel, true, ctx => ctx.PipelineIntent == "nope", null)
		};

		var service = new DefaultContentModelEnricherService(new DefaultContentModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext(pipelineIntent: "match");
		var model = CreateContentModel(context.PlatformIdentities);

		await service.EnrichAsync(context, model, ContentModelEnricherScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "first" }, ContentModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_RespectsRegistrationOrderWhenOrderIsNull()
	{
		ContentModelEnricherRecorder.Reset();

		var registrations = new List<IContentModelEnricherRegistration>
		{
			new ContentModelEnricherRegistration(typeof(NullOrderedFirstEnricher), ContentModelEnricherScope.PerChannel, true, null, null),
			new ContentModelEnricherRegistration(typeof(NullOrderedSecondEnricher), ContentModelEnricherScope.PerChannel, true, null, null)
		};

		var service = new DefaultContentModelEnricherService(new DefaultContentModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		await service.EnrichAsync(context, model, ContentModelEnricherScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "null-first", "null-second" }, ContentModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task EnrichAsync_ShortCircuitsWhenConfigured()
	{
		ContentModelEnricherRecorder.Reset();

		var registrations = new List<IContentModelEnricherRegistration>
		{
			new ContentModelEnricherRegistration(typeof(ShortCircuitEnricher), ContentModelEnricherScope.PerChannel, false, null, 0),
			new ContentModelEnricherRegistration(typeof(SecondEnricher), ContentModelEnricherScope.PerChannel, true, null, 1)
		};

		var service = new DefaultContentModelEnricherService(new DefaultContentModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		await service.EnrichAsync(context, model, ContentModelEnricherScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "short" }, ContentModelEnricherRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task HasEnrichersAsync_ReportsByScope()
	{
		var registrations = new List<IContentModelEnricherRegistration>
		{
			new ContentModelEnricherRegistration(typeof(FirstEnricher), ContentModelEnricherScope.PerRecipient, true, null, null)
		};

		var service = new DefaultContentModelEnricherService(new DefaultContentModelEnricherRegistrationFactory(registrations));

		Assert.IsTrue(await service.HasEnrichersAsync(ContentModelEnricherScope.PerRecipient));
		Assert.IsFalse(await service.HasEnrichersAsync(ContentModelEnricherScope.PerChannel));
	}

	[TestMethod]
	public async Task EnrichAsync_ThrowsOnCanceledToken()
	{
		var registrations = new List<IContentModelEnricherRegistration>
		{
			new ContentModelEnricherRegistration(typeof(FirstEnricher), ContentModelEnricherScope.PerChannel, true, null, null)
		};

		var service = new DefaultContentModelEnricherService(new DefaultContentModelEnricherRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
			service.EnrichAsync(context, model, ContentModelEnricherScope.PerChannel, cts.Token));
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

	private static class ContentModelEnricherRecorder
	{
		private static readonly ConcurrentQueue<string> _calls = new();
		public static IReadOnlyCollection<string> Calls => _calls.ToArray();

		public static void Reset()
		{
			while (_calls.TryDequeue(out _)) {/*empty*/ }
		}

		public static void Record(string value) => _calls.Enqueue(value);
	}

	public sealed class FirstEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ContentModelEnricherRecorder.Record("first");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class SecondEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ContentModelEnricherRecorder.Record("second");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class UnorderedEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ContentModelEnricherRecorder.Record("unordered");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class NullEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ContentModelEnricherRecorder.Record("null");
			return Task.FromResult<IContentModel?>(null);
		}
	}

	public sealed class AfterEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			if (currentModel.Model is IDictionary<string, object?> bag)
			{
				bag["value"] = "after";
			}
			ContentModelEnricherRecorder.Record("after");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ReplaceEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ContentModelEnricherRecorder.Record("replace");
			var replacement = new ContentModel(TransactionModel.Create(new { Marker = "replaced" }), context.PlatformIdentities);
			return Task.FromResult<IContentModel?>(replacement);
		}
	}

	public sealed class DelayedReplaceEnricher : IContentModelEnricher
	{
		public async Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ContentModelEnricherRecorder.Record("delayed-replace");
			await Task.Delay(25, cancellationToken);
			var replacement = new ContentModel(TransactionModel.Create(new { Marker = "external" }), context.PlatformIdentities);
			return replacement;
		}
	}

	public sealed class InspectEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			var bag = (IDictionary<string, object?>)currentModel.Model;
			if (bag.TryGetValue("Marker", out var value) && value?.ToString() == "replaced")
			{
				ContentModelEnricherRecorder.Record("inspect");
			}
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ExternalInspectEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			var bag = (IDictionary<string, object?>)currentModel.Model;
			if (bag.TryGetValue("Marker", out var value) && value?.ToString() == "external")
			{
				ContentModelEnricherRecorder.Record("external-inspect");
			}
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ChannelPredicateEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ContentModelEnricherRecorder.Record("channel");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class NullOrderedFirstEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ContentModelEnricherRecorder.Record("null-first");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class NullOrderedSecondEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ContentModelEnricherRecorder.Record("null-second");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ShortCircuitEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ContentModelEnricherRecorder.Record("short");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}
}
