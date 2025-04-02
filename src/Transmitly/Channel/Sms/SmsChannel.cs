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

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Transmitly.Template.Configuration;

namespace Transmitly.Channel.Sms
{
#if FEATURE_SOURCE_GEN
    internal sealed partial class SmsChannel(string[]? channelProviderId = null) : ISmsChannel
#else
	internal sealed class SmsChannel(string[]? channelProviderId = null) : ISmsChannel
#endif
	{
		private static readonly string[] _supportedAddressTypes = [IdentityAddress.Types.Cell(), IdentityAddress.Types.HomePhone(), IdentityAddress.Types.Phone(), IdentityAddress.Types.Mobile()];
		private static readonly Regex _smsMatchRegex = CreateRegex();
		private readonly Func<IDispatchCommunicationContext, IIdentityAddress>? _fromAddressResolver;
		const string pattern = @"^\+?[1-9]\d{1,14}$";
		const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
#if FEATURE_SOURCE_GEN
		[GeneratedRegex(pattern, options)]
		private static partial Regex DefaultRegex();
#endif
		public IIdentityAddress? From { get; }

		public IContentTemplateConfiguration Message { get; } = new ContentTemplateConfiguration();

		public string Id => Transmitly.Id.Channel.Sms();

		public IEnumerable<string> AllowedChannelProviderIds => channelProviderId ?? [];

		public Type CommunicationType => typeof(ISms);

		public ExtendedProperties ExtendedProperties { get; } = new ExtendedProperties();

		public string? DeliveryReportCallbackUrl { get; set; }

		public Func<IDispatchCommunicationContext, Task<string?>>? DeliveryReportCallbackUrlResolver { get; set; }

		internal SmsChannel(IIdentityAddress? fromAddress, string[]? channelProviderId = null) : this(channelProviderId)
		{
			From = fromAddress;
		}

		internal SmsChannel(Func<IDispatchCommunicationContext, IIdentityAddress> fromAddressResolver, string[]? channelProviderId = null) : this(channelProviderId)
		{
			_fromAddressResolver = Guard.AgainstNull(fromAddressResolver);
		}

		public async Task<object> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
		{
			Guard.AgainstNull(communicationContext);

			var body = await Message.RenderAsync(communicationContext, true).ConfigureAwait(false);

			return new SmsCommunication(ExtendedProperties)
			{
				From = GetSenderFromAddress(communicationContext),
				Message = body,
				Attachments = ConvertAttachments(communicationContext),
				Priority = communicationContext.MessagePriority,
				TransportPriority = communicationContext.TransportPriority,
				To = communicationContext.PlatformIdentities.SelectMany(m => m.Addresses).ToArray(),
				DeliveryReportCallbackUrl = DeliveryReportCallbackUrl,
				DeliveryReportCallbackUrlResolver = DeliveryReportCallbackUrlResolver
			};
		}

		public bool SupportsIdentityAddress(IIdentityAddress identityAddress)
		{
			return identityAddress != null &&
				(
					string.IsNullOrWhiteSpace(identityAddress.ChannelId) ||
					(
						!string.IsNullOrWhiteSpace(identityAddress.ChannelId) &&
						!_supportedAddressTypes.Contains(identityAddress.ChannelId)
					)
				) &&
				_smsMatchRegex.IsMatch(identityAddress.Value);
		}

		private IIdentityAddress? GetSenderFromAddress(IDispatchCommunicationContext communicationContext)
		{
			return _fromAddressResolver != null ? _fromAddressResolver(communicationContext) : From;
		}

		private static Regex CreateRegex()
		{
			// https://en.wikipedia.org/wiki/E.164

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
#if FEATURE_SOURCE_GEN
			return DefaultRegex();
#else
			return new Regex(pattern, options);
#endif
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
