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

using System.Collections.ObjectModel;
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

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities, ITransactionModel transactionalModel, IReadOnlyCollection<string> dispatchChannelPreferences, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			Guard.AgainstNullOrWhiteSpace(pipelineName);
			Guard.AgainstNull(transactionalModel);
			Guard.AgainstNull(platformIdentities);

			var culture = GuardCulture.AgainstNull(cultureInfo);

			var matchingPipelines = await _pipelineRegistrations.GetAsync(pipelineName).ConfigureAwait(false);
			if (matchingPipelines.Count == 0)
				return new DispatchCommunicationResult([new DispatchResult(PredefinedDispatchStatuses.PipelineNotFound)]);
			//throw new CommunicationsException($"A communication pipeline named, '{pipelineName}', has not been registered.");

			var allResults = new List<IDispatchCommunicationResult>();
			var allRegisteredChannelProviders = await _channelProviderRegistrations.GetAllAsync().ConfigureAwait(false);
			var templateEngine = _templateEngineRegistrations.Get();

			foreach (var pipeline in matchingPipelines)
			{
				var pipelineConfiguration = pipeline.Configuration;
				var filteredPlatformIdentities = await FilterPlatformIdentityPersonas(platformIdentities, pipelineConfiguration).ConfigureAwait(false);
				var deliveryStrategy = pipeline.Configuration.PipelineDeliveryStrategyProvider;

				var dispatchList = filteredPlatformIdentities.Select(identity =>
				{
					var context = new InternalDispatchCommunicationContext(
						transactionalModel,
						pipelineConfiguration,
						[identity],
						templateEngine,
						_deliveryReportProvider,
						culture,
						pipelineName);

					var group = CreateChannelChannelProviderGroupsForPlatformIdentity(
						pipeline.Category,
						_channelProviderRegistrations,
						pipelineConfiguration.Channels,
						allRegisteredChannelProviders,
						dispatchChannelPreferences,
						identity
					);

					if (group.Count == 0)
						return null;

					return new RecipientDispatchCommunicationContext(context, group);
				})
				.Where(result => result != null)
				.Cast<RecipientDispatchCommunicationContext>()
				.ToList().AsReadOnly();

				var dispatchResult = await deliveryStrategy.DispatchAsync(dispatchList, cancellationToken).ConfigureAwait(false);
				allResults.Add(dispatchResult);
			}

			var allDispatchResults = allResults.SelectMany(r => r.Results).ToList().AsReadOnly();
			//var allDispatchSuccessful = allResults.All(r => r.IsSuccessful);

			return new DispatchCommunicationResult(allDispatchResults);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IPlatformIdentityReference> identityReferences, ITransactionModel transactionalModel, IReadOnlyCollection<string> channelPreferences, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			var uniqueTypes = Guard.AgainstNullOrEmpty(identityReferences?.ToList()).Select(s => s.Type).Distinct().ToArray();

			var resolvers = await _platformIdentityResolvers.GetAsync(uniqueTypes).ConfigureAwait(false);

			List<IPlatformIdentityProfile> results = [];

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

			return await DispatchAsync(pipelineName, results, transactionalModel, channelPreferences, cultureInfo, cancellationToken).ConfigureAwait(false);
		}

		private static IEnumerable<ChannelChannelProviderGroup> OrderProvidersByPlatformIdentityPreference(IEnumerable<ChannelChannelProviderGroup> groups, IReadOnlyCollection<string>? preferences)
		{
			if ((preferences?.Count ?? 0) == 0)
				return groups;

			var channelPreferences = preferences!.ToList();
			return groups.OrderBy(g =>
			{
				var index = channelPreferences.IndexOf(g.Channel.Id);
				return index >= 0 ? index : int.MaxValue;
			});
		}

		private async Task<IReadOnlyCollection<IPlatformIdentityProfile>> FilterPlatformIdentityPersonas(IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities, IPipelineConfiguration pipelineConfiguration)
		{
			if (pipelineConfiguration.PersonaFilters.Count == 0)
			{
				return platformIdentities;
			}

			var tasks = platformIdentities.Select(async identity =>
			{
				foreach (var personaFilter in pipelineConfiguration.PersonaFilters)
				{
					if (await _personaRegistrations.AnyMatch(personaFilter, new[] { identity }).ConfigureAwait(false))
					{
						return identity;
					}
				}
				return null;
			});

			var results = await Task.WhenAll(tasks).ConfigureAwait(false);
			platformIdentities = new List<IPlatformIdentityProfile>(results.Where(identity => identity != null).Cast<IPlatformIdentityProfile>());
			return platformIdentities.ToList().AsReadOnly();
		}

		private static ReadOnlyCollection<ChannelChannelProviderGroup> CreateChannelChannelProviderGroupsForPlatformIdentity(
			string? pipelineCategory,
			IChannelProviderFactory channelProviderFactory,
			IReadOnlyCollection<IChannel> configuredChannels,
			IReadOnlyCollection<IChannelProviderRegistration> configuredChannelProviders,
			IReadOnlyCollection<string> dispatchChannelPreferences,
			IPlatformIdentityProfile platformIdentity)
		{
			var groups = new List<ChannelChannelProviderGroup>();
			var activeChannelPreferences = GetActiveChannelPreferences(pipelineCategory, platformIdentity);

			foreach (var channel in configuredChannels)
			{
				var providerWrappers = new List<ChannelProviderWrapper>();

				// Filter allowed channel providers based on dispatch preferences and channel restrictions
				foreach (var channelProvider in configuredChannelProviders)
				{
					if (!IsChannelProviderEligible(dispatchChannelPreferences, channel, channelProvider))
					{
						continue;
					}

					foreach (var dispatcher in channelProvider.DispatcherRegistrations)
					{
						if (!IsDispatcherEligible(platformIdentity, activeChannelPreferences, channel, dispatcher))
						{
							continue;
						}

						// Create the wrapper with a lazy resolver for the dispatcher.
						var wrapper = new ChannelProviderWrapper(
							channelProvider.Id,
							dispatcher,
							async () => await channelProviderFactory.ResolveDispatcherAsync(channelProvider, dispatcher).ConfigureAwait(false)
						);

						providerWrappers.Add(wrapper);
					}
				}

				if (providerWrappers.Count > 0)
				{
					groups.Add(new ChannelChannelProviderGroup(channel, providerWrappers.AsReadOnly()));
				}
			}

			// Order the groups according to channel preferences, if any
			if (activeChannelPreferences != null && activeChannelPreferences.Type == ChannelPreferenceType.Priority)
			{
				groups = [.. OrderProvidersByPlatformIdentityPreference(groups, activeChannelPreferences.Channels)];
			}

			return groups.AsReadOnly();
		}

		private static bool IsDispatcherEligible(IPlatformIdentityProfile platformIdentity, IChannelPreference? activeChannelPreferences, IChannel channel, IChannelProviderDispatcherRegistration dispatcher)
		{
			if (!dispatcher.SupportsChannel(channel.Id))
				return false;

			if (dispatcher.CommunicationType != typeof(object) && channel.CommunicationType != dispatcher.CommunicationType)
				return false;

			if (activeChannelPreferences != null && activeChannelPreferences.Channels.Count != 0 && activeChannelPreferences.Type == ChannelPreferenceType.Filter &&
				!activeChannelPreferences.Channels.Any(c => string.Equals(c, channel.Id, StringComparison.InvariantCulture)))
				return false;

			if (!platformIdentity.Addresses.Any(a => channel.SupportsIdentityAddress(a)))
				return false;

			return true;
		}

		private static bool IsChannelProviderEligible(IReadOnlyCollection<string> dispatchChannelPreferences, IChannel channel, IChannelProviderRegistration channelProvider)
		{
			// Check dispatch preferences: if preferences exist, the channel must be listed.
			if (dispatchChannelPreferences.Count > 0 &&
				!dispatchChannelPreferences.Any(preference => string.Equals(channel.Id, preference, StringComparison.InvariantCulture)))
			{
				return false;
			}

			// Check if channel has restrictions and if the provider is allowed
			if (channel.AllowedChannelProviderIds.Any() &&
				!channel.AllowedChannelProviderIds.Any(cpi => string.Equals(cpi, channelProvider.Id, StringComparison.InvariantCulture)))
			{
				return false;
			}

			return true;
		}

		private static IChannelPreference? GetActiveChannelPreferences(string? pipelineCategory, IPlatformIdentityProfile platformIdentity)
		{
			var activeChannelPreferences = platformIdentity.ChannelPreferences?
											.FirstOrDefault(a => string.Equals(a.Category, pipelineCategory, StringComparison.InvariantCulture));

			if (activeChannelPreferences == null && (platformIdentity.ChannelPreferences?.All(a => string.IsNullOrWhiteSpace(a.Category)) ?? false))
				activeChannelPreferences = platformIdentity.ChannelPreferences.FirstOrDefault();
			return activeChannelPreferences;
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