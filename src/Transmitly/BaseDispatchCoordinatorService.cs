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

using System.Globalization;
using Transmitly.Channel.Configuration;
using Transmitly.ChannelProvider;
using Transmitly.Delivery;
using Transmitly.Model.Configuration;
using Transmitly.Pipeline.Configuration;
using Transmitly.Template.Configuration;

namespace Transmitly;

public abstract class BaseDispatchCoordinatorService(
	IChannelChannelProviderService channelChannelProviderService,
	IPersonaService personaService,
	ITemplateEngineFactory templateEngineFactory,
	ITransactionModelEnricherService transactionModelEnricherService,
	IContentModelEnricherService contentModelEnricherService,
	IDeliveryReportService deliveryReportService) : IDispatchCoordinatorService
{
	private readonly IChannelChannelProviderService _channelChannelProviderService = channelChannelProviderService;
	private readonly IPersonaService _personaService = personaService;
	private readonly ITemplateEngineFactory _templateEngineFactory = templateEngineFactory;
	private readonly ITransactionModelEnricherService _transactionModelEnricherService = transactionModelEnricherService;
	private readonly IContentModelEnricherService _contentModelEnricherService = contentModelEnricherService;
	private readonly IDeliveryReportService _deliveryReportProvider = deliveryReportService;

	public virtual async Task<IReadOnlyCollection<RecipientDispatchCommunicationContext>> CreateRecipientContexts(
		IReadOnlyCollection<IPipeline> pipelines,
		IReadOnlyCollection<IPlatformIdentityProfile> platformIdentityProfiles,
		ITransactionModel transactionalModel,
		IReadOnlyCollection<string> dispatchChannelPreferences,
		CultureInfo cultureInfo)
	{
		var recipientContexts = new List<RecipientDispatchCommunicationContext>();
		var templateEngine = _templateEngineFactory.Get();
		var hasTransactionEnrichers = await _transactionModelEnricherService.HasEnrichersAsync().ConfigureAwait(false);
		var hasPerRecipientEnrichers = await _contentModelEnricherService.HasEnrichersAsync(ContentModelEnricherScope.PerRecipient).ConfigureAwait(false);

		foreach (var pipeline in pipelines)
		{
			var pipelineConfiguration = pipeline.Configuration;
			var filteredPlatformIdentities = await _personaService.FilterPlatformIdentityPersonasAsync(platformIdentityProfiles, pipelineConfiguration.PersonaFilters).ConfigureAwait(false);
			var pipelineTransactionModel = transactionalModel;

			if (hasTransactionEnrichers)
			{
				var transactionContext = new DispatchCommunicationContext(
					null,
					pipelineConfiguration,
					filteredPlatformIdentities,
					templateEngine,
					_deliveryReportProvider,
					cultureInfo,
					pipeline.Intent,
					pipeline.Id);

				pipelineTransactionModel = await _transactionModelEnricherService.EnrichAsync(
					transactionContext,
					pipelineTransactionModel).ConfigureAwait(false);
			}

			foreach (var platformIdentity in filteredPlatformIdentities)
			{
				var context = new InternalDispatchCommunicationContext(
					pipelineTransactionModel,
					pipelineConfiguration,
					new[] { platformIdentity },
					templateEngine,
					_deliveryReportProvider,
					cultureInfo,
					pipeline.Intent,
					pipeline.Id,
					pipelineConfiguration.PipelineDeliveryStrategyProvider,
					_contentModelEnricherService);

				if (hasPerRecipientEnrichers)
				{
					var baseContentModel = new ContentModel(pipelineTransactionModel, new[] { platformIdentity });
					var enricherContext = new DispatchCommunicationContext(
						baseContentModel,
						pipelineConfiguration,
						new[] { platformIdentity },
						templateEngine,
						_deliveryReportProvider,
						cultureInfo,
						pipeline.Intent,
						pipeline.Id);

					context.ContentModel = await _contentModelEnricherService.EnrichAsync(
						enricherContext,
						baseContentModel,
						ContentModelEnricherScope.PerRecipient).ConfigureAwait(false);
				}

				var orderedChannels = SetPipelineChannelOrderPreference(dispatchChannelPreferences, pipelineConfiguration);

				var group = await _channelChannelProviderService.CreateGroupingsForPlatformIdentityAsync(
					pipeline.Category,
					orderedChannels,
					dispatchChannelPreferences,
					platformIdentity).ConfigureAwait(false);

				if (group.Count > 0)
				{
					recipientContexts.Add(new RecipientDispatchCommunicationContext(context, group));
				}
			}
		}

		return recipientContexts.AsReadOnly();
	}

	private static IReadOnlyCollection<IChannel> SetPipelineChannelOrderPreference(IReadOnlyCollection<string> dispatchChannelPreferences, IPipelineConfiguration pipelineConfiguration)
	{
		var orderedChannels = pipelineConfiguration.Channels;

		if (pipelineConfiguration.IsDispatchChannelPriorityPreferenceAllowed)
		{
			//order the pipeline channels by the order of the provided dispatchChannelPreferences
			var preferenceIndexLookup = dispatchChannelPreferences
				.Select((id, index) => new { id, index })
				.ToDictionary(x => x.id, x => x.index, StringComparer.OrdinalIgnoreCase);

			orderedChannels = pipelineConfiguration.Channels
				.OrderBy(c => preferenceIndexLookup.TryGetValue(c.Id, out var index) ? index : int.MaxValue)
				.ToList()
				.AsReadOnly();
		}
		return orderedChannels;
	}
}
