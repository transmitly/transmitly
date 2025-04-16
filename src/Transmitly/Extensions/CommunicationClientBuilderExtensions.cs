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
using Transmitly.Template.Configuration;

namespace Transmitly
{
	public static class CommunicationClientBuilderExtensions
	{
		/// <summary>
		/// Adds a template engine to the configuration.
		/// </summary>
		/// <typeparam name="TEngine">The type of the template engine.</typeparam>
		/// <param name="builder">Client builder instance.</param>
		/// <param name="id">The optional Id of the template engine.</param>
		/// <returns>The configuration builder</returns>
		public static CommunicationsClientBuilder AddTemplateEngine<TEngine>(this CommunicationsClientBuilder builder, string? id = null)
			where TEngine : ITemplateEngine, new() =>
			builder.AddTemplateEngine(new TEngine(), id);

		/// <summary>
		/// Adds a pipeline to the configuration.
		/// </summary>
		/// <param name="builder">Client builder instance.</param>
		/// <param name="name">The name of the pipeline.</param>
		/// <param name="options">The configuration options for the pipeline.</param>
		/// <returns>The configuration builder.</returns>
		public static CommunicationsClientBuilder AddPipeline(this CommunicationsClientBuilder builder, string name, Action<IPipelineConfiguration> options) =>
			builder.AddPipeline(name, null, options);

		/// <summary>
		/// Adds a delivery report handler to the configuration.
		/// </summary>
		/// <param name="builder">Client builder instance.</param>
		/// <param name="reportHandler">The event handler to register.</param>
		/// <param name="filterEventNames">List of events to listen to. See <see cref="DeliveryReport.Event"/></param>
		/// <param name="filterChannelIds">List of channel ids to listen to. See <see cref="Id.Channel"/></param>
		/// <param name="filterChannelProviderIds">List of channel provider ids to listen to. See <see cref="Id.ChannelProvider"/></param>
		/// <param name="filterPipelineNames">List of pipeline names to listen to.</param>
		/// <returns>The configuration builder.</returns>
		public static CommunicationsClientBuilder AddDeliveryReportHandler(this CommunicationsClientBuilder builder, DeliveryReportAsyncHandler reportHandler, IReadOnlyCollection<string>? filterEventNames = null, IReadOnlyCollection<string>? filterChannelIds = null, IReadOnlyCollection<string>? filterChannelProviderIds = null, IReadOnlyCollection<string>? filterPipelineNames = null)
		{
			return builder.AddDeliveryReportHandler(reportHandler, filterEventNames, filterChannelIds, filterChannelProviderIds, filterPipelineNames);
		}

	}
}