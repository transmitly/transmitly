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
	/// <inheritdoc cref="IPipelineConfiguration"/>
	internal class DefaultPipelineProviderConfiguration : IPipelineConfiguration
	{
		private readonly List<IChannelRegistration> _channelRegistrations = [];
		private readonly List<string> _personaFilters = [];
		private DefaultPipelineChannelConfiguration? _activeChannel;
		private readonly List<string> _platformIdentityTypeFilters = [];

		public TransportPriority TransportPriority { get; set; } = TransportPriority.Normal;

		public MessagePriority MessagePriority { get; set; } = MessagePriority.Normal;

		public IReadOnlyCollection<string> PersonaFilters => _personaFilters.AsReadOnly();

		public string? PipelineId { get; set; }

		public BasePipelineDeliveryStrategyProvider PipelineDeliveryStrategyProvider { get; private set; } = new FirstMatchPipelineDeliveryStrategy();

		public IReadOnlyCollection<IChannelRegistration> ChannelRegistrations => _channelRegistrations.AsReadOnly();

		public IReadOnlyCollection<string> PlatformIdentityTypeFilters => _platformIdentityTypeFilters.AsReadOnly();

		public string? Description { get; set; }

		public IPipelineChannelConfiguration AddChannel(IChannel channel)
		{
			EndActiveChannelConfiguration();
			_activeChannel = new DefaultPipelineChannelConfiguration(this, new ChannelRegistration(channel));
			return _activeChannel;
		}

		public IPipelineConfiguration UsePipelineDeliveryStrategy(BasePipelineDeliveryStrategyProvider deliveryStrategyProvider)
		{
			PipelineDeliveryStrategyProvider = Guard.AgainstNull(deliveryStrategyProvider);
			return this;
		}

		public IPipelineConfiguration AddPersonaFilter(string personaName)
		{
			Guard.AgainstNullOrWhiteSpace(personaName);

			if (!_personaFilters.Exists(a => a.Equals(personaName, StringComparison.OrdinalIgnoreCase)))
				_personaFilters.Add(personaName);

			return this;
		}

		public IPipelineConfiguration AddPlatformIdentityTypeFilter(string platformIdentityType)
		{
			Guard.AgainstNullOrWhiteSpace(platformIdentityType);

			if (!_personaFilters.Exists(a => a.Equals(platformIdentityType, StringComparison.OrdinalIgnoreCase)))
				_personaFilters.Add(platformIdentityType);

			return this;
		}

		public IPipelineConfiguration Id(string id)
		{
			PipelineId = id;
			return this;
		}

		public void Build()
		{
			EndActiveChannelConfiguration();
		}

		private void EndActiveChannelConfiguration()
		{
			if (_activeChannel == null)
				return;

			_channelRegistrations.Add(_activeChannel.Registration);
			_activeChannel = null;
		}
	}
}