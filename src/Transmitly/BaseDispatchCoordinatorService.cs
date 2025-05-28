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
using Transmitly.Pipeline.Configuration;
using Transmitly.Template.Configuration;

namespace Transmitly;

public abstract class BaseDispatchCoordinatorService(
	IChannelChannelProviderService channelChannelProviderService,
	IPersonaService personaService,
	ITemplateEngineFactory templateEngineFactory,
	IDeliveryReportService deliveryReportService) : IDispatchCoordinatorService
{
	private readonly IChannelChannelProviderService _channelChannelProviderService = channelChannelProviderService;
	private readonly IPersonaService _personaService = personaService;
	private readonly ITemplateEngineFactory _templateEngineFactory = templateEngineFactory;
	private readonly IDeliveryReportService _deliveryReportProvider = deliveryReportService;

	public virtual async Task<IReadOnlyCollection<RecipientDispatchCommunicationContext>> CreateRecipientContexts(
		IReadOnlyCollection<IPipeline> pipelines,
		IReadOnlyCollection<IPlatformIdentityProfile> platformIdentityProfiles,
		ITransactionModel transactionalModel,
		IReadOnlyCollection<string> dispatchChannelPreferences
		)
	{
		var recipientContexts = new List<RecipientDispatchCommunicationContext>();
		var templateEngine = _templateEngineFactory.Get();

		foreach (var pipeline in pipelines)
		{
			var pipelineConfiguration = pipeline.Configuration;
			var filteredPlatformIdentities = await _personaService.FilterPlatformIdentityPersonasAsync(platformIdentityProfiles, pipelineConfiguration.PersonaFilters).ConfigureAwait(false);

			foreach (var platformIdentity in filteredPlatformIdentities)
			{
				var context = new InternalDispatchCommunicationContext(
					transactionalModel,
					pipelineConfiguration,
					new[] { platformIdentity },
					templateEngine,
					_deliveryReportProvider,
					CultureInfo.InvariantCulture,//todo
					pipeline.Intent,
					pipelineConfiguration.PipelineDeliveryStrategyProvider);

				var group = await _channelChannelProviderService.CreateGroupingsForPlatformIdentityAsync(
					pipeline.Category,
					pipelineConfiguration.Channels,
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
}