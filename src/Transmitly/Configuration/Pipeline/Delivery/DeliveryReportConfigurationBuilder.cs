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

namespace Transmitly.Delivery.Configuration
{
	public sealed class DeliveryReportConfigurationBuilder
	{
		private readonly CommunicationsClientBuilder _communicationsClientBuilder;
		private readonly List<IObserver<DeliveryReport>> _observerRegistrations = [];

		internal DeliveryReportConfigurationBuilder(CommunicationsClientBuilder communicationsClientBuilder)
		{
			_communicationsClientBuilder = Guard.AgainstNull(communicationsClientBuilder);
		}

		public CommunicationsClientBuilder AddDeliveryReportHandler(IObserver<DeliveryReport> reportHandler, IReadOnlyCollection<string>? filterEventNames = null, IReadOnlyCollection<string>? channelIds = null, IReadOnlyCollection<string>? channelProviderIds = null, IReadOnlyCollection<string>? filterCommunicationIntentIds = null)
		{
			_observerRegistrations.Add(new DeliveryReportMonitor(reportHandler, filterEventNames, channelIds, channelProviderIds, filterCommunicationIntentIds));
			return _communicationsClientBuilder;
		}

		public CommunicationsClientBuilder AddDeliveryReportHandler(DeliveryReportAsyncHandler reportHandler, IReadOnlyCollection<string>? filterEventNames = null, IReadOnlyCollection<string>? filterChannelIds = null, IReadOnlyCollection<string>? filterChannelProviderIds = null, IReadOnlyCollection<string>? filterCommunciationIntentIds = null)
		{
			_observerRegistrations.Add(new DeliveryReportMonitor(reportHandler, filterEventNames, filterChannelIds, filterChannelProviderIds, filterCommunciationIntentIds));
			return _communicationsClientBuilder;
		}

		internal IDeliveryReportReporter BuildHandler()
		{
			var handler = new DefaultDeliveryReportsReporter();
			handler.Subscribe(_observerRegistrations);
			return handler;
		}
	}
}