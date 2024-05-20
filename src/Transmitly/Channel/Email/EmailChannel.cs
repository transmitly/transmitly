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

namespace Transmitly.Channel.Email
{
#if FEATURE_SOURCE_GEN
	internal sealed partial class EmailChannel : IEmailChannel
#else
	internal sealed class EmailChannel : IEmailChannel
#endif
	{
		private readonly string[] _channelProviderId;
		private static readonly Regex _emailMatchRegex = CreateRegEx();

		public IContentTemplateConfiguration Subject { get; } = new ContentTemplateConfiguration();
		public IContentTemplateConfiguration HtmlBody { get; } = new ContentTemplateConfiguration();
		public IContentTemplateConfiguration TextBody { get; } = new ContentTemplateConfiguration();

		public string Id => Transmitly.Id.Channel.Email();
		public IEnumerable<string> AllowedChannelProviderIds => _channelProviderId;

		private readonly Func<IDispatchCommunicationContext, IIdentityAddress>? _fromAddressResolver;
		public IIdentityAddress? FromAddress { get; }

		public Type CommunicationType => typeof(IEmail);

		public ExtendedProperties ExtendedProperties { get; } = new ExtendedProperties();

		public string? DeliveryReportCallbackUrl { get; set; }

		public Func<IDispatchCommunicationContext, Task<string?>>? DeliveryReportCallbackUrlResolver { get; set; }

		internal EmailChannel(Func<IDispatchCommunicationContext, IIdentityAddress> fromAddressResolver, string[]? channelProviderId = null)
		{
			_fromAddressResolver = Guard.AgainstNull(fromAddressResolver);
			_channelProviderId = channelProviderId ?? [];
		}

		internal EmailChannel(IIdentityAddress fromAddress, string[]? channelProviderId = null)
		{
			FromAddress = Guard.AgainstNull(fromAddress);
			_channelProviderId = channelProviderId ?? [];
		}

		public bool SupportsIdentityAddress(IIdentityAddress identityAddress)
		{
			return identityAddress != null &&
				!string.IsNullOrWhiteSpace(identityAddress.Value) &&
				(identityAddress.IsType(IdentityAddress.Types.Email()) || _emailMatchRegex.IsMatch(identityAddress.Value));
		}

		public async Task<object> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
		{
			Guard.AgainstNull(communicationContext);

			var subject = await Subject.RenderAsync(communicationContext);
			var htmlBody = await HtmlBody.RenderAsync(communicationContext, false);
			var textBody = await TextBody.RenderAsync(communicationContext, false);
			var attachments = ConvertAttachments(communicationContext);

			return new EmailCommunication(GetSenderFromAddress(communicationContext), ExtendedProperties)
			{
				To = communicationContext.PlatformIdentities.SelectMany(m => m.Addresses).ToArray(),
				Priority = communicationContext.MessagePriority,
				TransportPriority = communicationContext.TransportPriority,
				Subject = subject,
				HtmlBody = htmlBody,
				TextBody = textBody,
				Attachments = attachments,
				DeliveryReportCallbackUrl = DeliveryReportCallbackUrl,
				DeliveryReportCallbackUrlResolver = DeliveryReportCallbackUrlResolver
			};
		}

		private IIdentityAddress GetSenderFromAddress(IDispatchCommunicationContext communicationContext)
		{
			return Guard.AgainstNull(_fromAddressResolver != null ? _fromAddressResolver(communicationContext) : FromAddress);
		}

		private static ReadOnlyCollection<IAttachment> ConvertAttachments(IDispatchCommunicationContext communicationContext)
		{
			if (communicationContext.ContentModel?.Resources?.Count > 0)
			{
				List<IAttachment> attachments = new(communicationContext.ContentModel?.Resources?.Count ?? 0);
				foreach (var resource in communicationContext.ContentModel?.Resources ?? [])
				{
					attachments.Add(new EmailAttachment(resource));
				}
				return attachments.AsReadOnly();
			}
			return new ReadOnlyCollection<IAttachment>([]);
		}

		const string pattern = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$";
		const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
#if FEATURE_SOURCE_GEN
		[GeneratedRegex(pattern, options)]
		private static partial Regex DefaultRegex();
#endif
		//Source=https://github.com/Microsoft/referencesource/blob/master/System.ComponentModel.DataAnnotations/DataAnnotations/EmailAddressAttribute.cs
		private static Regex CreateRegEx()
		{
			// Set explicit regex match timeout, sufficient enough for email parsing
			// Unless the global REGEX_DEFAULT_MATCH_TIMEOUT is already set
			TimeSpan matchTimeout = TimeSpan.FromSeconds(2);

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
	}
}