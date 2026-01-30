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
public sealed class ModelResolverServiceTests
{
	[TestMethod]
	public async Task ResolveAsync_OrdersByOrderThenRegistrationIndex()
	{
		ModelResolverRecorder.Reset();

		var registrations = new List<IModelResolverRegistration>
		{
			new ModelResolverRegistration(typeof(SecondResolver), ModelResolverScope.PerChannel, true, null, 2),
			new ModelResolverRegistration(typeof(FirstResolver), ModelResolverScope.PerChannel, true, null, 1),
			new ModelResolverRegistration(typeof(UnorderedResolver), ModelResolverScope.PerChannel, true, null, null)
		};

		var service = new DefaultModelResolverService(new DefaultModelResolverRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		await service.ResolveAsync(context, model, ModelResolverScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "first", "second", "unordered" }, ModelResolverRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task ResolveAsync_PreservesModelWhenResolverReturnsNull()
	{
		ModelResolverRecorder.Reset();

		var registrations = new List<IModelResolverRegistration>
		{
			new ModelResolverRegistration(typeof(NullResolver), ModelResolverScope.PerChannel, true, null, 0),
			new ModelResolverRegistration(typeof(AfterResolver), ModelResolverScope.PerChannel, true, null, 1)
		};

		var service = new DefaultModelResolverService(new DefaultModelResolverRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);
		var bag = (IDictionary<string, object?>)model.Model;
		bag["value"] = "original";

		var resolved = await service.ResolveAsync(context, model, ModelResolverScope.PerChannel);

		Assert.AreSame(model, resolved);
		Assert.AreEqual("after", bag["value"]);
		CollectionAssert.AreEqual(new[] { "null", "after" }, ModelResolverRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task ResolveAsync_ReplacesModelAndDownstreamSeesNewModel()
	{
		ModelResolverRecorder.Reset();

		var registrations = new List<IModelResolverRegistration>
		{
			new ModelResolverRegistration(typeof(ReplaceResolver), ModelResolverScope.PerChannel, true, null, 0),
			new ModelResolverRegistration(typeof(InspectResolver), ModelResolverScope.PerChannel, true, null, 1)
		};

		var service = new DefaultModelResolverService(new DefaultModelResolverRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		var resolved = await service.ResolveAsync(context, model, ModelResolverScope.PerChannel);
		var resolvedBag = (IDictionary<string, object?>)resolved.Model;
		var originalBag = (IDictionary<string, object?>)model.Model;

		Assert.AreNotSame(model, resolved);
		Assert.IsFalse(originalBag.ContainsKey("Marker"));
		Assert.AreEqual("replaced", resolvedBag["Marker"]);
		CollectionAssert.AreEqual(new[] { "replace", "inspect" }, ModelResolverRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task ResolveAsync_AllowsAsyncExternalReplacement()
	{
		ModelResolverRecorder.Reset();

		var registrations = new List<IModelResolverRegistration>
		{
			new ModelResolverRegistration(typeof(DelayedReplaceResolver), ModelResolverScope.PerChannel, true, null, 0),
			new ModelResolverRegistration(typeof(ExternalInspectResolver), ModelResolverScope.PerChannel, true, null, 1)
		};

		var service = new DefaultModelResolverService(new DefaultModelResolverRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		var resolved = await service.ResolveAsync(context, model, ModelResolverScope.PerChannel);
		var resolvedBag = (IDictionary<string, object?>)resolved.Model;

		Assert.AreEqual("external", resolvedBag["Marker"]);
		CollectionAssert.AreEqual(new[] { "delayed-replace", "external-inspect" }, ModelResolverRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task ResolveAsync_RespectsPredicateUsingChannelId()
	{
		ModelResolverRecorder.Reset();

		var registrations = new List<IModelResolverRegistration>
		{
			new ModelResolverRegistration(typeof(ChannelPredicateResolver), ModelResolverScope.PerChannel, true, ctx => ctx.ChannelId == "channel-a", null),
			new ModelResolverRegistration(typeof(SecondResolver), ModelResolverScope.PerChannel, true, ctx => ctx.ChannelId == "channel-b", null)
		};

		var service = new DefaultModelResolverService(new DefaultModelResolverRegistrationFactory(registrations));
		var context = CreateDispatchContext(channelId: "channel-a");
		var model = CreateContentModel(context.PlatformIdentities);

		await service.ResolveAsync(context, model, ModelResolverScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "channel" }, ModelResolverRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task ResolveAsync_SkipsPredicateWhenFalse()
	{
		ModelResolverRecorder.Reset();

		var registrations = new List<IModelResolverRegistration>
		{
			new ModelResolverRegistration(typeof(FirstResolver), ModelResolverScope.PerChannel, true, ctx => ctx.PipelineIntent == "match", null),
			new ModelResolverRegistration(typeof(SecondResolver), ModelResolverScope.PerChannel, true, ctx => ctx.PipelineIntent == "nope", null)
		};

		var service = new DefaultModelResolverService(new DefaultModelResolverRegistrationFactory(registrations));
		var context = CreateDispatchContext(pipelineIntent: "match");
		var model = CreateContentModel(context.PlatformIdentities);

		await service.ResolveAsync(context, model, ModelResolverScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "first" }, ModelResolverRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task ResolveAsync_RespectsRegistrationOrderWhenOrderIsNull()
	{
		ModelResolverRecorder.Reset();

		var registrations = new List<IModelResolverRegistration>
		{
			new ModelResolverRegistration(typeof(NullOrderedFirstResolver), ModelResolverScope.PerChannel, true, null, null),
			new ModelResolverRegistration(typeof(NullOrderedSecondResolver), ModelResolverScope.PerChannel, true, null, null)
		};

		var service = new DefaultModelResolverService(new DefaultModelResolverRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		await service.ResolveAsync(context, model, ModelResolverScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "null-first", "null-second" }, ModelResolverRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task ResolveAsync_ShortCircuitsWhenConfigured()
	{
		ModelResolverRecorder.Reset();

		var registrations = new List<IModelResolverRegistration>
		{
			new ModelResolverRegistration(typeof(ShortCircuitResolver), ModelResolverScope.PerChannel, false, null, 0),
			new ModelResolverRegistration(typeof(SecondResolver), ModelResolverScope.PerChannel, true, null, 1)
		};

		var service = new DefaultModelResolverService(new DefaultModelResolverRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);

		await service.ResolveAsync(context, model, ModelResolverScope.PerChannel);

		CollectionAssert.AreEqual(new[] { "short" }, ModelResolverRecorder.Calls.ToArray());
	}

	[TestMethod]
	public async Task HasResolversAsync_ReportsByScope()
	{
		var registrations = new List<IModelResolverRegistration>
		{
			new ModelResolverRegistration(typeof(FirstResolver), ModelResolverScope.PerRecipient, true, null, null)
		};

		var service = new DefaultModelResolverService(new DefaultModelResolverRegistrationFactory(registrations));

		Assert.IsTrue(await service.HasResolversAsync(ModelResolverScope.PerRecipient));
		Assert.IsFalse(await service.HasResolversAsync(ModelResolverScope.PerChannel));
	}

	[TestMethod]
	public async Task ResolveAsync_ThrowsOnCanceledToken()
	{
		var registrations = new List<IModelResolverRegistration>
		{
			new ModelResolverRegistration(typeof(FirstResolver), ModelResolverScope.PerChannel, true, null, null)
		};

		var service = new DefaultModelResolverService(new DefaultModelResolverRegistrationFactory(registrations));
		var context = CreateDispatchContext();
		var model = CreateContentModel(context.PlatformIdentities);
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
			service.ResolveAsync(context, model, ModelResolverScope.PerChannel, cts.Token));
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

	private static class ModelResolverRecorder
	{
		private static readonly ConcurrentQueue<string> _calls = new();
		public static IReadOnlyCollection<string> Calls => _calls.ToArray();

		public static void Reset()
		{
			while (_calls.TryDequeue(out _)) {/*empty*/ }
		}

		public static void Record(string value) => _calls.Enqueue(value);
	}

	public sealed class FirstResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.Record("first");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class SecondResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.Record("second");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class UnorderedResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.Record("unordered");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class NullResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.Record("null");
			return Task.FromResult<IContentModel?>(null);
		}
	}

	public sealed class AfterResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			if (currentModel.Model is IDictionary<string, object?> bag)
			{
				bag["value"] = "after";
			}
			ModelResolverRecorder.Record("after");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ReplaceResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.Record("replace");
			var replacement = new ContentModel(TransactionModel.Create(new { Marker = "replaced" }), context.PlatformIdentities);
			return Task.FromResult<IContentModel?>(replacement);
		}
	}

	public sealed class DelayedReplaceResolver : IModelResolver
	{
		public async Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.Record("delayed-replace");
			await Task.Delay(25, cancellationToken);
			var replacement = new ContentModel(TransactionModel.Create(new { Marker = "external" }), context.PlatformIdentities);
			return replacement;
		}
	}

	public sealed class InspectResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			var bag = (IDictionary<string, object?>)currentModel.Model;
			if (bag.TryGetValue("Marker", out var value) && value?.ToString() == "replaced")
			{
				ModelResolverRecorder.Record("inspect");
			}
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ExternalInspectResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			var bag = (IDictionary<string, object?>)currentModel.Model;
			if (bag.TryGetValue("Marker", out var value) && value?.ToString() == "external")
			{
				ModelResolverRecorder.Record("external-inspect");
			}
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ChannelPredicateResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.Record("channel");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class NullOrderedFirstResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.Record("null-first");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class NullOrderedSecondResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.Record("null-second");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}

	public sealed class ShortCircuitResolver : IModelResolver
	{
		public Task<IContentModel?> ResolveAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			ModelResolverRecorder.Record("short");
			return Task.FromResult<IContentModel?>(currentModel);
		}
	}
}
