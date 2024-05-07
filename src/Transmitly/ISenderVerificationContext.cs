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

using Transmitly.Template.Configuration;
using Transmitly.Verification.Configuration;

namespace Transmitly
{
	/// <summary>
	/// Context of the sender verification request.
	/// </summary>
	public interface ISenderVerificationContext
	{
		/// <summary>
		/// The <see cref="Id.Channel"/> of the verification context.
		/// </summary>
		string? ChannelId { get; }
		/// <summary>
		/// The <see cref="Id.ChannelProvider"/> of the verification context.
		/// </summary>
		string? ChannelProviderId { get; }
		/// <summary>
		/// The sender address to verify.
		/// </summary>
		IAudienceAddress SenderAddress { get; }
		/// <summary>
		/// Extended extensibility properties.
		/// </summary>
		IExtendedProperties ExtendedProperties { get; }
		/// <summary>
		/// Optional status sender verification status callback url.
		/// </summary>
		string? DeliveryReportStatusCallbackUrl { get; }
		/// <summary>
		/// Optional status sender verification status callback url resolver.
		/// </summary>
		Func<ISenderVerificationContext, Task<string?>>? DeliveryReportStatusCallbackUrlResolver { get; }
		/// <summary>
		/// Optional sender verification message.
		/// </summary>
		IContentTemplateConfiguration Message { get; }
	}
}