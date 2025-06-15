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

namespace Transmitly.Pipeline.Configuration;

/// <summary>
/// Configuration of a pipeline
/// </summary>
public interface IPipelineConfiguration
{
	/// <summary>
	/// The unique identifier for the pipeline.
	/// <para>To set value see <see cref="Id(string)"/></para>
	/// </summary>
	string? PipelineId { get; }
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
	/// Whether the pipeline is forced to comply with the provided platform dispatch requirements. (Default = True)
	/// <para>To set value see <see cref="AllowDispatchRequirements(bool)"/></para>
	/// </summary>
	bool IsDispatchRequirementsAllowed { get; }
	/// <summary>
	/// Whether the pipeline allows dispatch channel priority preference. (Default = True)
	/// <para>To set value see <see cref="AllowDispatchChannelPriorityPreference(bool)"/></para>
	/// </summary>
	bool IsDispatchChannelPriorityPreferenceAllowed {get; } 
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
	/// <summary>
	/// Adds an Id to a pipeline to uniquely identify it from other pipelines that may have the same intent name.
	/// </summary>
	/// <param name="id">Unique id of the pipeline.</param>
	/// <returns>Pipeline configuration.</returns>
	IPipelineConfiguration Id(string id);
	/// <summary>
	/// Forces the pipeline to fail if it cannot comply to the provided platform dispatch requirements.
	/// </summary>
	/// <returns>Pipeline configuration.</returns>
	IPipelineConfiguration AllowDispatchRequirements(bool allowed);
	/// <summary>
	/// Allows the pipeline to use the dispatch channel priority preference when dispatching communications.
	/// </summary>
	IPipelineConfiguration AllowDispatchChannelPriorityPreference(bool allowed);
}