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

namespace Transmitly.ChannelProvider.Configuration;

public sealed class ChannelProviderRegistrationBuilder
{
	private readonly ChannelProviderConfigurationBuilder _communicationsClientBuilder;
	private readonly List<IChannelProviderDispatcherRegistration> _channelProviderDispatcherRegistrations = [];
	private readonly List<IDeliveryReportRequestAdaptorRegistration> _channelProviderDeliveryReportRequestAdaptorRegistrations = [];

	internal IReadOnlyCollection<IDeliveryReportRequestAdaptorRegistration> DeliveryReportRegistrationAdaptorRegistrations => _channelProviderDeliveryReportRequestAdaptorRegistrations.AsReadOnly();
	internal IReadOnlyCollection<IChannelProviderDispatcherRegistration> DispatcherRegistration => _channelProviderDispatcherRegistrations.AsReadOnly();
	internal string ProviderId { get; }
	internal object? Configuration { get; }

	internal ChannelProviderRegistrationBuilder(ChannelProviderConfigurationBuilder builder, string providerId, object? configuration)
	{
		_communicationsClientBuilder = builder;
		ProviderId = providerId;
		Configuration = configuration;
	}

	public ChannelProviderRegistrationBuilder AddDispatcher<TDispatcher, TCommunication>(params string[] supportedChannelIds)
		where TDispatcher : IChannelProviderDispatcher<TCommunication>
	{
		_channelProviderDispatcherRegistrations.Add(new ChannelProviderDispatcherRegistration<TDispatcher, TCommunication>(supportedChannelIds));
		return this;
	}

	public ChannelProviderRegistrationBuilder AddDeliveryReportRequestAdaptor<TAdaptor>()
		where TAdaptor : IChannelProviderDeliveryReportRequestAdaptor
	{
		_channelProviderDeliveryReportRequestAdaptorRegistrations.Add(new DeliveryReportRequestAdaptorRegistration(typeof(TAdaptor)));
		return this;
	}

	public void Register()
	{
		_communicationsClientBuilder.Add(ProviderId, _channelProviderDispatcherRegistrations, _channelProviderDeliveryReportRequestAdaptorRegistrations, Configuration);
	}
}