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

namespace Transmitly.Pipeline.Configuration
{
	/// <summary>
	/// A dispatch pipeline.
	/// </summary>
	public interface IPipeline
	{
		/// <summary>
		/// Name of the pipeline.
		/// </summary>
		string PipelineName { get; }
		/// <summary>
		/// Description of the pipeline.
		/// </summary>
		string? Description { get; set; }
		/// <summary>
		/// Category of the pipeline.
		/// </summary>
		string? Category { get; }
		/// <summary>
		/// Priority of the transportation method.
		/// </summary>
		TransportPriority TransportPriority { get; }
		/// <summary>
		/// Priority of the message.
		/// </summary>
		MessagePriority MessagePriority { get; }
		/// <summary>
		/// Pipeline channel configuration.
		/// </summary>
		IPipelineConfiguration ChannelConfiguration { get; }
	}
}