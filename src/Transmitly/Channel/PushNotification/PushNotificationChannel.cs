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

using Transmitly.Channel.Configuration;
using Transmitly.Channel.Configuration.Push;
using Transmitly.Channel.Push;

namespace Transmitly
{
	sealed class PushNotificationChannel(IPushNotificationChannelConfiguration configuration) : IChannel<IPushNotification>
	{
		//const string pushTokenPattern = @"\b(?:[A-Fa-f0-9]{64}|[A-Za-z0-9_-]{20,})\b";
		private readonly IPushNotificationChannelConfiguration _configuration = Guard.AgainstNull(configuration);
		private static readonly string[] _supportedAddressTypes = [IdentityAddress.Types.DeviceToken(), IdentityAddress.Types.Topic()];

		public Type CommunicationType => typeof(IPushNotification);

		public string Id => Transmitly.Id.Channel.PushNotification();

		public IEnumerable<string> AllowedChannelProviderIds => _configuration.ChannelProviderFilter ?? Array.Empty<string>();

		public IExtendedProperties ExtendedProperties { get; } = new ExtendedProperties();

		public async Task<IPushNotification> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
		{
			var title = await _configuration.Title.RenderAsync(communicationContext);
			var body = await _configuration.Body.RenderAsync(communicationContext);
			var imageUrl = await _configuration.ImageUrl.RenderAsync(communicationContext);

			var recipients = communicationContext.PlatformIdentities.SelectMany(a => a.Addresses).ToList();

			return new PushNotificationCommunication(recipients, ExtendedProperties, title, body, imageUrl);
		}

		public bool SupportsIdentityAddress(IIdentityAddress identityAddress)
		{
			return _supportedAddressTypes.Contains(identityAddress.Type);
		}

		async Task<object> IChannel.GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
		{
			return await GenerateCommunicationAsync(communicationContext);
		}
	}
}