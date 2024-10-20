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
using Transmitly.Persona.Configuration;
using Transmitly.Pipeline.Configuration;
using Transmitly.PlatformIdentity.Configuration;
using Transmitly.Template.Configuration;

namespace Transmitly
{
    public sealed class DefaultCommunicationsClient(
        IPipelineFactory pipelineRegistrations,
        IChannelProviderFactory channelProviderRegistrations,
        ITemplateEngineFactory templateEngineRegistrations,
        IPersonaFactory personaRegistrations,
        IPlatformIdentityResolverFactory platformIdentityResolverRegistrations,
        IDeliveryReportReporter deliveryReportHandler
        ) :
        ICommunicationsClient
    {
        private readonly IPipelineFactory _pipelineRegistrations = Guard.AgainstNull(pipelineRegistrations);
        private readonly IChannelProviderFactory _channelProviderRegistrations = Guard.AgainstNull(channelProviderRegistrations);
        private readonly ITemplateEngineFactory _templateEngineRegistrations = Guard.AgainstNull(templateEngineRegistrations);
        private readonly IPersonaFactory _personaRegistrations = Guard.AgainstNull(personaRegistrations);
        private readonly IDeliveryReportReporter _deliveryReportProvider = Guard.AgainstNull(deliveryReportHandler);
        private readonly IPlatformIdentityResolverFactory _platformIdentityResolvers = Guard.AgainstNull(platformIdentityResolverRegistrations);

        public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IIdentityAddress> identityAddresses, ITransactionModel transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(identityAddresses);
            var platformIdentities = new PlatformIdentityRecord[] {
                new(null,null,identityAddresses)
            };
            return await DispatchAsync(pipelineName, platformIdentities, transactionalModel, cultureInfo, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, string identityAddress, object transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNullOrWhiteSpace(identityAddress);

            return await DispatchAsync(pipelineName, [identityAddress], transactionalModel, cultureInfo, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<string> identityAddresses, object transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(identityAddresses);

            return await DispatchAsync(pipelineName, identityAddresses.Select(x => (IIdentityAddress)new IdentityAddress(x)).ToList(), TransactionModel.Create(transactionalModel), cultureInfo, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, string identityAddress, ITransactionModel transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
        {

            return await DispatchAsync(pipelineName, new IdentityAddress[] { new(identityAddress) }, transactionalModel, cultureInfo, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IPlatformIdentity> platformIdentities, ITransactionModel transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
        {
            return await DispatchAsync(pipelineName, platformIdentities, transactionalModel, [], cultureInfo, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IIdentityAddress> identityAddresses, ITransactionModel transactionalModel, IReadOnlyCollection<string> allowedChannels, string? cultureInfo = null, CancellationToken cancellationToken = default)
        {
            return await DispatchAsync(pipelineName, [identityAddresses.AsPlatformIdentity()], transactionalModel, allowedChannels, cultureInfo, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IPlatformIdentity> platformIdentities, ITransactionModel transactionalModel, IReadOnlyCollection<string> allowedChannels, string? cultureInfo = null, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNullOrWhiteSpace(pipelineName);
            Guard.AgainstNull(transactionalModel);
            Guard.AgainstNull(platformIdentities);

            var culture = GuardCulture.AgainstNull(cultureInfo);

            var pipeline = await _pipelineRegistrations.GetAsync(pipelineName).ConfigureAwait(false) ??
                throw new CommunicationsException($"A communication pipeline named, '{pipelineName}', has not been registered.");

            var pipelineConfiguration = pipeline.ChannelConfiguration;

            if (pipelineConfiguration.PersonaFilters.Count != 0 && !await pipelineConfiguration.PersonaFilters.AllAsync(pf => _personaRegistrations.AnyMatch(pf, platformIdentities)))
            {
                var expected = string.Join(", ", pipelineConfiguration.PersonaFilters);
                throw new CommunicationsException($"One or more pipeline persona filter criteria were not met. Expected={expected}");
            }

            var deliveryStrategy = pipeline.ChannelConfiguration.PipelineDeliveryStrategyProvider;
            var contentModel = new ContentModel(transactionalModel, platformIdentities);
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

        public Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IIdentityReference> identityReferences, ITransactionModel transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
        {
            return DispatchAsync(pipelineName, identityReferences, transactionalModel, [], cultureInfo, cancellationToken);
        }

        public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IIdentityReference> identityReferences, ITransactionModel transactionalModel, IReadOnlyCollection<string> allowedCHannels, string? cultureInfo = null, CancellationToken cancellationToken = default)
        {
            var uniqueTypes = Guard.AgainstNullOrEmpty(identityReferences?.ToList()).Select(s => s.Type).Distinct().ToArray();

            var resolvers = await _platformIdentityResolvers.GetAsync(uniqueTypes).ConfigureAwait(false);

            List<IPlatformIdentity> results = [];

            foreach (var resolver in resolvers)
            {
                var resolverInstance = await _platformIdentityResolvers.ResolveResolver(resolver).ConfigureAwait(false)
                    ?? throw new CommunicationsException("Unable to get an instance of platform identity resolver");

                var filteredByTypeReferences = identityReferences
                    .Where(x =>
                        string.IsNullOrEmpty(resolver.PlatformIdentityType) ||
                        string.Equals(resolver.PlatformIdentityType, x.Type, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList()
                    .AsReadOnly();

                var resolvedIdentities = await resolverInstance.Resolve(filteredByTypeReferences).ConfigureAwait(false);

                if (resolvedIdentities != null)
                    results.AddRange(resolvedIdentities);
            }

            return await DispatchAsync(pipelineName, results, transactionalModel, allowedCHannels, cultureInfo, cancellationToken).ConfigureAwait(false);
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
                                    async () => await channelProviderFactory.ResolveClientAsync(providerRegistration, clientRegistration).ConfigureAwait(false)
                                )
                            );
                        })
                        .ToList();

                return Task.FromResult(new ChannelChannelProviderGroup(
                        channel,
                        channelProviders
                    ));

            })).ConfigureAwait(false))
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