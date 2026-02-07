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

namespace Transmitly.Model.Configuration;

/// <summary>
/// Represents a builder for configuring content model enrichers in the communications configuration.
/// </summary>
public sealed class ContentModelEnricherConfigurationBuilder
{
	private readonly CommunicationsClientBuilder _communicationsConfiguration;
	private readonly Action<IContentModelEnricherRegistration> _addContentModelEnricher;

	/// <summary>
	/// Initializes a new instance of the <see cref="ContentModelEnricherConfigurationBuilder"/> class.
	/// </summary>
	/// <param name="communicationsConfiguration">The communications configuration builder.</param>
	/// <param name="addContentModelEnricher">The action to add a content model enricher.</param>
	internal ContentModelEnricherConfigurationBuilder(CommunicationsClientBuilder communicationsConfiguration, Action<IContentModelEnricherRegistration> addContentModelEnricher)
	{
		_communicationsConfiguration = Guard.AgainstNull(communicationsConfiguration);
		_addContentModelEnricher = Guard.AgainstNull(addContentModelEnricher);
	}

	/// <summary>
	/// Adds a content model enricher to the communications configuration.
	/// </summary>
	/// <typeparam name="TEnricher">Concrete content model enricher type.</typeparam>
	/// <param name="options">Optional configuration for the registration.</param>
	/// <returns>The communications configuration builder.</returns>
	public CommunicationsClientBuilder Add<TEnricher>(Action<ContentModelEnricherRegistrationOptions>? options = null)
		where TEnricher : IContentModelEnricher
	{
		var registrationOptions = new ContentModelEnricherRegistrationOptions();
		options?.Invoke(registrationOptions);

		_addContentModelEnricher(new ContentModelEnricherRegistration(
			typeof(TEnricher),
			registrationOptions.Scope,
			registrationOptions.ContinueOnEnrichedModel,
			registrationOptions.Predicate,
			registrationOptions.Order));

		return _communicationsConfiguration;
	}
}
