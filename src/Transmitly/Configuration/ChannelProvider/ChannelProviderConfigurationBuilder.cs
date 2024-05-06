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

using Transmitly.Exceptions;
using Transmitly.Delivery;
using Transmitly.Verification;
using System.Net.Sockets;
using Transmitly.Verification.Configuration;

namespace Transmitly.ChannelProvider.Configuration
{
	/// <summary>
	/// A builder for configuring channel providers in the communications configuration.
	/// </summary>
	public sealed class ChannelProviderConfigurationBuilder
	{
		private readonly CommunicationsClientBuilder _communicationsConfiguration;
		private readonly Action<IChannelProviderRegistration> _addProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="ChannelProviderConfigurationBuilder"/> class.
		/// </summary>
		/// <param name="communicationsConfiguration">The communications configuration builder.</param>
		/// <param name="addProvider">The action to add a channel provider.</param>
		internal ChannelProviderConfigurationBuilder(CommunicationsClientBuilder communicationsConfiguration, Action<IChannelProviderRegistration> addProvider)
		{
			_communicationsConfiguration = Guard.AgainstNull(communicationsConfiguration);
			_addProvider = Guard.AgainstNull(addProvider);
		}

		/// <summary>
		/// Adds a channel provider to the communications configuration
		/// </summary>
		/// <typeparam name="TClient">Channel Provider Type</typeparam>
		/// <param name="providerId">Id of the provider.</param>
		/// <param name="supportedChannelIds">Channels supported by this provider.</param>
		/// <returns></returns>
		public CommunicationsClientBuilder Add<TClient>(string providerId, params string[]? supportedChannelIds)
			where TClient : IChannelProviderClient
		{
			return Add(providerId, typeof(TClient), typeof(object), supportedChannelIds);
		}

		public CommunicationsClientBuilder Add(IChannelProviderRegistration channelProviderClientRegistration)
		{
			_addProvider(Guard.AgainstNull(channelProviderClientRegistration));
			return _communicationsConfiguration;
		}

		/// <summary>
		/// Adds a channel provider to the communications configuration.
		/// </summary>
		/// <typeparam name="TClient">Channel Provider Client Type</typeparam>
		/// <typeparam name="TCommunication">Communication Type. (see: <see cref="IEmail"/>, <see cref="ISms"/></typeparam>
		/// <param name="providerId">Id of the provider.</param>
		/// <param name="configuration">Client configuration object.</param>
		/// <param name="supportedChannelIds">Channels supported by this provider.</param>
		/// <returns></returns>
		public CommunicationsClientBuilder Add<TClient, TCommunication>(string providerId, object? configuration, params string[]? supportedChannelIds)
			where TClient : IChannelProviderClient<TCommunication>
		{
			_addProvider(
				new ChannelProviderRegistration(
					Guard.AgainstNullOrWhiteSpace(providerId),
					[new ChannelProviderClientRegistration<TClient, TCommunication>(supportedChannelIds)],
					[],
					[],
					configuration: configuration
					)
			);
			return _communicationsConfiguration;
		}

		public CommunicationsClientBuilder Add(
			string providerId,
			IReadOnlyCollection<IChannelProviderClientRegistration>? channelProviderClientRegistrations,
			IReadOnlyCollection<ISenderVerificationClientRegistration>? senderVerificationClientRegistrations,
			IReadOnlyCollection<IDeliveryReportRequestAdaptorRegistration>? deliveryReportRequestAdaptorRegistrations,
			object? configuration)
		{
			_addProvider(new ChannelProviderRegistration(
					Guard.AgainstNullOrWhiteSpace(providerId),
					channelProviderClientRegistrations,
					senderVerificationClientRegistrations,
					deliveryReportRequestAdaptorRegistrations,
					configuration: configuration
				)
			);
			return _communicationsConfiguration;
		}

		/// <summary>
		/// Adds a channel provider to the communications configuration.
		/// </summary>
		/// <typeparam name="TClient">Channel Provider Client Type</typeparam>
		/// <typeparam name="TCommunication">Communication Type. (see: <see cref="IEmail"/>, <see cref="ISms"/></typeparam>
		/// <param name="providerId">Id of the provider.</param>
		/// <param name="supportedChannelIds">Channels supported by this provider.</param>
		/// <returns></returns>
		public CommunicationsClientBuilder Add<TClient, TCommunication>(string providerId, params string[]? supportedChannelIds)
			where TClient : IChannelProviderClient<TCommunication>
		{
			return Add<TClient, TCommunication>(providerId, configuration: null, supportedChannelIds);
		}

		/// <summary>
		/// Adds a channel provider to the communications configuration.
		/// </summary>
		/// <param name="providerId"></param>
		/// <param name="clientType"></param>
		/// <param name="communicationType"></param>
		/// <param name="supportedChannelIds"></param>
		/// <returns></returns>
		/// <exception cref="CommunicationsException"></exception>
		public CommunicationsClientBuilder Add(string providerId, Type clientType, Type communicationType, params string[]? supportedChannelIds)
		{
			Guard.AgainstNull(clientType);
			Guard.AgainstNull(communicationType);

			if (!(typeof(IChannelProviderClient).IsAssignableFrom(clientType)))
			{
				throw new CommunicationsException($"{nameof(clientType)} must be of type, {nameof(IChannelProviderClient)}");
			}
			if (!typeof(IChannelProviderClient<>).MakeGenericType(communicationType).IsAssignableFrom(clientType))
			{
				throw new CommunicationsException($"{nameof(clientType)} must be of type, {nameof(IChannelProviderClient)}<{communicationType.Name}>");
			}

			var clientRegistration = Activator.CreateInstance(typeof(ChannelProviderClientRegistration<,>).MakeGenericType(clientType, communicationType), supportedChannelIds) as IChannelProviderClientRegistration
				?? throw new CommunicationsException("Unable to create channel provider client registration type with provided client type and communication type.");

			_addProvider(new ChannelProviderRegistration(providerId, [clientRegistration], [], [], null));
			return _communicationsConfiguration;
		}

		public ChannelProviderRegistrationBuilder Create(string providerId, object? configuration)
		{
			return new ChannelProviderRegistrationBuilder(this, providerId, configuration);
		}
	}
}