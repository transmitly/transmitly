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

using System.Globalization;
using Transmitly.Delivery;
using Transmitly.Pipeline.Configuration;
using Transmitly.Template.Configuration;

namespace Transmitly;

/// <summary>
/// Represents the communication context.
/// </summary>
public interface IDispatchCommunicationContext
{
	/// <summary>
	/// Gets or sets the content model.
	/// </summary>
	IContentModel? ContentModel { get; }

	/// <summary>
	/// Gets or sets the culture information.
	/// </summary>
	CultureInfo CultureInfo { get; }

	/// <summary>
	/// Gets or sets the collection of recipients.
	/// </summary>
	IReadOnlyCollection<IPlatformIdentityProfile> PlatformIdentities { get; }

	/// <summary>
	/// Gets or sets the transport priority.
	/// </summary>
	TransportPriority TransportPriority { get; }

	/// <summary>
	/// Gets or sets the message priority.
	/// </summary>
	MessagePriority MessagePriority { get; }

	/// <summary>
	/// Gets or sets the channel configuration.
	/// </summary>
	IPipelineConfiguration ChannelConfiguration { get; }

	/// <summary>
	/// Gets the collection of dispatch results.
	/// </summary>
	ICollection<IDispatchResult> DispatchResults { get; }

	/// <summary>
	/// Gets the delivery report handler instance.
	/// </summary>
	IDeliveryReportService DeliveryReportManager { get; }

	/// <summary>
	/// Gets the channel Id. 
	/// </summary>
	string? ChannelId { get; }
	/// <summary>
	/// Gets the channel provider Id. 
	/// </summary>
	string? ChannelProviderId { get; }

	/// <summary>
	/// Gets the template engine.
	/// </summary>
	ITemplateEngine TemplateEngine { get; }

	/// <summary>
	/// Gets the current pipeline intent.
	/// </summary>
	string PipelineIntent { get; }

	/// <summary>
	/// Gets the unique identifier for the pipeline.
	/// </summary>
	string? PipelineId { get; }
}