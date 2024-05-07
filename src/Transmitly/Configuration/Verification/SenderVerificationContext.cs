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

namespace Transmitly.Verification.Configuration
{

	internal sealed class SenderVerificationContext : ISenderVerificationContext
	{
		public SenderVerificationContext(
			IAudienceAddress audienceAddress,
			string? channelProviderId,
			string? channelId,
			ISenderVerificationConfiguration senderVerificationConfiguration
		)
		{
			Guard.AgainstNull(senderVerificationConfiguration);

			SenderAddress = Guard.AgainstNull(audienceAddress);
			ChannelProviderId = channelProviderId;
			ChannelId = channelId;

			DeliveryReportStatusCallbackUrl = senderVerificationConfiguration.DeliveryReportCallbackUrl;
			DeliveryReportStatusCallbackUrlResolver = senderVerificationConfiguration.DeliveryReportCallbackUrlResolver;
			Message = senderVerificationConfiguration.Message;

			ExtendedProperties = senderVerificationConfiguration.ExtendedProperties;
		}

		public string? ChannelId { get; }

		public string? ChannelProviderId { get; }

		public IAudienceAddress SenderAddress { get; }

		public IExtendedProperties ExtendedProperties { get; }

		public string? DeliveryReportStatusCallbackUrl { get; }

		public Func<ISenderVerificationContext, Task<string?>>? DeliveryReportStatusCallbackUrlResolver { get; }

		public IContentTemplateConfiguration Message { get; }
	}
}