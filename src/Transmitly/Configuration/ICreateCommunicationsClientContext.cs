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

using Transmitly.ChannelProvider.Configuration;
using Transmitly.Delivery;
using Transmitly.Model.Configuration;
using Transmitly.Persona.Configuration;
using Transmitly.Pipeline.Configuration;
using Transmitly.PlatformIdentity.Configuration;
using Transmitly.Template.Configuration;

namespace Transmitly;

/// <summary>
/// Defines the context provided to communications client factories for registering channel providers, pipelines,
/// template engines, observers, identity resolvers, enrichers, personas, and logging facilities.
/// </summary>
/// <remarks>Implementations of this interface supply collections for registering and accessing components
/// required to configure and extend communications clients. This context is typically used during client initialization
/// to compose the necessary services and behaviors.</remarks>
public interface ICreateCommunicationsClientContext
{
	/// <summary>
	/// Gets the collection of channel provider registrations.
	/// </summary>
	IReadOnlyCollection<IChannelProviderRegistration> ChannelProviders { get; }
	/// <summary>
	/// Gets the collection of pipelines configured for this instance.
	/// </summary>
	IReadOnlyCollection<IPipeline> Pipelines { get; }
	/// <summary>
	/// Gets the collection of template engine registrations.
	/// </summary>
	IReadOnlyCollection<ITemplateEngineRegistration> TemplateEngines { get; }
	/// <summary>
	/// Gets a read-only collection of observers that receive delivery report notifications.
	/// </summary>
	/// <remarks>Observers in this collection are notified when a delivery report is generated. The collection is
	/// read-only and reflects the current set of subscribed observers.</remarks>
	IReadOnlyCollection<IObserver<DeliveryReport>> DeliveryReportObservers { get; }
	/// <summary>
	/// Gets the collection of platform identity resolver registrations.
	/// </summary>
	IReadOnlyCollection<IPlatformIdentityResolverRegistration> PlatformIdentityResolvers { get; }
	/// <summary>
	/// Gets the collection of content model enricher registrations.
	/// </summary>
	IReadOnlyCollection<IContentModelEnricherRegistration> ContentModelEnrichers { get; }
	/// <summary>
	/// Gets the collection of registered platform identity profile enrichers.
	/// </summary>
	/// <remarks>Use this collection to access all enrichers that contribute additional profile information for
	/// platform identities. The collection is read-only and reflects the current set of registered enrichers.</remarks>
	IReadOnlyCollection<IPlatformIdentityProfileEnricherRegistration> PlatformIdentityProfileEnrichers { get; }
	/// <summary>
	/// Gets the collection of persona registrations. 
	/// </summary>
	IReadOnlyCollection<IPersonaRegistration> Personas { get; }
	/// <summary>
	/// Gets the logger factory used to create logger instances for logging application events and errors.
	/// </summary>
	/// <remarks>The logger factory provides a central point for configuring and obtaining loggers throughout the
	/// application. Use this property to create loggers for custom components or to integrate with the application's
	/// logging infrastructure.</remarks>
	ILoggerFactory LoggerFactory { get; }
}
