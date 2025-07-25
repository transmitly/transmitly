﻿// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
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

namespace Transmitly.Pipeline.Configuration;

///<inheritdoc cref="IPipeline"/>
internal class PipelineRegistration(
	   IPipelineConfiguration configuration,
	   string pipelineIntent,
	   string? platformIdentityType,
	   string? category
) : IPipeline
{
	public string? Id => configuration.PipelineId;

	public string Intent { get; } = Guard.AgainstNullOrWhiteSpace(pipelineIntent);

	public string? PlatformIdentityType { get; } = platformIdentityType;

	public string? Category { get; } = category;

	public IPipelineConfiguration Configuration { get; } = Guard.AgainstNull(configuration);
}