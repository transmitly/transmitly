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

using System.Collections;
using System.Reflection;
using Transmitly.KitchenSink.AspNetCoreWebApi.Controllers;
using Transmitly.Model.Configuration;
using Transmitly.PlatformIdentity.Configuration;

namespace Transmitly.KitchenSink.AspNetCoreWebApi
{
	// This enricher demonstrates a DDD-style profile hydration step.
	// In a real system this could call separate read models from Customer and Loyalty bounded contexts.
	// We keep those calls in-memory here so the sample is self-contained and easy to run.
	public sealed class HydrateCustomerProfileFromReadModelsEnricher : IPlatformIdentityProfileEnricher
	{
		public Task EnrichIdentityProfileAsync(IPlatformIdentityProfile identityProfile)
		{
			if (identityProfile is not DispatchPlatformIdentityProfile profile)
				return Task.CompletedTask;

			if (string.IsNullOrWhiteSpace(profile.Id))
				return Task.CompletedTask;

			var customer = SampleCustomerDirectoryService.GetById(profile.Id);
			var loyalty = SampleLoyaltyReadModelService.GetByCustomerId(profile.Id);

			profile.Attributes ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (customer != null)
			{
				profile.Attributes["customerDisplayName"] = customer.DisplayName;
				profile.Attributes["customerRegion"] = customer.Region;
				profile.Attributes["customerPreferredCulture"] = customer.PreferredCulture;

				// If the caller only passed an id, this shows how profile enrichers can backfill addresses.
				if (profile.Addresses.Count == 0)
				{
					if (!string.IsNullOrWhiteSpace(customer.PrimaryEmail))
						profile.Addresses.Add(new DispatchPlatformIdentityAddress(customer.PrimaryEmail) { Display = customer.DisplayName });

					if (!string.IsNullOrWhiteSpace(customer.MobilePhone))
						profile.Addresses.Add(new DispatchPlatformIdentityAddress(customer.MobilePhone));
				}
			}

			if (loyalty != null)
			{
				profile.Attributes["customerLoyaltyTier"] = loyalty.Tier;
				profile.Attributes["customerLoyaltyPoints"] = loyalty.Points.ToString();
			}

			return Task.CompletedTask;
		}
	}

	// This enricher demonstrates gathering domain-specific data that belongs in content generation.
	// It combines Order + Fulfillment read models and projects a communication-specific content shape.
	public sealed class HydrateOrderAndFulfillmentContentModelEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			var orderId = EnricherModelMapper.GetString(currentModel.Model, "orderId");
			if (string.IsNullOrWhiteSpace(orderId))
				return Task.FromResult<IContentModel?>(currentModel);

			var order = SampleOrderReadModelService.GetById(orderId);
			var shipment = SampleFulfillmentReadModelService.GetByOrderId(orderId);
			var profile = context.PlatformIdentities.FirstOrDefault() as DispatchPlatformIdentityProfile;

			var replacementModel = EnricherModelMapper.ToMutableDictionary(currentModel.Model);
			replacementModel["customerId"] = profile?.Id ?? "unknown";
			replacementModel["customerType"] = profile?.Type ?? "unknown";
			replacementModel["customerDisplayName"] = EnricherModelMapper.GetAttribute(profile, "customerDisplayName") ?? "Customer";
			replacementModel["customerRegion"] = EnricherModelMapper.GetAttribute(profile, "customerRegion") ?? "unknown";
			replacementModel["customerLoyaltyTier"] = EnricherModelMapper.GetAttribute(profile, "customerLoyaltyTier") ?? "standard";

			if (order != null)
			{
				replacementModel["orderNumber"] = order.OrderNumber;
				replacementModel["orderTotal"] = order.Total;
				replacementModel["orderCurrency"] = order.Currency;
				replacementModel["orderStatus"] = order.Status;
			}

			if (shipment != null)
			{
				replacementModel["shipmentCarrier"] = shipment.Carrier;
				replacementModel["shipmentTrackingNumber"] = shipment.TrackingNumber;
				replacementModel["shipmentTrackingUrl"] = shipment.TrackingUrl;
				replacementModel["shipmentEstimatedDeliveryUtc"] = shipment.EstimatedDeliveryUtc.ToString("u");
			}

			return Task.FromResult<IContentModel?>(EnricherModelMapper.CreateReplacement(currentModel, replacementModel));
		}
	}

	// This enricher demonstrates channel-specific tailoring.
	// Push payloads are often size constrained, so we short-circuit to prevent later enrichers
	// from adding extra content for that channel.
	public sealed class PushChannelShortCircuitContentModelEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			var replacementModel = EnricherModelMapper.ToMutableDictionary(currentModel.Model);
			replacementModel["deliveryChannel"] = context.ChannelId ?? "unknown";
			replacementModel["deliveryProvider"] = context.ChannelProviderId ?? "unknown";
			replacementModel["channelMessagePrefix"] = "Order update:";
			replacementModel["channelNote"] = "Push notification content trimmed for brevity.";

			return Task.FromResult<IContentModel?>(EnricherModelMapper.CreateReplacement(currentModel, replacementModel));
		}
	}

	// This enricher runs for non-push channels and adds richer delivery context.
	public sealed class AddChannelDeliveryContextContentModelEnricher : IContentModelEnricher
	{
		public Task<IContentModel?> EnrichAsync(IDispatchCommunicationContext context, IContentModel currentModel, CancellationToken cancellationToken = default)
		{
			var replacementModel = EnricherModelMapper.ToMutableDictionary(currentModel.Model);
			replacementModel["deliveryChannel"] = context.ChannelId ?? "unknown";
			replacementModel["deliveryProvider"] = context.ChannelProviderId ?? "unknown";
			replacementModel["channelMessagePrefix"] = "Detailed order update:";
			replacementModel["channelNote"] = "Email/SMS can include additional domain context and links.";

			return Task.FromResult<IContentModel?>(EnricherModelMapper.CreateReplacement(currentModel, replacementModel));
		}
	}

	#region Sample Services
	// These in-memory services mimic what a DDD application might expose as read models.
	// They make the enricher examples concrete without introducing infrastructure dependencies.

	internal sealed record CustomerDirectoryRecord(string CustomerId, string DisplayName, string PreferredCulture, string Region, string PrimaryEmail, string MobilePhone);

	internal static class SampleCustomerDirectoryService
	{
		private static readonly Dictionary<string, CustomerDirectoryRecord> _customers = new(StringComparer.OrdinalIgnoreCase)
		{
			["customer-1001"] = new("customer-1001", "Alex Johnson", "en-US", "US-WEST", "alex.johnson@example.com", "+12065550111"),
			["customer-1002"] = new("customer-1002", "Priya Patel", "en-US", "US-EAST", "priya.patel@example.com", "+12065550112")
		};

		public static CustomerDirectoryRecord? GetById(string customerId)
		{
			return _customers.TryGetValue(customerId, out var value) ? value : null;
		}
	}

	internal sealed record LoyaltySnapshot(string CustomerId, string Tier, int Points);

	internal static class SampleLoyaltyReadModelService
	{
		private static readonly Dictionary<string, LoyaltySnapshot> _tiers = new(StringComparer.OrdinalIgnoreCase)
		{
			["customer-1001"] = new("customer-1001", "Gold", 12440),
			["customer-1002"] = new("customer-1002", "Silver", 4280)
		};

		public static LoyaltySnapshot? GetByCustomerId(string customerId)
		{
			return _tiers.TryGetValue(customerId, out var value) ? value : null;
		}
	}

	internal sealed record OrderReadModel(string OrderId, string OrderNumber, decimal Total, string Currency, string Status);

	internal static class SampleOrderReadModelService
	{
		private static readonly Dictionary<string, OrderReadModel> _orders = new(StringComparer.OrdinalIgnoreCase)
		{
			["ord-90001"] = new("ord-90001", "SO-90001", 129.49m, "USD", "Packed"),
			["ord-90002"] = new("ord-90002", "SO-90002", 89.95m, "USD", "ReadyToShip")
		};

		public static OrderReadModel? GetById(string orderId)
		{
			return _orders.TryGetValue(orderId, out var value) ? value : null;
		}
	}

	internal sealed record FulfillmentReadModel(string OrderId, string Carrier, string TrackingNumber, string TrackingUrl, DateTime EstimatedDeliveryUtc);

	internal static class SampleFulfillmentReadModelService
	{
		private static readonly Dictionary<string, FulfillmentReadModel> _shipments = new(StringComparer.OrdinalIgnoreCase)
		{
			["ord-90001"] = new("ord-90001", "UPS", "1Z999AA10123456784", "https://tracking.example.com/1Z999AA10123456784", DateTime.UtcNow.AddDays(2)),
			["ord-90002"] = new("ord-90002", "FedEx", "782635118264", "https://tracking.example.com/782635118264", DateTime.UtcNow.AddDays(1))
		};

		public static FulfillmentReadModel? GetByOrderId(string orderId)
		{
			return _shipments.TryGetValue(orderId, out var value) ? value : null;
		}
	}
	#endregion

	internal static class EnricherModelMapper
	{
		public static string? GetAttribute(DispatchPlatformIdentityProfile? profile, string attributeName)
		{
			if (profile?.Attributes == null)
				return null;

			return profile.Attributes.TryGetValue(attributeName, out var value) ? value : null;
		}

		public static string? GetString(object sourceModel, string key)
		{
			if (sourceModel is IDictionary<string, object?> generic && generic.TryGetValue(key, out var genericValue))
				return genericValue?.ToString();

			if (sourceModel is IDictionary legacy && legacy.Contains(key))
				return legacy[key]?.ToString();

			var property = sourceModel.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
			return property?.GetValue(sourceModel)?.ToString();
		}

		public static IDictionary<string, object?> ToMutableDictionary(object sourceModel)
		{
			if (sourceModel is IDictionary<string, object?> dictionary)
				return new Dictionary<string, object?>(dictionary, StringComparer.OrdinalIgnoreCase);

			if (sourceModel is IDictionary legacyDictionary)
			{
				var converted = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
				foreach (DictionaryEntry item in legacyDictionary)
				{
					if (item.Key is null)
						continue;

					converted[item.Key.ToString() ?? string.Empty] = item.Value;
				}

				return converted;
			}

			var reflected = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
			foreach (var property in sourceModel.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.GetIndexParameters().Length == 0))
			{
				reflected[property.Name] = property.GetValue(sourceModel);
			}

			return reflected;
		}

		public static IContentModel CreateReplacement(IContentModel currentModel, IDictionary<string, object?> replacementModel)
		{
			return new ReplacementContentModel(replacementModel, currentModel.Resources, currentModel.LinkedResources);
		}
	}

	internal sealed class ReplacementContentModel(
		object model,
		IReadOnlyList<Resource> resources,
		IReadOnlyList<LinkedResource> linkedResources) : IContentModel
	{
		public object Model { get; } = model;
		public IReadOnlyList<Resource> Resources { get; } = resources;
		public IReadOnlyList<LinkedResource> LinkedResources { get; } = linkedResources;
	}
}
