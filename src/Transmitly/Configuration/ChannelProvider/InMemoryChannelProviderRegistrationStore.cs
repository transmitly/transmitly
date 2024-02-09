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

using Transmitly.ChannelProvider;
using Transmitly.ChannelProvider.Configuration;

namespace Transmitly.Channel.Configuration
{
	/// <summary>
	/// Represents a registration store for in-memory channel providers.
	/// </summary>
	internal class InMemoryChannelProviderRegistrationStore : IChannelProviderRegistrationStore
	{
		private readonly List<IChannelProvider> _registrations;

		/// <summary>
		/// Creates a new instance of <see cref="InMemoryChannelProviderRegistrationStore"/>
		/// </summary>
		/// <param name="registrations">Enumerable of registered channel providers</param>
		/// <exception cref="ArgumentNullException">When the registrations is null</exception>
		internal InMemoryChannelProviderRegistrationStore(IEnumerable<IChannelProvider> registrations)
		{
			_registrations = registrations?.ToList() ?? throw new ArgumentNullException(nameof(registrations));
		}

		/// <summary>
		/// Retrieves all channel providers.
		/// </summary>
		/// <returns>A read-only list of channel providers.</returns>
		public Task<IReadOnlyList<IChannelProvider>> GetAllAsync()
		{
			return Task.FromResult<IReadOnlyList<IChannelProvider>>(_registrations);
		}

		/// <summary>
		/// Retrieves channel providers that support the specified channel providers.
		/// </summary>
		/// <param name="supportedChannelProviders">The array of supported channel providers.</param>
		/// <returns>A read-only list of channel providers.</returns>
		public Task<IReadOnlyList<IChannelProvider>> GetAllAsync(IReadOnlyCollection<string> supportedChannelProviders, IReadOnlyCollection<IChannel> channels)
		{
			return Task.FromResult<IReadOnlyList<IChannelProvider>>(
				_registrations
				.Where(r =>
					(!supportedChannelProviders.Any() || supportedChannelProviders.Any(a => r.Id == a)) &&
					channels.Any(c => r.SupportsChannel(c.Id))
				).ToList()
			);
		}

		/// <summary>
		/// Resolves the client for the specified channel provider.
		/// </summary>
		/// <param name="registration">The channel provider registration.</param>
		/// <returns>The channel provider client.</returns>
		public Task<IChannelProviderClient> ResolveClientAsync(IChannelProvider registration)
		{
			return Task.FromResult(registration.GetClient());
		}
	}

}