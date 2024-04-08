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

namespace Transmitly.Channel.Push
{
	internal sealed class PushNotificationChannel(IReadOnlyCollection<string>? allowedChannelProviderIds) : IPushNotificationChannel
	{
		private static readonly string[] _supportedAddressTypes = [AudienceAddress.Types.DeviceToken(), AudienceAddress.Types.Topic()];

		public string Id => Transmitly.Id.Channel.PushNotification();

		public IEnumerable<string> AllowedChannelProviderIds { get; } = allowedChannelProviderIds ?? [];

		public IContentTemplateConfiguration Title { get; } = new ContentTemplateConfiguration();

		public IContentTemplateConfiguration Body { get; } = new ContentTemplateConfiguration();

		public IContentTemplateConfiguration ImageUrl { get; } = new ContentTemplateConfiguration();

		public Type CommunicationType => typeof(IPushNotification);

		public ExtendedProperties ExtendedProperties { get; } = new ExtendedProperties();

		public string? CommunicationId { get; set; }

		public async Task<object> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
		{
			var title = await Title.RenderAsync(communicationContext);
			var body = await Body.RenderAsync(communicationContext);
			var imageUrl = await ImageUrl.RenderAsync(communicationContext);

			var recipients = communicationContext.RecipientAudiences.SelectMany(a => a.Addresses).ToList();

			return new PushNotificationCommunication(recipients, ExtendedProperties, title, body, imageUrl) { CommunicationId = CommunicationId };
		}

		public bool SupportsAudienceAddress(IAudienceAddress audienceAddress)
		{
			return _supportedAddressTypes.Contains(audienceAddress.Type);
		}
	}
}