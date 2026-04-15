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

public sealed class DefaultContentModelEnricherService : IContentModelEnricherService
{
	private readonly IContentModelEnricherFactory _enricherFactory;
	private readonly ILogger _logger;

	public DefaultContentModelEnricherService(IContentModelEnricherFactory enricherFactory, ILoggerFactory loggerFactory)
		: this(enricherFactory, Guard.AgainstNull(loggerFactory).CreateLogger<DefaultContentModelEnricherService>())
	{
	}

	internal DefaultContentModelEnricherService(IContentModelEnricherFactory enricherFactory, ILogger logger)
	{
		_enricherFactory = Guard.AgainstNull(enricherFactory);
		_logger = Guard.AgainstNull(logger);
	}

	public async Task<bool> HasEnrichersAsync(ContentModelEnricherScope scope)
	{
		var allEnrichers = await _enricherFactory.GetAllEnrichersAsync().ConfigureAwait(false);
		return allEnrichers.Any(r => r.Scope == scope);
	}

	public async Task<IContentModel> EnrichAsync(
		IDispatchCommunicationContext context,
		IContentModel contentModel,
		ContentModelEnricherScope scope,
		CancellationToken cancellationToken = default)
	{
		Guard.AgainstNull(context);
		Guard.AgainstNull(contentModel);

		var allEnrichers = await _enricherFactory.GetAllEnrichersAsync().ConfigureAwait(false);

		var orderedEnrichers = allEnrichers
			.Select((registration, index) => new { registration, index })
			.Where(x => x.registration.Scope == scope)
			.OrderBy(x => x.registration.Order ?? int.MaxValue)
			.ThenBy(x => x.index)
			.Select(x => x.registration)
			.ToList();

		var currentModel = contentModel;
		foreach (var registration in orderedEnrichers)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (registration.Predicate != null && !registration.Predicate(context))
				continue;

			var enricherInstance = await _enricherFactory.GetEnricher(registration).ConfigureAwait(false)
				?? throw new CommunicationsException("Unable to get an instance of content model enricher");
			_logger.LogDebug(
				LogEvents.ContentEnrichmentStarted,
				"Running content model enricher.",
				(Scope: scope.ToString(), EnricherType: registration.EnricherType.FullName ?? registration.EnricherType.Name),
				static state => new Dictionary<string, object?>
				{
					["scope"] = state.Scope,
					["enricherType"] = state.EnricherType
				});

			var enrichedModel = await enricherInstance.EnrichAsync(context, currentModel, cancellationToken).ConfigureAwait(false);
			if (enrichedModel != null)
			{
				currentModel = PreserveProtectedProperties(context, currentModel, enrichedModel);
				_logger.LogDebug(
					LogEvents.ContentEnrichmentCompleted,
					"Content model enricher produced an updated model.",
					(Scope: scope.ToString(), EnricherType: registration.EnricherType.FullName ?? registration.EnricherType.Name),
					static state => new Dictionary<string, object?>
					{
						["scope"] = state.Scope,
						["enricherType"] = state.EnricherType
					});
				if (!registration.ContinueOnEnrichedModel)
					break;
			}
		}

		return currentModel;
	}

	private static IContentModel PreserveProtectedProperties(IDispatchCommunicationContext context, IContentModel source, IContentModel target)
	{
		var normalizedSource = NormalizeModelForProtection(source, context.PlatformIdentities);
		var normalizedTarget = NormalizeModelForProtection(target, context.PlatformIdentities);

		if (normalizedSource.Model is DynamicContentModel sourceDynamic &&
			normalizedTarget.Model is DynamicContentModel targetDynamic)
		{
			targetDynamic.CopyProtectedPropertiesFrom(sourceDynamic);
		}

		return normalizedTarget;
	}

	private static IContentModel NormalizeModelForProtection(IContentModel model, IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities)
	{
		if (model.Model is DynamicContentModel)
			return model;

		return new ContentModel(model, platformIdentities);
	}
}
