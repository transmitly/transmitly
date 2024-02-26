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

		/// <inheritdoc />
		public TransportPriority TransportPriority { get; set; } = TransportPriority.Normal;

		/// <inheritdoc />
		public MessagePriority MessagePriority { get; set; } = MessagePriority.Normal;

		/// <inheritdoc />
		public ICollection<string> BlindCopyAudiences { get; } = new List<string>();

		/// <inheritdoc />
		public ICollection<string> CopyAudiences { get; } = new List<string>();

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
		public void BlindCopyAudience(params string[] audienceType)
		{
			Array.ForEach(audienceType, BlindCopyAudiences.Add);
		}

		/// <inheritdoc />
		public void UsePipelineDeliveryStrategy(BasePipelineDeliveryStrategyProvider deliveryStrategyProvider)
		{
			PipelineDeliveryStrategyProvider = Guard.AgainstNull(deliveryStrategyProvider);
		}

		/// <inheritdoc />
		public void CopyAudience(params string[] audienceType)
		{
			Array.ForEach(audienceType, CopyAudiences.Add);
		}
	}
}
