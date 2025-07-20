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

namespace Transmitly;

/// <summary>
/// Represents the result of a dispatch operation. 
/// </summary>
public interface IDispatchResult
{
	/// <summary>
	/// The resource identifier for the dispatched communication, if available.	
	/// </summary>
	string? ResourceId { get; }
	/// <summary>
	/// The status of the communication after dispatching.
	/// </summary>
	CommunicationsStatus Status { get; }
	/// <summary>
	/// The identifier of the channel provider used for dispatching the communication.
	/// </summary>
	string? ChannelProviderId { get; }
	/// <summary>
	/// The identifier of the channel used for dispatching the communication.
	/// </summary>
	string? ChannelId { get; }
	/// <summary>
	/// The identifier of the pipeline used for dispatching the communication, if applicable.
	/// </summary>
	string? PipelineId { get; }
	/// <summary>
	/// The intent of the pipeline used for dispatching the communication, if applicable.
	/// </summary>
	string? PipelineIntent { get; }
	/// <summary>
	/// An exception that occurred during the dispatch operation, if any. 
	/// </summary>
	Exception? Exception { get; }
}