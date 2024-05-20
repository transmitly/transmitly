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
using Transmitly.Channel.Configuration;
using Transmitly.ChannelProvider;
using Transmitly.ChannelProvider.Configuration;
using Transmitly.Delivery;
using Transmitly.Exceptions;
using Transmitly.Pipeline.Configuration;
using Transmitly.Template.Configuration;

namespace Transmitly
{
	public sealed class DefaultCommunicationsClient(
		IPipelineFactory pipelineRegistrations,
		IChannelProviderFactory channelProviderRegistrations,
		ITemplateEngineFactory templateEngineRegistrations,
		IDeliveryReportReporter deliveryReportHandler
		) :
		ICommunicationsClient
	{
		private readonly IPipelineFactory _pipelineRegistrations = Guard.AgainstNull(pipelineRegistrations);
		private readonly IChannelProviderFactory _channelProviderRegistrations = Guard.AgainstNull(channelProviderRegistrations);
		private readonly ITemplateEngineFactory _templateEngineRegistrations = Guard.AgainstNull(templateEngineRegistrations);
		private readonly IDeliveryReportReporter _deliveryReportProvider = Guard.AgainstNull(deliveryReportHandler);

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IIdentityAddress> identityAddresses, IContentModel contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			Guard.AgainstNull(identityAddresses);
			var platformIdentities = new PlatformIdentityRecord[] {
				new(identityAddresses) {
					Type = "Unknown:" + pipelineName,
					Id = string.Join(";", identityAddresses.Select(x => x.Value))
				}
			};
			return await DispatchAsync(pipelineName, platformIdentities, contentModel, cultureInfo, cancellationToken).ConfigureAwait(false);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, string identityAddress, object contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			Guard.AgainstNullOrWhiteSpace(identityAddress);

			return await DispatchAsync(pipelineName, [identityAddress], contentModel, cultureInfo, cancellationToken).ConfigureAwait(false);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<string> identityAddresses, object contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			Guard.AgainstNull(identityAddresses);

			return await DispatchAsync(pipelineName, identityAddresses.Select(x => (IIdentityAddress)new IdentityAddress(x)).ToList(), ContentModel.Create(contentModel), cultureInfo, cancellationToken).ConfigureAwait(false);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, string identityAddress, IContentModel contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{

			return await DispatchAsync(pipelineName, new IdentityAddress[] { new(identityAddress) }, contentModel, cultureInfo, cancellationToken).ConfigureAwait(false);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IPlatformIdentity> platformIdentities, IContentModel contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return await DispatchAsync(pipelineName, platformIdentities, contentModel, [], cultureInfo, cancellationToken).ConfigureAwait(false);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IIdentityAddress> identityAddresses, IContentModel contentModel, IReadOnlyCollection<string> allowedChannels, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return await DispatchAsync(pipelineName, [identityAddresses.AsPlatformIdentity()], contentModel, allowedChannels, cultureInfo, cancellationToken).ConfigureAwait(false);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IPlatformIdentity> platformIdentities, IContentModel contentModel, IReadOnlyCollection<string> allowedChannels, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			Guard.AgainstNullOrWhiteSpace(pipelineName);
			Guard.AgainstNull(contentModel);
			Guard.AgainstNull(platformIdentities);
			var culture = GuardCulture.AgainstNull(cultureInfo);

			var pipeline = await _pipelineRegistrations.GetAsync(pipelineName).ConfigureAwait(false) ??
				throw new CommunicationsException($"A communication pipeline named, '{pipelineName}', has not been registered.");

			var pipelineConfiguration = pipeline.ChannelConfiguration;

			var deliveryStrategy = pipeline.ChannelConfiguration.PipelineDeliveryStrategyProvider;

			var context = new DispatchCommunicationContext(
				contentModel,
				pipelineConfiguration,
				platformIdentities,
				_templateEngineRegistrations.Get(),
				_deliveryReportProvider,
				culture,
				pipelineName,
				pipeline.MessagePriority,
				pipeline.TransportPriority);


			var channelProviders = await _channelProviderRegistrations.GetAllAsync().ConfigureAwait(false);
			var groups = await CreateChannelChannelProviderGroupsAsync(_channelProviderRegistrations, pipelineConfiguration.Channels, channelProviders, allowedChannels, platformIdentities);
			if (groups.Count == 0)
				return new DispatchCommunicationResult([], true);

			var result = await deliveryStrategy.DispatchAsync(groups, context, cancellationToken).ConfigureAwait(false);

			return result;
		}

		private static async Task<IReadOnlyCollection<ChannelChannelProviderGroup>> CreateChannelChannelProviderGroupsAsync(
			IChannelProviderFactory channelProviderFactory,
			IReadOnlyCollection<IChannel> configuredChannels,
			IReadOnlyCollection<IChannelProviderRegistration> allowedChannelProviders,
			IReadOnlyCollection<string> allowedChannels,
			IReadOnlyCollection<IPlatformIdentity> platformIdentities)
		{

			return (await Task.WhenAll(configuredChannels.Select(channel =>
			{
				var identityAddresses = platformIdentities.SelectMany(m => m.Addresses);
				bool filterClientRegistrations(IChannelProviderClientRegistration clientRegistration)
				{
					return clientRegistration
						.SupportsChannel(channel.Id) &&
						(
							clientRegistration.CommunicationType == typeof(object) ||
							channel.CommunicationType == clientRegistration.CommunicationType
						) &&
						identityAddresses.Any(a => channel.SupportsIdentityAddress(a));
				}

				var channelProviders = allowedChannelProviders.Where(x =>
							(
								allowedChannels.Count == 0 ||
								allowedChannels.Any(a => channel.Id == a)
							) &&
							(
								!channel.AllowedChannelProviderIds.Any() ||
								channel.AllowedChannelProviderIds.Contains(x.Id)
							) //&&
							  //x.ClientRegistrations.Any(filterClients)
						).SelectMany(providerRegistration =>
						{
							return providerRegistration.ClientRegistrations
							.Where(filterClientRegistrations)
							.Select(clientRegistration =>
								new ChannelProviderWrapper(
									providerRegistration.Id,
									clientRegistration,
									async () => await channelProviderFactory.ResolveClientAsync(providerRegistration, clientRegistration)
								)
							);
						})
						.ToList();

				return Task.FromResult(new ChannelChannelProviderGroup(
						channel,
						channelProviders
					));

			})))
			.Where(x => x.ChannelProviderClients.Count != 0)
			.ToList();
		}

		public void DeliverReport(DeliveryReport report)
		{
			Guard.AgainstNull(report);
			_deliveryReportProvider.DispatchReport(report);
		}

		public void DeliverReports(IReadOnlyCollection<DeliveryReport> reports)
		{
			Guard.AgainstNull(reports);
			foreach (var report in reports)
				_deliveryReportProvider.DispatchReport(report);
		}
	}
}