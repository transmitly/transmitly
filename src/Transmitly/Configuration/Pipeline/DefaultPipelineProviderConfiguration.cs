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
	internal class DefaultPipelineProviderConfiguration : IPipelineConfiguration
	{
		private readonly List<IChannel> _channels = [];
		private readonly List<string> _personaFilters = [];
		
		public TransportPriority TransportPriority { get; set; } = TransportPriority.Normal;

		public MessagePriority MessagePriority { get; set; } = MessagePriority.Normal;

		public ICollection<string> BlindCopyIdentityAddresses { get; } = [];

		public ICollection<string> CopyIdentityAddresses { get; } = [];

		public IReadOnlyCollection<string> PersonaFilters => _personaFilters.AsReadOnly();
		
		public BasePipelineDeliveryStrategyProvider PipelineDeliveryStrategyProvider { get; private set; } = new FirstMatchPipelineDeliveryStrategy();

		public IReadOnlyCollection<IChannel> Channels => _channels;

		public string? Description { get; set; }

		public IPipelineConfiguration AddChannel(IChannel channel)
		{
			_channels.Add(Guard.AgainstNull(channel));
			return this;
		}
		
		public IPipelineConfiguration BlindCopyIdentityAddress(params string[] platformIdentityType)
		{
			Array.ForEach(platformIdentityType, BlindCopyIdentityAddresses.Add);
			return this;
		}

		public IPipelineConfiguration UsePipelineDeliveryStrategy(BasePipelineDeliveryStrategyProvider deliveryStrategyProvider)
		{
			PipelineDeliveryStrategyProvider = Guard.AgainstNull(deliveryStrategyProvider);
			return this;
		}

		public IPipelineConfiguration CopyIdentityAddress(params string[] platformIdentityType)
		{
			Array.ForEach(platformIdentityType, CopyIdentityAddresses.Add);
			return this;
		}

		public IPipelineConfiguration AddPersonaFilter(string personaName)
		{
			Guard.AgainstNullOrWhiteSpace(personaName);

			if (!_personaFilters.Exists(a => a.Equals(personaName, StringComparison.OrdinalIgnoreCase)))
				_personaFilters.Add(personaName);

			return this;
		}
	}
}
