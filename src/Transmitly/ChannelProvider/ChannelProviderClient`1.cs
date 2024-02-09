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

namespace Transmitly.ChannelProvider
{
	public abstract class ChannelProviderClient<TCommunication> : IChannelProviderClient<TCommunication>
	{
		public virtual IReadOnlyCollection<string>? RegisteredEvents { get; } = [];

		public virtual Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(object communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken) =>
			DispatchAsync((TCommunication)communication, communicationContext, cancellationToken);

		public abstract Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(TCommunication communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken);

		public virtual void DeliveryReport(string eventName, IDispatchCommunicationContext context, TCommunication communication)
		{
			Guard.AgainstNull(RegisteredEvents);

			if (RegisteredEvents.Contains(eventName, StringComparer.OrdinalIgnoreCase))
				context.DeliveryReportHandler.DeliveryReport(new DeliveryReport(eventName, context.ChannelId, context.ChannelProviderId, context, communication));
			else
				throw new CommunicationsException($"Delivery Report Event, '{eventName}', is not a registered event. Make sure to register your events. See: {nameof(RegisteredEvents)}");
		}

		public virtual void Delivered(IDispatchCommunicationContext context, TCommunication communication) =>
			DeliveryReport(DeliveryReportEvent.Name.Delivered(), context, communication);

		public virtual void Error(IDispatchCommunicationContext context, TCommunication communication) =>
			DeliveryReport(DeliveryReportEvent.Name.Error(), context, communication);

		public virtual void Dispatched(IDispatchCommunicationContext context, TCommunication communication) =>
			DeliveryReport(DeliveryReportEvent.Name.Dispatched(), context, communication);
	}
}
