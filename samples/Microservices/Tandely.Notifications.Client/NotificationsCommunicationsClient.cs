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

using System.Text.Json;
using Transmitly;
using Transmitly.Channel.Configuration;
using Transmitly.Delivery;
using Transmitly.Persona.Configuration;
using Transmitly.PlatformIdentity.Configuration;
using Transmitly.Util;

namespace Tandely.Notifications.Client
{
	public sealed class NotificationsCommunicationsClient : ICommunicationsClient
	{
		readonly JsonSerializerOptions _jsonOptions;
		readonly static Lazy<HttpClient> _httpClient = new(() => CreateHttpClient(_options));
		static NotificationsOptions? _options;
		readonly DefaultCommunicationsClient _defaultClient;
		readonly IReadOnlyCollection<IPersonaRegistration>? _pipelinePersonas;

		internal NotificationsCommunicationsClient(DefaultCommunicationsClient defaultClient, ICreateCommunicationsClientContext context,
			IPlatformIdentityResolverFactory platformIdentityResolverFactory, NotificationsOptions options,
			JsonSerializerOptions jsonOptions
		)
		{
			_pipelinePersonas = Guard.AgainstNull(context.Personas);
			_defaultClient = Guard.AgainstNull(defaultClient);
			_options = Guard.AgainstNull(options);
			_jsonOptions = Guard.AgainstNull(jsonOptions);
		}

		private static HttpClient CreateHttpClient(NotificationsOptions? options)
		{
			Guard.AgainstNull(options);

			var client = new HttpClient { BaseAddress = options.BasePath };
			client.DefaultRequestHeaders.Add("x-tandely-api-key", options.ApiKey);
			return client;
		}

		private IPersonaRegistration[] getIdentityPersonas(IPlatformIdentityProfile pid)
		{
			var result = _pipelinePersonas?.Where(x => pid.GetType().IsAssignableFrom(x.PersonaType) && x.IsMatch(pid)).ToArray();
			return result ?? [];
		}



		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineIntent, IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities, ITransactionModel transactionalModel, IReadOnlyCollection<string> dispatchChannelPreferences, string? pipelineId = null, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			try
			{
				var personFilters = _pipelinePersonas?.Select(x => x.Name);
				var dispatchCorrelationId = Guid.NewGuid().ToString("N");

				var payload = JsonSerializer.Serialize(new DispatchNotificationModel
				{
					AllowedChannels = dispatchChannelPreferences,
					CommunicationIntent = pipelineIntent,
					CommunicationId = pipelineId,
					PersonFilters = personFilters ?? [],
					ExternalInstanceReferenceId = dispatchCorrelationId,
					TransactionalModel = new NotificationsTransactionalModel(transactionalModel),

					PlatformIdentities = [.. platformIdentities.Select(pid => new NotificationsPlatformIdentity
					{
						Id = pid.Id,
						Type = pid.Type,
						Personas = [.. getIdentityPersonas(pid).Select(x => x.Name)],
						Addresses = [.. pid.Addresses.Select(a => new NotificationsIdentityAddress
						{
							Type = a.Type,
							Value = a.Value
						})]
					})]
				});

				var apiCallResult = await _httpClient.Value.PostAsync($"notifications/dispatch", new StringContent(payload, System.Text.Encoding.UTF8, "application/json"), cancellationToken);

				var result = await apiCallResult.Content.ReadAsStringAsync(cancellationToken);
				var apiResult = JsonSerializer.Deserialize<DispatchNotificationResult?>(result, _jsonOptions);

				if (apiCallResult.IsSuccessStatusCode)
				{
					return new DispatchCommunicationResult([new NotificationsDispatchResult
						{
							Status = CommunicationsStatus.Success("Tandely", "Dispatched"),
							ResourceId = dispatchCorrelationId,
							ChannelId = "Tandely.Notifications",
							Exception = null,
							ChannelProviderId = "Tandely.Notifications"
						}]);
				}
				else
				{
					return new DispatchCommunicationResult([new NotificationsDispatchResult
						{
							Status = CommunicationsStatus.ServerError("Tandely", "Undeliverable"),
							ResourceId = dispatchCorrelationId,
							ChannelId = "Tandely.Notifications",
							Exception = new Exception(string.Join(", ", apiResult?.Results.Select(x => x.Exception?.ToString())??[])),
							ChannelProviderId = "Tandely.Notifications"
						}]);
				}
			}
			catch (Exception ex)
			{
				return new DispatchCommunicationResult([new NotificationsDispatchResult
					{
						Status = CommunicationsStatus.ClientError("Tandely", "Exception"),
						ResourceId = null,
						ChannelId = "Tandely.Notifications",
						Exception = new Exception("An exception occurred while attempting to dispatch communications to the Tandely Notifications services", ex),
						ChannelProviderId = "Tandely.Notifications"
					}]);
			}
		}

		public Task<IDispatchCommunicationResult> DispatchAsync(string pipelineIntent, IReadOnlyCollection<IPlatformIdentityReference> identityReferences, ITransactionModel transactionalModel, IReadOnlyCollection<string> dispatchChannelPreferences, string? pipelineId = null, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return DispatchAsync(pipelineIntent, identityReferences.Select(x => new PlatformIdentityProfile(x.Id, x.Type, [])).ToList(), transactionalModel, dispatchChannelPreferences, pipelineId, cultureInfo, cancellationToken);
		}

		public Task DispatchAsync(DeliveryReport report)
		{
			return _defaultClient.DispatchAsync(report);
		}

		public Task DispatchAsync(IReadOnlyCollection<DeliveryReport> reports)
		{
			return _defaultClient.DispatchAsync(reports);
		}
	}
}