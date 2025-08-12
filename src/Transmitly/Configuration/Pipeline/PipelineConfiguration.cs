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

namespace Transmitly.Pipeline.Configuration;

/// <summary>
/// Configuration builder for communication pipelines.
/// </summary>
public sealed class PipelineConfigurationBuilder
{
	private readonly CommunicationsClientBuilder _communicationsConfiguration;
	private readonly Action<IPipeline> _addPipeline;

	/// <summary>
	/// Creates an instance of <see cref="PipelineConfigurationBuilder"/>.
	/// </summary>
	/// <param name="communicationsConfiguration">The configuration builder</param>
	/// <param name="addPipeline">The action to add a pipeline to the configuration</param>
	internal PipelineConfigurationBuilder(CommunicationsClientBuilder communicationsConfiguration, Action<IPipeline> addPipeline)
	{
		_communicationsConfiguration = Guard.AgainstNull(communicationsConfiguration);
		_addPipeline = Guard.AgainstNull(addPipeline);
	}

	/// <summary>
	/// Adds a pipeline to the communication configuration with the specified intent name, category, transport priority, message priority, and options.
	/// </summary>
	/// <param name="intentName">The named intent of the pipeline</param>
	/// <param name="category">The category of the pipeline (optional)</param>
	/// <param name="options">The configuration options for the pipeline</param>
	/// <returns>The updated communication configuration builder</returns>
	public CommunicationsClientBuilder Add(string intentName, string? category, Action<IPipelineConfiguration> options)
	{
		var detailConfig = new DefaultPipelineProviderConfiguration();
		options(detailConfig);
		_addPipeline(new PipelineRegistration(detailConfig, intentName, null, category));
		return _communicationsConfiguration;
	}

	/// <summary>
	/// Adds a pipeline to the communication configuration with the specified intent name, and default category.
	/// </summary>
	/// <param name="intentName">The named intent of the pipeline</param>
	/// <param name="options">The configuration options for the pipeline</param>
	/// <returns>The updated communication configuration builder</returns>
	public CommunicationsClientBuilder Add(string intentName, Action<IPipelineConfiguration> options)
	{
		return Add(intentName, null, options);
	}

	/// <summary>
	/// Configures a pipeline with a provided configurator.
	/// </summary>
	/// <param name="configurator">The configurator to add.</param>
	/// <returns>The configuration builder.</returns>
	public CommunicationsClientBuilder AddConfigurator(IPipelineConfigurator configurator)
	{
		Guard.AgainstNull(configurator);

		var config = new DefaultPipelineProviderConfiguration();
		configurator.Configure(config);
		_addPipeline(new PipelineRegistration(config, configurator.Name, null, configurator.Category));
		return _communicationsConfiguration;
	}
}