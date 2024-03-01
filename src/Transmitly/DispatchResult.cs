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

namespace Transmitly
{
	public sealed class DispatchResult : IDispatchResult
	{
		internal DispatchResult(IDispatchResult result, string channelProviderId, string channelId, Exception? exception = null) : this(result.DispatchStatus, channelProviderId, channelId, exception)
		{
			IsDelivered = DispatchStatus != DispatchStatus.Error;
			ResourceId = result.ResourceId;
			DispatchStatus = result.DispatchStatus;
		}

		internal DispatchResult(DispatchStatus status, string channelProviderId, string channelId, Exception? exception=null)
		{
			DispatchStatus = status;
			ChannelProviderId = channelProviderId;
			ChannelId = channelId;
			Exception = exception;
		}

		public DispatchResult(bool communicationDelivered, string? id = null)
		{
			IsDelivered = communicationDelivered;

			if (IsDelivered)
				DispatchStatus = DispatchStatus.Delivered;

			if (string.IsNullOrWhiteSpace(id))
				ResourceId = Guid.NewGuid().ToString();
			else
				ResourceId = id;
		}

		public string? ResourceId { get; }

		public bool IsDelivered { get; }

		public string? ChannelProviderId { get; }

		public string? ChannelId { get; }

		public DispatchStatus DispatchStatus { get; }

		public Exception? Exception { get; set; }
	}
}