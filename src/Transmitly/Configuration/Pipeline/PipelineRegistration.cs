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

namespace Transmitly.Pipeline.Configuration
{
	///<inheritdoc cref="IPipeline"/>
	internal class PipelineRegistration(
		   IPipelineConfiguration configuration,
		   string communicationIntentId,
		   string? platformIdentityType,
		   string? category,
		   string? pipelineName,
		   TransportPriority transportPriority = TransportPriority.Normal,
		   MessagePriority messagePriority = MessagePriority.Normal
	) : IPipeline
	{
		public string CommunicationIntentId { get; } = Guard.AgainstNullOrWhiteSpace(communicationIntentId);

		public string? Description { get; set; }

		public string? PlatformIdentityType { get; } = platformIdentityType;

		public string? Category { get; } = category;

		public TransportPriority TransportPriority { get; } = transportPriority;

		public MessagePriority MessagePriority { get; } = messagePriority;

		public IPipelineConfiguration PipelineConfiguration { get; } = Guard.AgainstNull(configuration);

		public string? PipelineName { get; } = pipelineName;
	}
}
