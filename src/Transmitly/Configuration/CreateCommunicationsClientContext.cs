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


using Transmitly.ChannelProvider.Configuration;
using Transmitly.Pipeline.Configuration;
using Transmitly.Template.Configuration;
using Transmitly.Delivery;

namespace Transmitly
{
	internal sealed class CreateCommunicationsClientContext(
		IReadOnlyCollection<IChannelProviderRegistration> channelProviders,
		IReadOnlyCollection<IPipeline> pipelines,
		IReadOnlyCollection<ITemplateEngineRegistration> templateEngines,
		IDeliveryReportReporter deliveryReportProvider
	) : ICreateCommunicationsClientContext
	{
		public IReadOnlyCollection<IChannelProviderRegistration> ChannelProviders { get; } = channelProviders;

		public IReadOnlyCollection<IPipeline> Pipelines { get; } = pipelines;

		public IReadOnlyCollection<ITemplateEngineRegistration> TemplateEngines { get; } = templateEngines;

		public IDeliveryReportReporter DeliveryReportProvider { get; } = deliveryReportProvider;
	}
}