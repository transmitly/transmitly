// Copyright (c) Code Impressions, LLC. All Rights Reserved.
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
/// Represents a builder for configuring transaction model enrichers in the communications configuration.
/// </summary>
public sealed class TransactionModelEnricherConfigurationBuilder
{
	private readonly CommunicationsClientBuilder _communicationsConfiguration;
	private readonly Action<ITransactionModelEnricherRegistration> _addTransactionModelEnricher;

	/// <summary>
	/// Initializes a new instance of the <see cref="TransactionModelEnricherConfigurationBuilder"/> class.
	/// </summary>
	/// <param name="communicationsConfiguration">The communications configuration builder.</param>
	/// <param name="addTransactionModelEnricher">The action to add a transaction model enricher.</param>
	internal TransactionModelEnricherConfigurationBuilder(CommunicationsClientBuilder communicationsConfiguration, Action<ITransactionModelEnricherRegistration> addTransactionModelEnricher)
	{
		_communicationsConfiguration = Guard.AgainstNull(communicationsConfiguration);
		_addTransactionModelEnricher = Guard.AgainstNull(addTransactionModelEnricher);
	}

	/// <summary>
	/// Adds a transaction model enricher to the communications configuration.
	/// </summary>
	/// <typeparam name="TEnricher">Concrete transaction model enricher type.</typeparam>
	/// <param name="options">Optional configuration for the registration.</param>
	/// <returns>The communications configuration builder.</returns>
	public CommunicationsClientBuilder Add<TEnricher>(Action<TransactionModelEnricherRegistrationOptions>? options = null)
		where TEnricher : ITransactionModelEnricher
	{
		var registrationOptions = new TransactionModelEnricherRegistrationOptions();
		options?.Invoke(registrationOptions);

		_addTransactionModelEnricher(new TransactionModelEnricherRegistration(
			typeof(TEnricher),
			registrationOptions.ContinueOnEnrichedModel,
			registrationOptions.Predicate,
			registrationOptions.Order));

		return _communicationsConfiguration;
	}
}
