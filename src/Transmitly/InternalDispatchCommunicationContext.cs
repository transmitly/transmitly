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
	internal class InternalDispatchCommunicationContext(ITransactionModel? transactionModel,
		IPipelineConfiguration channelConfiguration,
		IReadOnlyCollection<IPlatformIdentityProfile> recipients,
		ITemplateEngine templateEngine,
		IDeliveryReportReporter deliveryReportManager,
		CultureInfo cultureInfo,
		string pipelineName,
		MessagePriority messagePriority = MessagePriority.Normal,
		TransportPriority transportPriority = TransportPriority.Normal) : IInternalDispatchCommunicationContext
	{
		public ITransactionModel? TransactionModel => transactionModel;

		public ITemplateEngine TemplateEngine { get; } = Guard.AgainstNull(templateEngine);

		public CultureInfo CultureInfo { get; set; } = GuardCulture.AgainstNull(cultureInfo);

		public IReadOnlyCollection<IPlatformIdentityProfile> PlatformIdentities { get; set; } = Guard.AgainstNull(recipients);

		public TransportPriority TransportPriority { get; set; } = transportPriority;

		public MessagePriority MessagePriority { get; set; } = messagePriority;

		public IPipelineConfiguration ChannelConfiguration { get; } = Guard.AgainstNull(channelConfiguration);

		public ICollection<IDispatchResult> DispatchResults { get; } = [];

		public IDeliveryReportReporter DeliveryReportManager { get; } = Guard.AgainstNull(deliveryReportManager);

		public string PipelineName { get; } = Guard.AgainstNullOrWhiteSpace(pipelineName);
	}
}