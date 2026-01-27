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

using Transmitly.Exceptions;

namespace Transmitly.Model.Configuration;

public sealed class DefaultModelResolverService(IModelResolverFactory resolverFactory) : IModelResolverService
{
	private readonly IModelResolverFactory _resolverFactory = Guard.AgainstNull(resolverFactory);

	public async Task<bool> HasResolversAsync(ModelResolverScope scope)
	{
		var allResolvers = await _resolverFactory.GetAllResolversAsync().ConfigureAwait(false);
		return allResolvers.Any(r => r.Scope == scope);
	}

	public async Task<IContentModel> ResolveAsync(
		IDispatchCommunicationContext context,
		IContentModel contentModel,
		ModelResolverScope scope,
		CancellationToken cancellationToken = default)
	{
		Guard.AgainstNull(context);
		Guard.AgainstNull(contentModel);

		var allResolvers = await _resolverFactory.GetAllResolversAsync().ConfigureAwait(false);

		var orderedResolvers = allResolvers
			.Select((registration, index) => new { registration, index })
			.Where(x => x.registration.Scope == scope)
			.OrderBy(x => x.registration.Order ?? int.MaxValue)
			.ThenBy(x => x.index)
			.Select(x => x.registration)
			.ToList();

		var currentModel = contentModel;
		foreach (var registration in orderedResolvers)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (registration.Predicate != null && !registration.Predicate(context))
				continue;

			var resolverInstance = await _resolverFactory.GetResolver(registration).ConfigureAwait(false)
				?? throw new CommunicationsException("Unable to get an instance of model resolver");

			var resolvedModel = await resolverInstance.ResolveAsync(context, currentModel, cancellationToken).ConfigureAwait(false);
			if (resolvedModel != null)
			{
				currentModel = resolvedModel;
				if (!registration.ContinueOnResolvedModel)
					break;
			}
		}

		return currentModel;
	}
}
