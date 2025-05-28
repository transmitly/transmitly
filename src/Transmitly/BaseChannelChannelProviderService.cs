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

namespace Transmitly;

public abstract class BaseChannelChannelProviderService(IChannelProviderFactory channelProviderFactory) : IChannelChannelProviderService
{
	private readonly IChannelProviderFactory _channelProviderFactory = Guard.AgainstNull(channelProviderFactory);

	public async Task<IReadOnlyCollection<ChannelChannelProviderGroup>> CreateGroupingsForPlatformIdentityAsync(string? pipelineCategory, IReadOnlyCollection<IChannel> pipelineChannels, IReadOnlyCollection<string> dispatchChannelPreferences, IPlatformIdentityProfile platformIdentity)
	{
		var groups = new List<ChannelChannelProviderGroup>();
		var activeChannelPreferences = GetActiveChannelPreferences(pipelineCategory, platformIdentity);

		foreach (var channel in pipelineChannels)
		{
			var providerWrappers = new List<ChannelProviderWrapper>();

			// Filter allowed channel providers based on dispatch preferences and channel restrictions
			foreach (var channelProvider in await _channelProviderFactory.GetAllAsync().ConfigureAwait(false))
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
}