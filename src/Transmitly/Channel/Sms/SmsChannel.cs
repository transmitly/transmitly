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

using System.Text.RegularExpressions;
using Transmitly.Template.Configuration;

namespace Transmitly.Channel.Sms
{
	internal sealed class SmsChannel(string[]? channelProviderId = null) : ISmsChannel
	{
		private static readonly string[] _supportedAddressTypes = [AudienceAddress.Types.Cell(), AudienceAddress.Types.HomePhone(), AudienceAddress.Types.Phone(), AudienceAddress.Types.Mobile()];
		public IAudienceAddress? FromAddress { get; }

		public IContentTemplateConfiguration Body { get; } = new ContentTemplateConfiguration();

		public string Id => Transmitly.Id.Channel.Sms();

		public IEnumerable<string> AllowedChannelProviderIds => channelProviderId ?? [];

		public async Task<object> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
		{
			var body = await Body.RenderAsync(communicationContext, true).ConfigureAwait(false);

			return new SmsCommunication()
			{
				From = FromAddress,
				Body = body,
				Attachments = ConvertAttachments(communicationContext),
				Priority = communicationContext.MessagePriority,
				TransportPriority = communicationContext.TransportPriority,
				To = communicationContext.RecipientAudiences.SelectMany(m => m.Addresses).ToArray()
			};
		}

		public bool SupportsAudienceAddress(IAudienceAddress audienceAddress)
		{
			string phoneNumberPattern = @"^[\+\d\s()-]{1,20}$";
			if (!string.IsNullOrWhiteSpace(audienceAddress.Type) && !_supportedAddressTypes.Contains(audienceAddress.Type))
			{
				return false;
			}
			return Regex.IsMatch(audienceAddress.Value, phoneNumberPattern, RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));
		}

		private static IReadOnlyCollection<IAttachment> ConvertAttachments(IDispatchCommunicationContext communicationContext)
		{
			if (communicationContext.ContentModel?.Resources?.Count > 0)
			{
				List<IAttachment> attachments = new(communicationContext.ContentModel?.Resources?.Count ?? 0);
				foreach (var resource in communicationContext.ContentModel?.Resources ?? [])
				{
					attachments.Add(new SmsAttachment(resource));
				}
				return attachments;
			}
			return [];
		}
	}
}
