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
    ///<inheritdoc cref="IPipelineChannelConfiguration"/>
    internal class DefaultPipelineProviderConfiguration : IPipelineChannelConfiguration
    {
        private readonly List<IChannel> _channels = [];
        private readonly List<string> _personaFilters = [];
        /// <inheritdoc />
        public TransportPriority TransportPriority { get; set; } = TransportPriority.Normal;

        /// <inheritdoc />
        public MessagePriority MessagePriority { get; set; } = MessagePriority.Normal;

        /// <inheritdoc />
        public ICollection<string> BlindCopyIdentityAddresses { get; } = [];

        /// <inheritdoc />
        public ICollection<string> CopyIdentityAddresses { get; } = [];

        public IReadOnlyCollection<string> PersonaFilters => _personaFilters.AsReadOnly();
        /// <inheritdoc />
        public BasePipelineDeliveryStrategyProvider PipelineDeliveryStrategyProvider { get; private set; } = new FirstMatchPipelineDeliveryStrategy();

        /// <inheritdoc />
        public IReadOnlyCollection<IChannel> Channels => _channels;

        /// <inheritdoc />
        public void AddChannel(IChannel channel)
        {
            _channels.Add(Guard.AgainstNull(channel));
        }

        /// <inheritdoc />
        public void BlindCopyIdentityAddress(params string[] platformIdentityType)
        {
            Array.ForEach(platformIdentityType, BlindCopyIdentityAddresses.Add);
        }

        /// <inheritdoc />
        public void UsePipelineDeliveryStrategy(BasePipelineDeliveryStrategyProvider deliveryStrategyProvider)
        {
            PipelineDeliveryStrategyProvider = Guard.AgainstNull(deliveryStrategyProvider);
        }

        /// <inheritdoc />
        public void CopyIdentityAddress(params string[] platformIdentityType)
        {
            Array.ForEach(platformIdentityType, CopyIdentityAddresses.Add);
        }

        public void AddPersonaFilter(string personaName)
        {
            Guard.AgainstNullOrWhiteSpace(personaName);

            if (!_personaFilters.Exists(a => a.Equals(personaName, StringComparison.OrdinalIgnoreCase)))
                _personaFilters.Add(personaName);
        }
    }
}
