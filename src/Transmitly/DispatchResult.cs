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
		internal DispatchResult(IDispatchResult result, string channelProviderId, string channelId, Exception? exception = null) : this(result.Status, channelProviderId, channelId, exception)
		{
			ResourceId = result.ResourceId;
			Status = result.Status;
		}

		internal DispatchResult(CommunicationsStatus status, string channelProviderId, string channelId, Exception? exception = null)
		{
			Status = status;
			ChannelProviderId = channelProviderId;
			ChannelId = channelId;
			Exception = exception;
		}

		public DispatchResult(CommunicationsStatus status, string? id = null)
		{
			Status = status;

			if (string.IsNullOrWhiteSpace(id))
				ResourceId = Guid.NewGuid().ToString();
			else
				ResourceId = id;
		}

		public string? ResourceId { get; }

		public string? ChannelProviderId { get; }

		public string? ChannelId { get; }

		public CommunicationsStatus Status { get; internal set; }

		public Exception? Exception { get; internal set; }

		public string? PipelineId {get;internal set; }

		public string? PipelineName { get; internal set; }
	}
}