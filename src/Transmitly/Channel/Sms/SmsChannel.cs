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

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Transmitly.Template.Configuration;

namespace Transmitly.Channel.Sms
{
	public sealed class SmsChannel(string[]? channelProviderId = null) : ISmsChannel
	{
		private static readonly string[] _supportedAddressTypes = [AudienceAddress.Types.Cell(), AudienceAddress.Types.HomePhone(), AudienceAddress.Types.Phone(), AudienceAddress.Types.Mobile()];
		private static readonly Regex _smsMatchRegex = CreateRegex();

		public IAudienceAddress? FromAddress { get; }

		public IContentTemplateConfiguration Body { get; } = new ContentTemplateConfiguration();

		public string Id => Transmitly.Id.Channel.Sms();

		public IEnumerable<string> AllowedChannelProviderIds => channelProviderId ?? [];

		public Type CommunicationType => typeof(ISms);

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
			return audienceAddress != null && 
				(
					string.IsNullOrWhiteSpace(audienceAddress.Type) ||
					(
						!string.IsNullOrWhiteSpace(audienceAddress.Type) &&
						!_supportedAddressTypes.Contains(audienceAddress.Type)
					)
				) &&
				_smsMatchRegex.IsMatch(audienceAddress.Value);
		}

		private static Regex CreateRegex()
		{
			// https://en.wikipedia.org/wiki/E.164
			const string pattern = @"^\+[1-9]\d{1,14}$";
			const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
			TimeSpan matchTimeout = TimeSpan.FromSeconds(1);

			try
			{
				if (AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null)
				{
					return new Regex(pattern, options, matchTimeout);
				}
			}
			catch
			{
				// Fallback on error
			}

			// Legacy fallback (without explicit match timeout)
			return new Regex(pattern, options);
		}
		private static ReadOnlyCollection<IAttachment> ConvertAttachments(IDispatchCommunicationContext communicationContext)
		{
			if (communicationContext.ContentModel?.Resources?.Count > 0)
			{
				List<IAttachment> attachments = new(communicationContext.ContentModel?.Resources?.Count ?? 0);
				foreach (var resource in communicationContext.ContentModel?.Resources ?? [])
				{
					attachments.Add(new SmsAttachment(resource));
				}
				return attachments.AsReadOnly();
			}
			return new ReadOnlyCollection<IAttachment>([]);
		}
	}
}
