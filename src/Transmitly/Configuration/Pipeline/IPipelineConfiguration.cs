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
	/// <summary>
	/// Configuration of a pipeline
	/// </summary>
	public interface IPipelineConfiguration
	{
		/// <summary>
		/// Gets the list of available channels registered in the pipeline.
		/// </summary>
		/// <see cref="AddChannel(IChannel)"/>
		IReadOnlyCollection<IChannel> Channels { get; }
		/// <summary>
		/// Gets the list of persona filters registered in the pipeline.
		/// </summary>
		IReadOnlyCollection<string> PersonaFilters { get; }
		/// <summary>
		/// Gets the registered channel sending strategy provider
		/// </summary>
		BasePipelineDeliveryStrategyProvider PipelineDeliveryStrategyProvider { get; }
		/// <summary>
		/// Priority of the transportation method.
		/// </summary>
		TransportPriority TransportPriority { get; set; }
		/// <summary>
		/// Priority of the message.
		/// </summary>
		MessagePriority MessagePriority { get; set; }
		/// <summary>
		/// Recipients to copy the message to (if supported).
		/// </summary>
		/// <param name="platformIdentityType"></param>
		/// <returns>Pipeline configuration</returns>
		IPipelineConfiguration CopyIdentityAddress(params string[] platformIdentityType);
		/// <summary>
		/// Recipients to blind copy the message to (if supported).
		/// </summary>
		/// <param name="platformIdentityType"></param>
		/// <returns>Pipeline configuration</returns>
		IPipelineConfiguration BlindCopyIdentityAddress(params string[] platformIdentityType);
		/// <summary>
		/// Registers a communication channel with the pipeline.
		/// </summary>
		/// <param name="channel"><see cref="IChannelConfiguration"/> to register.</param>
		/// <returns>Pipeline configuration</returns>
		IPipelineConfiguration AddChannel(IChannel channel);
		/// <summary>
		/// Sets the pipeline sending strategy provider.
		/// </summary>
		/// <param name="deliveryStrategyProvider">Sending strategy provider.</param>
		/// <returns>Pipeline configuration.</returns>
		IPipelineConfiguration UsePipelineDeliveryStrategy(BasePipelineDeliveryStrategyProvider deliveryStrategyProvider);
		/// <summary>
		/// Adds a persona filter to the pipeline configuration.
		/// </summary>
		/// <param name="personaName">Name of the registered persona filter.</param>
		/// <returns>Pipeline configuration.</returns>
		IPipelineConfiguration AddPersonaFilter(string personaName);
	}
}