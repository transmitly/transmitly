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

using System.Linq.Expressions;
using Transmitly.ChannelProvider.Configuration;
using Transmitly.Delivery;
using Transmitly.Delivery.Configuration;
using Transmitly.Persona.Configuration;
using Transmitly.Pipeline.Configuration;
using Transmitly.PlatformIdentity.Configuration;
using Transmitly.Template.Configuration;

namespace Transmitly;

/// <summary>
/// Builds an instance of a <see cref="ICommunicationsClient"/>.
/// </summary>
public sealed class CommunicationsClientBuilder
{
	private const string DefaultTemplateEngineId = "Default";
	private bool _clientCreated;
	private ICommunicationClientFactory _clientFactory = new DefaultCommunicationClientFactory();
	private readonly List<IChannelProviderRegistration> _channelProviders = [];
	private readonly List<IPipeline> _pipelines = [];
	private readonly List<IPlatformIdentityResolverRegistration> _platformIdentityResolvers = [];
	private readonly List<ITemplateEngineRegistration> _templateEngines = [];
	private readonly List<IPersonaRegistration> _personaRegistrations = [];
	private readonly List<IObserver<DeliveryReport>> _deliveryReportObservers = [];

	/// <summary>
	/// Creates an instance of the class
	/// </summary>
	public CommunicationsClientBuilder()
	{
		ChannelProvider = new(this, _channelProviders.Add);
		Pipeline = new(this, _pipelines.Add);
		PlatformIdentityResolver = new(this, _platformIdentityResolvers.Add);
		TemplateEngine = new(this, _templateEngines.Add);
		DeliveryReport = new(this, _deliveryReportObservers.Add);
		Persona = new(this, _personaRegistrations.Add);
	}

	/// <summary>
	/// Gets the channel provider configuration builder.
	/// </summary>
	public ChannelProviderConfigurationBuilder ChannelProvider { get; }

	/// <summary>
	/// Gets the pipeline configuration builder.
	/// </summary>
	public PipelineConfigurationBuilder Pipeline { get; }

	/// <summary>
	/// Gets the platform identity resolver configuration builder.
	/// </summary>
	public PlatformIdentityResolverConfigurationBuilder PlatformIdentityResolver { get; }

	/// <summary>
	/// Gets the template engine configuration builder.
	/// </summary>
	public TemplateConfigurationBuilder TemplateEngine { get; }

	/// <summary>
	/// Gets the delivery report configuration builder.
	/// </summary>
	public DeliveryReportConfigurationBuilder DeliveryReport { get; }

	/// <summary>
	/// Gets the persona configuration builder.
	/// </summary>
	public PersonaConfigurationBuilder Persona { get; }

	/// <summary>
	/// Adds a template engine to the configuration.
	/// </summary>
	/// <param name="engine">The instance of the template engine.</param>
	/// <param name="id">The optional Id of the template engine.</param>
	/// <returns>The configuration builder.</returns>
	public CommunicationsClientBuilder AddTemplateEngine(ITemplateEngine engine, string? id = null) =>
		TemplateEngine.Add(engine, id);

	/// <summary>
	/// Adds a pipeline to the configuration.
	/// </summary>
	/// <param name="name">The name of the pipeline.</param>
	/// <param name="category">The optional category of the pipeline.</param>
	/// <param name="options">The configuration options for the pipeline.</param>
	/// <returns>The configuration builder.</returns>
	public CommunicationsClientBuilder AddPipeline(string name, string? category, Action<IPipelineConfiguration> options) =>
		Pipeline.Add(name, category, options);

	/// <summary>
	/// Adds a pipeline configurator to the configuration.
	/// </summary>
	/// <param name="configurator">The configurator to add.</param>
	/// <returns>The configuration builder.</returns>
	public CommunicationsClientBuilder AddPipelineConfigurator(IPipelineConfigurator configurator) =>
		Pipeline.AddConfigurator(configurator);

	/// <summary>
	/// Registers a custom client factory.
	/// </summary>
	/// <param name="communicationClientFactory">Factory to register</param>
	/// <returns>The configuration builder.</returns>
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
	/// <param name="filterPipelineIntents">List of pipeline intents to listen to.</param>
	/// <returns>The configuration builder</returns>
	public CommunicationsClientBuilder AddDeliveryReportHandler(IObserver<DeliveryReport> reportHandler, IReadOnlyCollection<string>? filterEventNames = null, IReadOnlyCollection<string>? filterChannelIds = null, IReadOnlyCollection<string>? filterChannelProviderIds = null, IReadOnlyCollection<string>? filterPipelineIntents = null) =>
		DeliveryReport.AddDeliveryReportHandler(reportHandler, filterEventNames, filterChannelIds, filterChannelProviderIds, filterPipelineIntents);

	/// <summary>
	/// Adds a platform identity resolver to the configuration.
	/// </summary>
	/// <typeparam name="TResolver">Platform identity resolver to register.</typeparam>
	/// <param name="platformIdentityType">Limit this resolver to only resolve platform identities to the provided type.</param>
	/// <returns>The configuration builder.</returns>
	public CommunicationsClientBuilder AddPlatformIdentityResolver<TResolver>(string? platformIdentityType = null)
		where TResolver : IPlatformIdentityResolver =>
		PlatformIdentityResolver.Add<TResolver>(platformIdentityType);

	/// <summary>
	/// Add a persona filter to the configuration.
	/// </summary>
	/// <typeparam name="TPersona">Concrete persona type.</typeparam>
	/// <param name="name">Persona filter name.</param>
	/// <param name="platformIdentityType">Platform Identity type name.</param>
	/// <param name="personaCondition">Conditions that the <typeparamref name="TPersona"/> must meet.</param>
	/// <returns>The configuration builder.</returns>
	public CommunicationsClientBuilder AddPersona<TPersona>(string name, string platformIdentityType, Expression<Func<TPersona, bool>> personaCondition)
		where TPersona : class =>
		Persona.Add(name, platformIdentityType, personaCondition);

	/// <summary>
	/// Creates an instance of the <see cref="ICommunicationsClient"/>.
	/// </summary>
	/// <returns>The communications client.</returns>
	public ICommunicationsClient BuildClient()
	{
		if (_clientCreated)
			throw new InvalidOperationException($"{nameof(BuildClient)}() can only be called once.");

		if (_templateEngines.Count == 0)
			AddTemplateEngine(new NoopTemplatingEngine(), DefaultTemplateEngineId);

		var client = _clientFactory.CreateClient(
			new CreateCommunicationsClientContext(
				_channelProviders,
				_pipelines,
				_templateEngines,
				_platformIdentityResolvers,
				_personaRegistrations,
				_deliveryReportObservers
			)
		);

		_clientCreated = true;

		return client;
	}
}