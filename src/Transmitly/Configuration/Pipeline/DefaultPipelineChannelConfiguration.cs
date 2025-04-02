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
using Transmitly.Delivery;

namespace Transmitly.Pipeline.Configuration
{
	internal class DefaultPipelineChannelConfiguration(DefaultPipelineProviderConfiguration pipelineConfiguration, ChannelRegistration channelRegistration) : IPipelineChannelConfiguration
	{
		private readonly DefaultPipelineProviderConfiguration _pipeline = Guard.AgainstNull(pipelineConfiguration);
		private readonly ChannelRegistration _registration = Guard.AgainstNull(channelRegistration);
		internal ChannelRegistration Registration => _registration;

		public IReadOnlyCollection<IChannelRegistration> ChannelRegistrations => _pipeline.ChannelRegistrations;

		public IReadOnlyCollection<string> PersonaFilters => _pipeline.PersonaFilters;

		public BasePipelineDeliveryStrategyProvider PipelineDeliveryStrategyProvider => _pipeline.PipelineDeliveryStrategyProvider;

		public TransportPriority TransportPriority { get => _pipeline.TransportPriority; set => _pipeline.TransportPriority = value; }
		public MessagePriority MessagePriority { get => _pipeline.MessagePriority; set => _pipeline.MessagePriority = value; }

		public IPipelineChannelConfiguration AddChannel(IChannel channel)
		{
			return _pipeline.AddChannel(channel);
		}

		public IPipelineConfiguration AddPersonaFilter(string personaName)
		{
			return _pipeline.AddPersonaFilter(personaName);
		}

		public IPipelineConfiguration AddPlatformIdentityTypeFilter(string platformIdentityType)
		{
			return _pipeline.AddPlatformIdentityTypeFilter(platformIdentityType);
		}

		public IPipelineChannelConfiguration BlindCopyAddress(string address)
		{
			_registration.BlindCopyAddress = address;
			return this;
		}

		public IPipelineChannelConfiguration BlindCopyAddressPurpose(string purpose)
		{
			_registration.BlindCopyAddressPurpose = purpose;
			return this;
		}

		public IPipelineChannelConfiguration ChannelProviderFilter(params string[]? providerIds)
		{
			_registration.FilterChannelProviderIds = providerIds ?? [];
			return this;
		}

		public IPipelineChannelConfiguration CompleteOnDispatched(bool completeOnDispatched = true)
		{
			_registration.CompleteOnDispatch = completeOnDispatched;
			return this;
		}

		public IPipelineChannelConfiguration CopyAddress(string address)
		{
			_registration.CopyAddress = address;
			return this;
		}

		public IPipelineChannelConfiguration CopyAddressPurpose(string purpose)
		{
			_registration.CopyAddressPurpose = purpose;
			return this;
		}

		public IPipelineConfiguration Id(string id)
		{
			return _pipeline.Id(id);
		}

		public IPipelineChannelConfiguration ToAddress(string address)
		{
			_registration.ToAddress = address;
			return this;
		}

		public IPipelineChannelConfiguration ToAddressPurpose(string purpose)
		{
			_registration.ToAddressPurpose = purpose;
			return this;
		}

		public IPipelineConfiguration UsePipelineDeliveryStrategy(BasePipelineDeliveryStrategyProvider deliveryStrategyProvider)
		{
			return _pipeline.UsePipelineDeliveryStrategy(deliveryStrategyProvider);
		}

	}
}