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

public interface IEmail
{
	/// <summary>
	/// Email subject.
	/// </summary>
	string? Subject { get; }
	/// <summary>
	/// Email html body.
	/// </summary>
	string? HtmlBody { get; }
	/// <summary>
	/// Email plain text body.
	/// </summary>
	string? TextBody { get; }
	/// <summary>
	/// Email priority.
	/// </summary>
	MessagePriority Priority { get; }
	/// <summary>
	/// Email transport priority.
	/// </summary>
	TransportPriority TransportPriority { get; }
	/// <summary>
	/// The sender of the email.
	/// </summary>
	IPlatformIdentityAddress From { get; }
	/// <summary>
	/// The reply to address of the email.
	/// </summary>
	IPlatformIdentityAddress[]? ReplyTo { get; }
	/// <summary>
	/// The recipient of the email.
	/// </summary>
	IPlatformIdentityAddress[]? To { get; }
	/// <summary>
	/// The carbon copy recipient of the email.
	/// </summary>
	IPlatformIdentityAddress[]? Cc { get; }
	/// <summary>
	/// The blind carbon copy recipient of the email.
	/// </summary>
	IPlatformIdentityAddress[]? Bcc { get; }
	/// <summary>
	/// The attachments of the email.
	/// </summary>
	IReadOnlyCollection<IEmailAttachment> Attachments { get; }
	/// <summary>
	/// Extended properties of the email.
	/// </summary>
	IExtendedProperties ExtendedProperties { get; }
	/// <summary>
	/// A resolver that will return The URL to call for status updates for the dispatched communication.
	/// </summary>
	Func<IDispatchCommunicationContext, Task<string?>>? DeliveryReportCallbackUrlResolver { get; }
}