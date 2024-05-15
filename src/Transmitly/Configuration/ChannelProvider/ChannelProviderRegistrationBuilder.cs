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
		private readonly List<ISenderVerificationClientRegistration> _senderVerificationClientRegistrations = [];

		internal IReadOnlyCollection<ISenderVerificationClientRegistration> SenderVerificationClientRegistrations => _senderVerificationClientRegistrations.AsReadOnly();
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

		public ChannelProviderRegistrationBuilder AddSenderVerificationClient<TClient>(bool isRequired, object? configuration = null, params string[] supportedChannelIds)
			where TClient : ISenderVerificationChannelProviderClient
		{
			_senderVerificationClientRegistrations.Add(new SenderVerificationClientRegistration<TClient>(isRequired, configuration, supportedChannelIds));
			return this;
		}

		public ChannelProviderRegistrationBuilder AddSenderVerificationClient<TClient>(object? configuration = null, params string[] supportedChannelIds)
			where TClient : ISenderVerificationChannelProviderClient
		{
			return AddSenderVerificationClient<TClient>(false, configuration, supportedChannelIds);
		}

		public ChannelProviderRegistrationBuilder AddSenderVerificationClient(Type clientType, bool isRequired, params string[] supportedChannelIds)
		{
			if (!typeof(ISenderVerificationChannelProviderClient).IsAssignableFrom(clientType))
				throw new Transmitly.Exceptions.CommunicationsException("Provided type must implement " + nameof(ISenderVerificationChannelProviderClient));

			var registration = Activator.CreateInstance(typeof(SenderVerificationClientRegistration<>).MakeGenericType(clientType), isRequired, supportedChannelIds) as ISenderVerificationClientRegistration;
			_senderVerificationClientRegistrations.Add(Guard.AgainstNull(registration));
			return this;
		}

		public ChannelProviderRegistrationBuilder AddSenderVerificationClient(Type clientType, params string[] supportedChannelIds)
		{
			return AddSenderVerificationClient(clientType, false, supportedChannelIds);
		}

		public void Register()
		{
			_communicationsClientBuilder.Add(ProviderId, _channelProviderClientRegistrations, _senderVerificationClientRegistrations, _channelProviderDeliveryReportRequestAdaptorRegistrations, Configuration);
		}
	}
}