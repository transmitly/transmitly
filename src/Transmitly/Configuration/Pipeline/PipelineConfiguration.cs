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

namespace Transmitly.Pipeline.Configuration
{
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
		/// Adds a pipeline to the communication configuration with the specified name, category, transport priority, message priority, and options.
		/// </summary>
		/// <param name="name">The name of the pipeline</param>
		/// <param name="category">The category of the pipeline (optional)</param>
		/// <param name="transportPriority">The transport priority of the pipeline</param>
		/// <param name="messagePriority">The message priority of the pipeline</param>
		/// <param name="options">The configuration options for the pipeline</param>
		/// <returns>The updated communication configuration builder</returns>
		public CommunicationsClientBuilder Add(string name, string? category, TransportPriority transportPriority, MessagePriority messagePriority, Action<IPipelineConfiguration> options)
		{
			var detailConfig = new DefaultPipelineProviderConfiguration();
			options(detailConfig);
			detailConfig.Build();
			_addPipeline(new PipelineRegistration(detailConfig, name, null, category, transportPriority, messagePriority));
			return _communicationsConfiguration;
		}

		/// <summary>
		/// Adds a pipeline to the communication configuration with the specified name, and category.
		/// </summary>
		/// <param name="name">The name of the pipeline</param>
		/// <param name="category">The category of the pipeline (optional)</param>
		/// <param name="options">The configuration options for the pipeline</param>
		/// <returns>The updated communication configuration builder</returns>
		public CommunicationsClientBuilder Add(string name, string? category, Action<IPipelineConfiguration> options)
		{
			return Add(name, category, TransportPriority.Normal, MessagePriority.Normal, options);
		}

		/// <summary>
		/// Adds a pipeline to the communication configuration with the specified name, and default category.
		/// </summary>
		/// <param name="name">The name of the pipeline</param>
		/// <param name="options">The configuration options for the pipeline</param>
		/// <returns>The updated communication configuration builder</returns>
		public CommunicationsClientBuilder Add(string name, Action<IPipelineConfiguration> options)
		{
			return Add(name, null, options);
		}

		/// <summary>
		/// Adds a pipeline to the communication configuration with the specified name, transport priority, message priority, and category.
		/// </summary>
		/// <param name="name">The name of the pipeline</param>
		/// <param name="transportPriority">The transport priority of the pipeline</param>
		/// <param name="messagePriority">The message priority of the pipeline</param>
		/// <param name="options">The configuration options for the pipeline</param>
		/// <returns>The updated communication configuration builder</returns>
		public CommunicationsClientBuilder Add(string name, TransportPriority transportPriority, MessagePriority messagePriority, Action<IPipelineConfiguration> options)
		{
			return Add(name, null, transportPriority, messagePriority, options);
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
			config.Build();
			_addPipeline(new PipelineRegistration(config, configurator.Name, null, configurator.Category, config.TransportPriority, config.MessagePriority));
			return _communicationsConfiguration;
		}
	}
}