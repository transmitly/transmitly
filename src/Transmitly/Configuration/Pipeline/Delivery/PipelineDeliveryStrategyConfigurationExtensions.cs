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
using Transmitly.Pipeline.Configuration;

namespace Transmitly
{
	public static class PipelineDeliveryStrategyConfigurationExtensions
	{
		public static void UseFirstMatchPipelineDeliveryStrategy(this IPipelineChannelConfiguration pipelineChannelConfiguration)
		{
			Guard.AgainstNull(pipelineChannelConfiguration);
			pipelineChannelConfiguration.UsePipelineDeliveryStrategy(new FirstMatchPipelineDeliveryStrategy());
		}

		public static void UseAnyMatchPipelineDeliveryStrategy(this IPipelineChannelConfiguration pipelineChannelConfiguration)
		{
			Guard.AgainstNull(pipelineChannelConfiguration);
			pipelineChannelConfiguration.UsePipelineDeliveryStrategy(new AnyMatchPipelineDeliveryStrategy());
		}

		public static void UsePipelineDeliveryStrategy(this IPipelineChannelConfiguration pipelineChannelConfiguration, BasePipelineDeliveryStrategyProvider pipelineDeliveryStrategyProvider)
		{
			Guard.AgainstNull(pipelineChannelConfiguration);
			Guard.AgainstNull(pipelineDeliveryStrategyProvider);
			pipelineChannelConfiguration.UsePipelineDeliveryStrategy(pipelineDeliveryStrategyProvider);
		}

		public static void UseDefaultPipelineDeliveryStrategy(this IPipelineChannelConfiguration pipelineChannelConfiguration)
		{
			UseAnyMatchPipelineDeliveryStrategy(pipelineChannelConfiguration);
		}
	}
}
