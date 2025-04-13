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

using Transmitly.Template.Configuration;

namespace Transmitly.Channel.Configuration.Push
{
	internal sealed class PushNotificationChannelConfiguration() : IPushNotificationChannelConfiguration
	{
		public IContentTemplateConfiguration Title { get; } = new ContentTemplateConfiguration();

		public IContentTemplateConfiguration Body { get; } = new ContentTemplateConfiguration();

		public IContentTemplateConfiguration ImageUrl { get; } = new ContentTemplateConfiguration();

		public IReadOnlyCollection<string>? RecipientAddressPurposes { get; private set; }

		public IReadOnlyCollection<string>? BlindCopyRecipientPurposes { get; private set; }

		public IReadOnlyCollection<string>? CopyRecipientPurposes { get; private set; }

		public IReadOnlyCollection<string>? ChannelProviderFilter { get; private set; }

		public Func<IDispatchCommunicationContext, Task<string?>>? DeliveryReportCallbackUrlResolver { get; private set; }

		public IChannelConfiguration AddBlindCopyRecipientAddressPurpose(params string[] purposes)
		{
			BlindCopyRecipientPurposes = purposes;
			return this;
		}

		public IChannelConfiguration AddChannelProviderFilter(params string[] channelProviderIds)
		{
			ChannelProviderFilter = channelProviderIds;
			return this;
		}

		public IChannelConfiguration AddCopyRecipientAddressPurpose(params string[] purposes)
		{
			CopyRecipientPurposes = purposes;
			return this;
		}

		public IChannelConfiguration AddDeliveryReportCallbackUrlResolver(Func<IDispatchCommunicationContext, Task<string?>> callbackResolver)
		{
			DeliveryReportCallbackUrlResolver = Guard.AgainstNull(callbackResolver);
			return this;
		}

		public IChannelConfiguration AddRecipientAddressPurpose(params string[] purposes)
		{
			RecipientAddressPurposes = purposes;
			return this;
		}
	}
}

//private static readonly string[] _supportedAddressTypes = [IdentityAddress.Types.DeviceToken(), IdentityAddress.Types.Topic()];

//public string Id => Transmitly.Id.Channel.PushNotification();

//public IEnumerable<string> AllowedChannelProviderIds { get; } = allowedChannelProviderIds ?? [];

//public IContentTemplateConfiguration Title { get; } = new ContentTemplateConfiguration();

//public IContentTemplateConfiguration Body { get; } = new ContentTemplateConfiguration();

//public IContentTemplateConfiguration ImageUrl { get; } = new ContentTemplateConfiguration();

//public Type CommunicationType => typeof(IPushNotification);

//public ExtendedProperties ExtendedProperties { get; } = new ExtendedProperties();

//public async Task<object> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
//{
//	var title = await Title.RenderAsync(communicationContext);
//	var body = await Body.RenderAsync(communicationContext);
//	var imageUrl = await ImageUrl.RenderAsync(communicationContext);

//	var recipients = communicationContext.PlatformIdentities.SelectMany(a => a.Addresses).ToList();

//	return new PushNotificationCommunication(recipients, ExtendedProperties, title, body, imageUrl);
//}

//public bool SupportsIdentityAddress(IIdentityAddress identityAddress)
//{
//	return _supportedAddressTypes.Contains(identityAddress.Type);
//}