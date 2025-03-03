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

namespace Tandely.Notifications.Client
{
	public class NotificationsPipelineDispatchResult
	{

		/// <summary>
		/// The name of the pipeline, if resolution has completed by the time this result is returned.
		/// </summary>
		public string PipelineName { get; set; }

		/// <summary>
		/// The ID of the pipeline instance, if resolution and instantiation has completed by the time this result is returned.
		/// </summary>
		public Guid? PipelineInstanceId { get; set; }

		/// <summary>
		/// A reference identifier you want to track the originating send command to
		/// </summary>
		public string? ExternalInstanceReferenceId { get; set; }

		/// <summary>
		/// The status of the pipeline instance.
		/// </summary>
		public PipelineInstanceStatus PipelineStatus { get; set; }

		/// <summary>
		/// The pipeline resolution associated with the originating communication request or signal.
		/// </summary>
		public PipelineResolutionResult ResolutionStatus { get; set; }
	}
}