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
using Transmitly.ChannelProvider;
using Transmitly.Pipeline.Configuration;
using Transmitly.Settings.Configuration;
using Transmitly.Template.Configuration;

namespace Transmitly.Channel.Configuration
{
	///<inheritdoc cref="IDispatchCommunicationContext"/>
	internal class DispatchCommunicationContext(
		IContentModel? contentModel,
		IPipelineChannelConfiguration channelConfiguration,
		IReadOnlyCollection<IAudience> recipients,
		ITemplateEngine templateEngine,
		IDeliveryReportReporter deliveryReportManager,
		ICommunicationsConfigurationSettings configurationSettings,
		CultureInfo cultureInfo,
		string pipelineName,
		MessagePriority messagePriority = MessagePriority.Normal,
		TransportPriority transportPriority = TransportPriority.Normal,
		string? ChannelId = null, string? ChannelProviderId = null) : IDispatchCommunicationContext
	{
		public DispatchCommunicationContext(IDispatchCommunicationContext context, IChannel channel, IChannelProvider channelProvider)
			: this(context.ContentModel, context.ChannelConfiguration, context.RecipientAudiences, context.TemplateEngine, context.DeliveryReportManager,
				  context.Settings, context.CultureInfo, context.PipelineName, context.MessagePriority, context.TransportPriority, channel.Id, channelProvider.Id)
		{

		}

		/// <inheritdoc />
		public ICommunicationsConfigurationSettings Settings { get; } = Guard.AgainstNull(configurationSettings);

		/// <inheritdoc/>
		public ITemplateEngine TemplateEngine { get; } = Guard.AgainstNull(templateEngine);

		/// <inheritdoc/>
		public CultureInfo CultureInfo { get; set; } = GuardCulture.AgainstNull(cultureInfo);

		/// <inheritdoc/>
		public IReadOnlyCollection<IAudience> RecipientAudiences { get; set; } = Guard.AgainstNull(recipients);

		/// <inheritdoc/>
		public TransportPriority TransportPriority { get; set; } = transportPriority;

		/// <inheritdoc/>
		public MessagePriority MessagePriority { get; set; } = messagePriority;

		/// <inheritdoc/>
		public string? ChannelId { get; set; } = ChannelId;

		/// <inheritdoc/>
		public string? ChannelProviderId { get; set; } = ChannelProviderId;

		/// <inheritdoc/>
		public IPipelineChannelConfiguration ChannelConfiguration { get; } = Guard.AgainstNull(channelConfiguration);

		/// <inheritdoc/>
		public IContentModel? ContentModel { get; set; } = contentModel;

		/// <inheritdoc/>
		public ICollection<IDispatchResult> DispatchResults { get; } = [];

		/// <inheritdoc/>
		public IDeliveryReportReporter DeliveryReportManager { get; } = Guard.AgainstNull(deliveryReportManager);

		/// <inheritdoc/>
		public string PipelineName { get; } = Guard.AgainstNullOrWhiteSpace(pipelineName);
	}
}