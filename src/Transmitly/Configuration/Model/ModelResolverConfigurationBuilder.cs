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
/// Represents a builder for configuring model resolvers in the communications configuration.
/// </summary>
public sealed class ModelResolverConfigurationBuilder
{
	private readonly CommunicationsClientBuilder _communicationsConfiguration;
	private readonly Action<IModelResolverRegistration> _addModelResolver;

	/// <summary>
	/// Initializes a new instance of the <see cref="ModelResolverConfigurationBuilder"/> class.
	/// </summary>
	/// <param name="communicationsConfiguration">The communications configuration builder.</param>
	/// <param name="addModelResolver">The action to add a model resolver.</param>
	internal ModelResolverConfigurationBuilder(CommunicationsClientBuilder communicationsConfiguration, Action<IModelResolverRegistration> addModelResolver)
	{
		_communicationsConfiguration = Guard.AgainstNull(communicationsConfiguration);
		_addModelResolver = Guard.AgainstNull(addModelResolver);
	}

	/// <summary>
	/// Adds a model resolver to the communications configuration.
	/// </summary>
	/// <typeparam name="TResolver">Concrete model resolver type.</typeparam>
	/// <param name="options">Optional configuration for the registration.</param>
	/// <returns>The communications configuration builder.</returns>
	public CommunicationsClientBuilder Add<TResolver>(Action<ModelResolverRegistrationOptions>? options = null)
		where TResolver : IModelResolver
	{
		var registrationOptions = new ModelResolverRegistrationOptions();
		options?.Invoke(registrationOptions);

		_addModelResolver(new ModelResolverRegistration(
			typeof(TResolver),
			registrationOptions.Scope,
			registrationOptions.ContinueOnResolvedModel,
			registrationOptions.Predicate,
			registrationOptions.Order));

		return _communicationsConfiguration;
	}
}
