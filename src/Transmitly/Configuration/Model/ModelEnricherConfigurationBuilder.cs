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
/// Represents a builder for configuring model enrichers in the communications configuration.
/// </summary>
public sealed class ModelEnricherConfigurationBuilder
{
	private readonly CommunicationsClientBuilder _communicationsConfiguration;
	private readonly Action<IModelEnricherRegistration> _addModelEnricher;

	/// <summary>
	/// Initializes a new instance of the <see cref="ModelEnricherConfigurationBuilder"/> class.
	/// </summary>
	/// <param name="communicationsConfiguration">The communications configuration builder.</param>
	/// <param name="addModelEnricher">The action to add a model enricher.</param>
	internal ModelEnricherConfigurationBuilder(CommunicationsClientBuilder communicationsConfiguration, Action<IModelEnricherRegistration> addModelEnricher)
	{
		_communicationsConfiguration = Guard.AgainstNull(communicationsConfiguration);
		_addModelEnricher = Guard.AgainstNull(addModelEnricher);
	}

	/// <summary>
	/// Adds a model enricher to the communications configuration.
	/// </summary>
	/// <typeparam name="TEnricher">Concrete model enricher type.</typeparam>
	/// <param name="options">Optional configuration for the registration.</param>
	/// <returns>The communications configuration builder.</returns>
	public CommunicationsClientBuilder Add<TEnricher>(Action<ModelEnricherRegistrationOptions>? options = null)
		where TEnricher : IModelEnricher
	{
		var registrationOptions = new ModelEnricherRegistrationOptions();
		options?.Invoke(registrationOptions);

		_addModelEnricher(new ModelEnricherRegistration(
			typeof(TEnricher),
			registrationOptions.Scope,
			registrationOptions.ContinueOnEnrichedModel,
			registrationOptions.Predicate,
			registrationOptions.Order));

		return _communicationsConfiguration;
	}
}
