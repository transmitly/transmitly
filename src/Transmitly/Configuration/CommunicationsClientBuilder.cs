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

using System.ComponentModel;
using Transmitly.ChannelProvider;
using Transmitly.ChannelProvider.Configuration;
using Transmitly.Delivery;
using Transmitly.Delivery.Configuration;
using Transmitly.Pipeline.Configuration;
using Transmitly.Template.Configuration;
using Transmitly.Verification.Configuration;

namespace Transmitly
{
	/// <summary>
	/// Builds a <see cref="ICommunicationsClient"/>.
	/// </summary>
	public sealed class CommunicationsClientBuilder
	{
		private const string DefaultTemplateEngineId = "Default";
		private bool _clientCreated;
		private ICommunicationClientFactory _clientFactory = new DefaultCommunicationClientFactory();
		private readonly List<IChannelProviderRegistration> _channelProviders = [];
		private readonly List<IPipeline> _pipelines = [];
		//private readonly List<IAudienceResolver> _audienceResolvers = [];
		private readonly List<ITemplateEngineRegistration> _templateEngines = [];

		/// <summary>
		/// Creates an instance of the class
		/// </summary>
		public CommunicationsClientBuilder()
		{
			ChannelProvider = new(this, cp => _channelProviders.Add(cp));
			Pipeline = new(this, p => _pipelines.Add(p));
			//AudienceResolver = new(this, ar => _audienceResolvers.Add(ar));
			TemplateEngine = new(this, te => _templateEngines.Add(te));
			DeliveryReport = new(this);
			SenderVerification = new(this);
		}

		/// <summary>
		/// Gets the channel provider configuration builder.
		/// </summary>
		public ChannelProviderConfigurationBuilder ChannelProvider { get; }

		/// <summary>
		/// Gets the pipeline configuration builder.
		/// </summary>
		public PipelineConfigurationBuilder Pipeline { get; }

		// <summary>
		// Gets the audience resolver configuration builder.
		// </summary>
		//public AudienceResolverConfigurationBuilder AudienceResolver { get; }

		/// <summary>
		/// Gets the template engine configuration builder.
		/// </summary>
		public TemplateConfigurationBuilder TemplateEngine { get; }

		/// <summary>
		/// Gets the delivery report configuration builder.
		/// </summary>
		public DeliveryReportConfigurationBuilder DeliveryReport { get; }

		/// <summary>
		/// Gets the sender verification configuration builder.
		/// </summary>
		public SenderVerificationConfigurationBuilder SenderVerification { get; }

		/// <summary>
		/// Adds a template engine to the configuration.
		/// </summary>
		/// <param name="engine">The instance of the template engine.</param>
		/// <param name="id">The optional Id of the template engine.</param>
		/// <returns>The configuration builder.</returns>
		public CommunicationsClientBuilder AddTemplateEngine(ITemplateEngine engine, string? id = null) =>
			TemplateEngine.Add(engine, id);

		/// <summary>
		/// Adds a template engine to the configuration.
		/// </summary>
		/// <typeparam name="TEngine">The type of the template engine.</typeparam>
		/// <param name="id">The optional Id of the template engine.</param>
		/// <returns>The configuration builder</returns>
		public CommunicationsClientBuilder AddTemplateEngine<TEngine>(string? id = null)
			where TEngine : ITemplateEngine, new() =>
			TemplateEngine.Add(new TEngine(), id);

		///// <summary>
		///// Adds a channel provider to the configuration.
		///// </summary>
		///// <typeparam name="TCommunication">The type of communication the client will handle.</typeparam>
		///// <param name="providerId">The ID of the channel provider.</param>
		///// <param name="supportedChannelIds">The array of supported channel IDs.</param>
		///// <typeparam name="TClient">Concrete type of the channel provider client.</typeparam>
		///// <returns>The configuration builder.</returns>
		//public CommunicationsClientBuilder AddChannelProvider<TClient, TCommunication>(string providerId, params string[]? supportedChannelIds)
		//	where TClient : IChannelProviderClient<TCommunication>
		//	=> ChannelProvider.Add<TClient, TCommunication>(providerId, null, supportedChannelIds);

		///// <summary>
		///// Adds a channel provider to the configuration.
		///// </summary>
		///// <typeparam name="TCommunication">The type of communication the client will handle.</typeparam>
		///// <param name="providerId">The ID of the channel provider.</param>
		///// <param name="configuration">Configuration settings for the client.</param>
		///// <param name="supportedChannelIds">The array of supported channel IDs.</param>
		///// <typeparam name="TClient">Concrete type of the channel provider client.</typeparam>
		///// <returns>The configuration builder.</returns>
		//public CommunicationsClientBuilder AddChannelProvider<TClient, TCommunication>(string providerId, object? configuration, params string[]? supportedChannelIds)
		//	where TClient : IChannelProviderClient<TCommunication>
		//	=> ChannelProvider.Add<TClient, TCommunication>(providerId, configuration, supportedChannelIds);

		/// <summary>
		/// Adds a pipeline to the configuration.
		/// </summary>
		/// <param name="name">The name of the pipeline.</param>
		/// <param name="category">The optional category of the pipeline.</param>
		/// <param name="transportPriority">The transport priority of the pipeline.</param>
		/// <param name="messagePriority">The message priority of the pipeline.</param>
		/// <param name="options">The configuration options for the pipeline.</param>
		/// <returns>The configuration builder.</returns>
		public CommunicationsClientBuilder AddPipeline(string name, /*string audienceType,*/ string? category, TransportPriority transportPriority, MessagePriority messagePriority, Action<IPipelineChannelConfiguration> options) =>
			Pipeline.Add(name, /*audienceType, */category, transportPriority, messagePriority, options);

		/// <summary>
		/// Adds a pipeline to the configuration.
		/// </summary>
		/// <param name="name">The name of the pipeline.</param>
		/// <param name="category">The optional category of the pipeline.</param>
		/// <param name="options">The configuration options for the pipeline.</param>
		/// <returns>The configuration builder.</returns>
		public CommunicationsClientBuilder AddPipeline(string name, /*string audienceType, */string? category, Action<IPipelineChannelConfiguration> options) =>
			Pipeline.Add(name, /*audienceType, */category, options);

		/// <summary>
		/// Adds a pipeline to the configuration.
		/// </summary>
		/// <param name="name">The name of the pipeline.</param>
		/// <param name="options">The configuration options for the pipeline.</param>
		/// <returns>The configuration builder.</returns>
		public CommunicationsClientBuilder AddPipeline(string name, /*string audienceType, */Action<IPipelineChannelConfiguration> options) =>
			Pipeline.Add(name, /*audienceType, */options);

		/// <summary>
		/// Adds a pipeline to the configuration.
		/// </summary>
		/// <param name="name">The name of the pipeline.</param>
		/// <param name="transportPriority">The transport priority of the pipeline.</param>
		/// <param name="messagePriority">The message priority of the pipeline.</param>
		/// <param name="options">The configuration options for the pipeline.</param>
		/// <returns>The configuration builder.</returns>
		public CommunicationsClientBuilder AddPipeline(string name, /*string audienceType, */TransportPriority transportPriority, MessagePriority messagePriority, Action<IPipelineChannelConfiguration> options) =>
			Pipeline.Add(name, /*audienceType, */transportPriority, messagePriority, options);

		/// <summary>
		/// Adds a pipeline to the configuration by calling the provided module.
		/// </summary>
		/// <param name="module"><see cref="PipelineModule"/> to add.</param>
		/// <returns>The configuration builder.</returns>
		public CommunicationsClientBuilder AddPipelineModule(PipelineModule module) =>
			Pipeline.AddModule(module);

		// <summary>
		// Adds a generic audience resolver to the configuration.
		// </summary>
		// <param name="audienceResolver">The audience resolver function.</param>
		// <returns>The configuration builder.</returns>
		//public CommunicationsConfigurationBuilder AddGenericAudienceResolver(AudienceResolverFunc audienceResolver) =>
		//    AudienceResolver.AddGeneric(audienceResolver);

		// <summary>
		// Adds an audience resolver to the configuration.
		// </summary>
		// <returns>The configuration builder.</returns>
		//public CommunicationsConfigurationBuilder AddAudienceResolver(string audienceTypeIdentifier, AudienceResolverFunc audienceResolver) =>
		//    AudienceResolver.Add(audienceTypeIdentifier, audienceResolver);

		public CommunicationsClientBuilder RegisterClientFactory(ICommunicationClientFactory communicationClientFactory)
		{
			_clientFactory = communicationClientFactory;
			return this;
		}

		/// <summary>
		/// Adds a delivery report handler to the configuration.
		/// </summary>
		/// <param name="reportHandler">The event handler to register.</param>
		/// <param name="filterEventNames">List of events to listen to. See <see cref="DeliveryReport.Event"/></param>
		/// <param name="filterChannelIds">List of channel ids to listen to. See <see cref="Id.Channel"/></param>
		/// <param name="filterChannelProviderIds">List of channel provider ids to listen to. See <see cref="Id.ChannelProvider"/></param>
		/// <param name="filterPipelineNames">List of pipeline names to listen to.</param>
		/// <returns>The configuration builder</returns>
		public CommunicationsClientBuilder AddDeliveryReportHandler(IObserver<DeliveryReport> reportHandler, IReadOnlyCollection<string>? filterEventNames = null, IReadOnlyCollection<string>? filterChannelIds = null, IReadOnlyCollection<string>? filterChannelProviderIds = null, IReadOnlyCollection<string>? filterPipelineNames = null)
		{
			return DeliveryReport.AddDeliveryReportHandler(reportHandler, filterEventNames, filterChannelIds, filterChannelProviderIds, filterPipelineNames);
		}

		/// <summary>
		/// Adds a delivery report handler to the configuration.
		/// </summary>
		/// <param name="reportHandler">The event handler to register.</param>
		/// <param name="filterEventNames">List of events to listen to. See <see cref="DeliveryReport.Event"/></param>
		/// <param name="filterChannelIds">List of channel ids to listen to. See <see cref="Id.Channel"/></param>
		/// <param name="filterChannelProviderIds">List of channel provider ids to listen to. See <see cref="Id.ChannelProvider"/></param>
		/// <param name="filterPipelineNames">List of pipeline names to listen to.</param>
		/// <returns>The configuration builder</returns>
		public CommunicationsClientBuilder AddDeliveryReportHandler(DeliveryReportAsyncHandler reportHandler, IReadOnlyCollection<string>? filterEventNames = null, IReadOnlyCollection<string>? filterChannelIds = null, IReadOnlyCollection<string>? filterChannelProviderIds = null, IReadOnlyCollection<string>? filterPipelineNames = null)
		{
			return DeliveryReport.AddDeliveryReportHandler(reportHandler, filterEventNames, filterChannelIds, filterChannelProviderIds, filterPipelineNames);
		}
		
		public CommunicationsClientBuilder AddSenderVerificationSupport(Action<ISenderVerificationRegistration> configure)
		{
			return SenderVerification.Configure(configure);
		}

		/// <summary>
		/// Creates an instance of the <see cref="ICommunicationsClient"/>.
		/// </summary>
		/// <returns>The communications client.</returns>
		public ICommunicationsClient BuildClient()
		{
			if (_clientCreated) throw new InvalidOperationException($"{nameof(BuildClient)}() can only be called once.");

			if (_templateEngines.Count == 0)
				AddTemplateEngine(new NoopTemplatingEngine(), DefaultTemplateEngineId);

			IDeliveryReportReporter deliveryReportProvider = DeliveryReport.BuildHandler();
			var client = _clientFactory.CreateClient(
				new CreateCommunicationsClientContext(
					_channelProviders,
					_pipelines,
					_templateEngines,
					deliveryReportProvider
				)
			);

			_clientCreated = true;

			return client;
		}

		// <summary>
		// Sets whether to assert that the configuration is valid at runtime
		// </summary>
		// <returns>The configuration builder</returns>
		//public CommunicationsClientBuilder AssertConfiguration()
		//{
		//	_assertConfiguration = true;
		//	return this;
		//}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool Equals(object? obj)
		{
			return base.Equals(obj);
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string ToString()
		{
			return base.ToString() ?? string.Empty;
		}
	}
}