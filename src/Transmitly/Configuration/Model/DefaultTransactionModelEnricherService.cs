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

using Transmitly.Exceptions;

namespace Transmitly.Model.Configuration;

public sealed class DefaultTransactionModelEnricherService(ITransactionModelEnricherFactory enricherFactory) : ITransactionModelEnricherService
{
	private readonly ITransactionModelEnricherFactory _enricherFactory = Guard.AgainstNull(enricherFactory);

	public async Task<bool> HasEnrichersAsync()
	{
		var allEnrichers = await _enricherFactory.GetAllEnrichersAsync().ConfigureAwait(false);
		return allEnrichers.Count > 0;
	}

	public async Task<ITransactionModel> EnrichAsync(
		IDispatchCommunicationContext context,
		ITransactionModel transactionModel,
		CancellationToken cancellationToken = default)
	{
		Guard.AgainstNull(context);
		Guard.AgainstNull(transactionModel);

		var allEnrichers = await _enricherFactory.GetAllEnrichersAsync().ConfigureAwait(false);

		var orderedEnrichers = allEnrichers
			.Select((registration, index) => new { registration, index })
			.OrderBy(x => x.registration.Order ?? int.MaxValue)
			.ThenBy(x => x.index)
			.Select(x => x.registration)
			.ToList();

		var currentModel = transactionModel;
		foreach (var registration in orderedEnrichers)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (registration.Predicate != null && !registration.Predicate(context))
				continue;

			var enricherInstance = await _enricherFactory.GetEnricher(registration).ConfigureAwait(false)
				?? throw new CommunicationsException("Unable to get an instance of transaction model enricher");

			var enrichedModel = await enricherInstance.EnrichAsync(context, currentModel, cancellationToken).ConfigureAwait(false);
			if (enrichedModel != null)
			{
				currentModel = enrichedModel;
				if (!registration.ContinueOnEnrichedModel)
					break;
			}
		}

		return currentModel;
	}
}
