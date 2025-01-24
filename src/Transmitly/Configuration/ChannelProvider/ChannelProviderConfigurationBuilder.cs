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
		/// <typeparam name="TDispatcher">Channel Provider DispatcherType</typeparam>
		/// <param name="providerId">Id of the provider.</param>
		/// <param name="supportedChannelIds">Channels supported by this provider.</param>
		/// <returns></returns>
		public CommunicationsClientBuilder Add<TDispatcher>(string providerId, params string[]? supportedChannelIds)
			where TDispatcher : IChannelProviderDispatcher
		{
			return Add(providerId, typeof(TDispatcher), typeof(object), supportedChannelIds);
		}

		public CommunicationsClientBuilder Add(IChannelProviderRegistration channelProviderRegistration)
		{
			_addProvider(Guard.AgainstNull(channelProviderRegistration));
			return _communicationsConfiguration;
		}

		/// <summary>
		/// Adds a channel provider to the communications configuration.
		/// </summary>
		/// <typeparam name="TDispatcher">Channel Provider Dispatcher Type</typeparam>
		/// <typeparam name="TCommunication">Communication Type. (see: <see cref="IEmail"/>, <see cref="ISms"/></typeparam>
		/// <param name="providerId">Id of the provider.</param>
		/// <param name="configuration">Dispatcher configuration object.</param>
		/// <param name="supportedChannelIds">Channels supported by this provider.</param>
		/// <returns></returns>
		public CommunicationsClientBuilder Add<TDispatcher, TCommunication>(string providerId, object? configuration, params string[]? supportedChannelIds)
			where TDispatcher : IChannelProviderDispatcher<TCommunication>
		{
			_addProvider(
				new ChannelProviderRegistration(
					Guard.AgainstNullOrWhiteSpace(providerId),
					[new ChannelProviderDispatcherRegistration<TDispatcher, TCommunication>(supportedChannelIds)],
					[],
					configuration: configuration
					)
			);
			return _communicationsConfiguration;
		}

		public CommunicationsClientBuilder Add(
			string providerId,
			IReadOnlyCollection<IChannelProviderDispatcherRegistration>? channelProviderDispatcherRegistrations,
			IReadOnlyCollection<IDeliveryReportRequestAdaptorRegistration>? deliveryReportRequestAdaptorRegistrations,
			object? configuration)
		{
			_addProvider(new ChannelProviderRegistration(
					Guard.AgainstNullOrWhiteSpace(providerId),
					channelProviderDispatcherRegistrations,
					deliveryReportRequestAdaptorRegistrations,
					configuration: configuration
				)
			);
			return _communicationsConfiguration;
		}

		/// <summary>
		/// Adds a channel provider to the communications configuration.
		/// </summary>
		/// <typeparam name="TDispatcher">Channel Provider Dispatcher Type</typeparam>
		/// <typeparam name="TCommunication">Communication Type. (see: <see cref="IEmail"/>, <see cref="ISms"/></typeparam>
		/// <param name="providerId">Id of the provider.</param>
		/// <param name="supportedChannelIds">Channels supported by this provider.</param>
		/// <returns></returns>
		public CommunicationsClientBuilder Add<TDispatcher, TCommunication>(string providerId, params string[]? supportedChannelIds)
			where TDispatcher : IChannelProviderDispatcher<TCommunication>
		{
			return Add<TDispatcher, TCommunication>(providerId, configuration: null, supportedChannelIds);
		}

		/// <summary>
		/// Adds a channel provider to the communications configuration.
		/// </summary>
		/// <param name="providerId"></param>
		/// <param name="dispatcherType"></param>
		/// <param name="communicationType"></param>
		/// <param name="supportedChannelIds"></param>
		/// <returns></returns>
		/// <exception cref="CommunicationsException"></exception>
		public CommunicationsClientBuilder Add(string providerId, Type dispatcherType, Type communicationType, params string[]? supportedChannelIds)
		{
			Guard.AgainstNull(dispatcherType);
			Guard.AgainstNull(communicationType);

			if (!(typeof(IChannelProviderDispatcher).IsAssignableFrom(dispatcherType)))
			{
				throw new CommunicationsException($"{nameof(dispatcherType)} must be of type, {nameof(IChannelProviderDispatcher)}");
			}
			if (!typeof(IChannelProviderDispatcher<>).MakeGenericType(communicationType).IsAssignableFrom(dispatcherType))
			{
				throw new CommunicationsException($"{nameof(dispatcherType)} must be of type, {nameof(IChannelProviderDispatcher)}<{communicationType.Name}>");
			}

			var dispatcherRegistration = Activator.CreateInstance(typeof(ChannelProviderDispatcherRegistration<,>).MakeGenericType(dispatcherType, communicationType), supportedChannelIds) as IChannelProviderDispatcherRegistration
				?? throw new CommunicationsException("Unable to create channel provider dispatcher registration type with provided dispatcher type and communication type.");

			_addProvider(new ChannelProviderRegistration(providerId, [dispatcherRegistration], [], null));
			return _communicationsConfiguration;
		}

		public ChannelProviderRegistrationBuilder Build(string providerId, object? configuration = null)
		{
			return new ChannelProviderRegistrationBuilder(this, providerId, configuration);
		}
	}
}