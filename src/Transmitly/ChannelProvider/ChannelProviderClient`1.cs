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
		public virtual Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(object communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken) =>
			DispatchAsync((TCommunication)communication, communicationContext, cancellationToken);

		public abstract Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(TCommunication communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken);

		public virtual void DispatchReport(string eventName, IDispatchCommunicationContext context, TCommunication communication, IReadOnlyCollection<IDispatchResult?> dispatchResults)
		{
			foreach (var result in dispatchResults.Where(r => r != null))
				context.DeliveryReportManager.DispatchReport(new DeliveryReport(eventName, context.ChannelId, context.ChannelProviderId, context.PipelineName, result!.ResourceId, result.DispatchStatus, communication, context.ContentModel));
		}

		public virtual void Dispatch(IDispatchCommunicationContext context, TCommunication communication) =>
			DispatchReport(DeliveryReport.Event.Dispatch(), context, communication, []);

		public virtual void Delivered(IDispatchCommunicationContext context, TCommunication communication, IReadOnlyCollection<IDispatchResult?> dispatchResults) =>
			DispatchReport(DeliveryReport.Event.Delivered(), context, communication, dispatchResults);

		public virtual void Error(IDispatchCommunicationContext context, TCommunication communication, IReadOnlyCollection<IDispatchResult?> dispatchResults) =>
			DispatchReport(DeliveryReport.Event.Error(), context, communication, dispatchResults);

		public virtual void Dispatched(IDispatchCommunicationContext context, TCommunication communication, IReadOnlyCollection<IDispatchResult?> dispatchResults) =>
			DispatchReport(DeliveryReport.Event.Dispatched(), context, communication, dispatchResults);
	}
}
