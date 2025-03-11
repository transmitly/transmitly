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
using Transmitly.Delivery;
using Transmitly.Pipeline.Configuration;
using Transmitly.Template.Configuration;

namespace Transmitly.Channel.Configuration
{
	///<inheritdoc cref="IDispatchCommunicationContext"/>
	internal class DispatchCommunicationContext(
		IContentModel? contentModel,
		IPipelineConfiguration channelConfiguration,
		IReadOnlyCollection<IPlatformIdentityProfile> recipients,
		ITemplateEngine templateEngine,
		IDeliveryReportReporter deliveryReportManager,
		CultureInfo cultureInfo,
		string pipelineName,
		MessagePriority messagePriority = MessagePriority.Normal,
		TransportPriority transportPriority = TransportPriority.Normal,
		string? ChannelId = null, string? ChannelProviderId = null) : IDispatchCommunicationContext
	{
		public DispatchCommunicationContext(IDispatchCommunicationContext context, IChannel channel, IChannelProvider channelProvider)
			: this(context.ContentModel, context.ChannelConfiguration, context.PlatformIdentities, context.TemplateEngine, context.DeliveryReportManager,
				  context.CultureInfo, context.PipelineName, context.MessagePriority, context.TransportPriority, channel.Id, channelProvider.Id)
		{

		}

		public ITemplateEngine TemplateEngine { get; } = Guard.AgainstNull(templateEngine);

		public CultureInfo CultureInfo { get; set; } = GuardCulture.AgainstNull(cultureInfo);

		public IReadOnlyCollection<IPlatformIdentityProfile> PlatformIdentities { get; set; } = Guard.AgainstNull(recipients);

		public TransportPriority TransportPriority { get; set; } = transportPriority;

		public MessagePriority MessagePriority { get; set; } = messagePriority;

		public string? ChannelId { get; set; } = ChannelId;

		public string? ChannelProviderId { get; set; } = ChannelProviderId;

		public IPipelineConfiguration ChannelConfiguration { get; } = Guard.AgainstNull(channelConfiguration);

		public ICollection<IDispatchResult> DispatchResults { get; } = [];

		public IDeliveryReportReporter DeliveryReportManager { get; } = Guard.AgainstNull(deliveryReportManager);

		public string PipelineName { get; } = Guard.AgainstNullOrWhiteSpace(pipelineName);

		public IContentModel? ContentModel { get; set; } = contentModel;
	}
}