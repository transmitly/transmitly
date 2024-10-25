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

using Transmitly.Delivery;
using Transmitly.Verification;

namespace Transmitly.ChannelProvider.Configuration
{
	public sealed class ChannelProviderRegistrationBuilder
	{
		private readonly ChannelProviderConfigurationBuilder _communicationsClientBuilder;
		private readonly List<IChannelProviderClientRegistration> _channelProviderClientRegistrations = [];
		private readonly List<IDeliveryReportRequestAdaptorRegistration> _channelProviderDeliveryReportRequestAdaptorRegistrations = [];
		private readonly List<IChannelVerificationClientRegistration> _channelVerificationClientRegistrations = [];

		internal IReadOnlyCollection<IChannelVerificationClientRegistration> ChannelVerificationClientRegistrations => _channelVerificationClientRegistrations.AsReadOnly();
		internal IReadOnlyCollection<IDeliveryReportRequestAdaptorRegistration> DeliveryReportRegistrationAdaptorRegistrations => _channelProviderDeliveryReportRequestAdaptorRegistrations.AsReadOnly();
		internal IReadOnlyCollection<IChannelProviderClientRegistration> ClientRegistration => _channelProviderClientRegistrations.AsReadOnly();
		internal string ProviderId { get; }
		internal object? Configuration { get; }

		internal ChannelProviderRegistrationBuilder(ChannelProviderConfigurationBuilder communicationsClientBuilder, string providerId, object? configuration)
		{
			_communicationsClientBuilder = communicationsClientBuilder;
			ProviderId = providerId;
			Configuration = configuration;
		}

		public ChannelProviderRegistrationBuilder AddClient<TClient, TCommunication>(params string[] supportedChannelIds)
			where TClient : IChannelProviderClient<TCommunication>
		{
			_channelProviderClientRegistrations.Add(new ChannelProviderClientRegistration<TClient, TCommunication>(supportedChannelIds));
			return this;
		}

		public ChannelProviderRegistrationBuilder AddDeliveryReportRequestAdaptor<TAdaptor>()
			where TAdaptor : IChannelProviderDeliveryReportRequestAdaptor
		{
			_channelProviderDeliveryReportRequestAdaptorRegistrations.Add(new DeliveryReportRequestAdaptorRegistration(typeof(TAdaptor)));
			return this;
		}

		public ChannelProviderRegistrationBuilder AddChannelVerificationClient<TClient>(object? configuration = null, params string[] supportedChannelIds)
			where TClient : IChannelVerificationChannelProviderClient
		{
			_channelVerificationClientRegistrations.Add(new ChannelVerificationClientRegistration<TClient>(configuration, supportedChannelIds));
			return this;
		}

		public ChannelProviderRegistrationBuilder AddChannelVerificationClient(Type clientType, bool isRequired, params string[] supportedChannelIds)
		{
			if (!typeof(IChannelVerificationChannelProviderClient).IsAssignableFrom(clientType))
				throw new Exceptions.CommunicationsException("Provided type must implement " + nameof(IChannelVerificationChannelProviderClient));

			var registration = Activator.CreateInstance(typeof(ChannelVerificationClientRegistration<>).MakeGenericType(clientType), isRequired, supportedChannelIds) as IChannelVerificationClientRegistration;
			_channelVerificationClientRegistrations.Add(Guard.AgainstNull(registration));
			return this;
		}

		public ChannelProviderRegistrationBuilder AddChannelVerificationClient(Type clientType, params string[] supportedChannelIds)
		{
			return AddChannelVerificationClient(clientType, false, supportedChannelIds);
		}

		public void Register()
		{
			_communicationsClientBuilder.Add(ProviderId, _channelProviderClientRegistrations, _channelVerificationClientRegistrations, _channelProviderDeliveryReportRequestAdaptorRegistrations, Configuration);
		}
	}
}