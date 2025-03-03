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
using Transmitly.Channel.Configuration;
using Transmitly.Delivery;
using Transmitly.Persona.Configuration;
using Transmitly.PlatformIdentity.Configuration;
using Transmitly;

namespace Tandely.Notifications.Client
{
	public sealed class NotificationsCommunicationsClient : ICommunicationsClient
	{
		readonly static Lazy<HttpClient> _httpClient = new(() => CreateHttpClient(_options));
		static NotificationsOptions? _options;
		readonly DefaultCommunicationsClient _defaultClient;
		readonly IReadOnlyCollection<IPersonaRegistration>? _pipelinePersonas;
		readonly static object _lock = new();
		internal NotificationsCommunicationsClient(DefaultCommunicationsClient defaultClient, ICreateCommunicationsClientContext context, IPlatformIdentityResolverFactory platformIdentityResolverFactory, NotificationsOptions options)
		{
			_pipelinePersonas = Guard.AgainstNull(context.Personas);
			_defaultClient = Guard.AgainstNull(defaultClient);
			_options = Guard.AgainstNull(options);
		}

		private static HttpClient CreateHttpClient(NotificationsOptions? options)
		{
			Guard.AgainstNull(options);

			var client = new HttpClient { BaseAddress = options.BasePath };
			client.DefaultRequestHeaders.Add("x-tandely-api-key", options.ApiKey);
			return client;
		}

		public void DeliverReport(DeliveryReport report)
		{
			_defaultClient.DeliverReport(report);
		}

		public void DeliverReports(IReadOnlyCollection<DeliveryReport> reports)
		{
			_defaultClient.DeliverReports(reports);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IPlatformIdentity> platformIdentities, ITransactionModel transactionalModel, IReadOnlyCollection<string> allowedChannels, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			try
			{
				var personFilters = _pipelinePersonas?.Select(x => x.Name);
				var dispatchCorrelationId = Guid.NewGuid().ToString("N");

				var payload = JsonSerializer.Serialize(new DispatchNotificationModel
				{
					AllowedChannels = allowedChannels,
					CommunicationId = pipelineName,
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

				var apiResult = JsonSerializer.Deserialize<DispatchNotificationResult?>(await apiCallResult.Content.ReadAsStringAsync(cancellationToken));

				if (apiCallResult.IsSuccessStatusCode)
				{
					return new DispatchCommunicationResult([new NotificationsDispatchResult
						{
							DispatchStatus = DispatchStatus.Dispatched,
							ResourceId = dispatchCorrelationId,
							ChannelId = "Tandely.Notifications",
							Exception = null,
							ChannelProviderId = "Tandely.Notifications"
						}], true);
				}
				else
				{
					return new DispatchCommunicationResult([new NotificationsDispatchResult
						{
							DispatchStatus = DispatchStatus.Undeliverable,
							ResourceId = dispatchCorrelationId,
							ChannelId = "Tandely.Notifications",
							Exception = new Exception(string.Join(", ", apiResult?.Errors.Select(x => x.ErrorMessage)??[])),
							ChannelProviderId = "Tandely.Notifications"
						}], false);
				}
			}
			catch (Exception ex)
			{
				return new DispatchCommunicationResult([new NotificationsDispatchResult
					{
						DispatchStatus = DispatchStatus.Exception,
						ResourceId = null,
						ChannelId = "Tandely.Notifications",
						Exception = new Exception("An exception occurred while attempting to dispatch communications to the Tandely Notifications services",  ex),
						ChannelProviderId = "Tandely.Notifications"
					}], false);
			}
		}

		private IPersonaRegistration[] getIdentityPersonas(IPlatformIdentity pid)
		{
			var result = _pipelinePersonas?.Where(x => pid.GetType().IsAssignableFrom(x.PersonaType) && x.IsMatch(pid)).ToArray();
			return result ?? [];
		}

		public Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IIdentityAddress> identityAddresses, ITransactionModel transactionalModel, IReadOnlyCollection<string> allowedChannels, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return DispatchAsync(pipelineName, [new PlatformIdentityRecord(null, null, identityAddresses)], transactionalModel, allowedChannels, null, cancellationToken);
		}

		public Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, string identityAddress, ITransactionModel transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return DispatchAsync(pipelineName, [new PlatformIdentityRecord(null, null, [identityAddress.AsIdentityAddress()])], transactionalModel, [], cultureInfo, cancellationToken);
		}

		public Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, string identityAddress, object transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return DispatchAsync(pipelineName, [new PlatformIdentityRecord(null, null, [identityAddress.AsIdentityAddress()])], TransactionModel.Create(transactionalModel), [], cultureInfo, cancellationToken);
		}

		public Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<string> identityAddresses, object transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return DispatchAsync(pipelineName, [new PlatformIdentityRecord(null, null, identityAddresses.Select(m => m.AsIdentityAddress()))], TransactionModel.Create(transactionalModel), [], cultureInfo, cancellationToken);
		}

		public Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IIdentityAddress> identityAddresses, ITransactionModel transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return DispatchAsync(pipelineName, [new PlatformIdentityRecord(null, null, identityAddresses)], transactionalModel, [], cultureInfo, cancellationToken);
		}

		public Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IPlatformIdentity> platformIdentities, ITransactionModel transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return DispatchAsync(pipelineName, platformIdentities, transactionalModel, [], cultureInfo, cancellationToken);
		}

		public Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IIdentityReference> identityReferences, ITransactionModel transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return DispatchAsync(pipelineName, identityReferences, transactionalModel, [], cultureInfo, cancellationToken);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IIdentityReference> identityReferences, ITransactionModel transactionalModel, IReadOnlyCollection<string> allowedCHannels, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return await DispatchAsync(pipelineName, identityReferences.Select(x => new PlatformIdentityRecord(x.Id, x.Type, [])).ToList(), transactionalModel, cultureInfo, cancellationToken).ConfigureAwait(false);
		}
	}
}