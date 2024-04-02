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

namespace Transmitly.ChannelProvider.Configuration
{
	/// <summary>
	/// Creates a new instance of <see cref="BaseChannelProviderFactory"/>
	/// </summary>
	/// <param name="registrations">Enumerable of registered channel providers.</param>
	/// <param name="adaptorRegistrations">Enumerable of registered channel provider request adaptor registrations.</param>
	/// <exception cref="ArgumentNullException">When the registrations is null</exception>
	/// 
	public abstract class BaseChannelProviderFactory(IEnumerable<IChannelProviderRegistration> registrations, IEnumerable<IChannelProviderDeliveryReportRequestAdaptorRegistration> adaptorRegistrations) : IChannelProviderFactory
	{
		private readonly List<IChannelProviderRegistration> _registrations = Guard.AgainstNull(registrations).ToList();
		protected IReadOnlyCollection<IChannelProviderRegistration> Registrations => _registrations.AsReadOnly();

		private readonly List<IChannelProviderDeliveryReportRequestAdaptorRegistration> _adaptorRegistrations = Guard.AgainstNull(adaptorRegistrations).ToList();
		protected IReadOnlyCollection<IChannelProviderDeliveryReportRequestAdaptorRegistration> AdaptorRegistrations => _adaptorRegistrations.AsReadOnly();

		///<inheritdoc/>
		public virtual Task<IReadOnlyCollection<IChannelProviderRegistration>> GetAllAsync()
		{
			return Task.FromResult(Registrations);
		}

		///<inheritdoc/>
		public virtual Task<IReadOnlyCollection<IChannelProviderRegistration>> GetAllAsync(IReadOnlyCollection<string> supportedChannelProviders, IReadOnlyCollection<IChannel> channels)
		{
			return Task.FromResult<IReadOnlyCollection<IChannelProviderRegistration>>(
				_registrations
				.Where(r =>
					(supportedChannelProviders.Count == 0 || supportedChannelProviders.Any(a => r.Id == a)) &&
					channels.Any(c => r.SupportsChannel(c.Id))
				).ToList().AsReadOnly()
			);
		}

		///<inheritdoc/>
		public abstract Task<IChannelProviderClient> ResolveClientAsync(IChannelProviderRegistration channelProvider);

		///<inheritdoc/>
		public abstract Task<IChannelProviderDeliveryReportRequestAdaptor> ResolveDeliveryReportRequestAdaptorAsync(IChannelProviderDeliveryReportRequestAdaptorRegistration channelProviderDeliveryReportRequestAdaptor);

		///<inheritdoc/>
		public virtual Task<IReadOnlyCollection<IChannelProviderDeliveryReportRequestAdaptorRegistration>> GetAllDeliveryReportRequestAdaptorsAsync()
		{
			return Task.FromResult(AdaptorRegistrations);
		}
	}
}