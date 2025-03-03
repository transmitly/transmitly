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
using System.Text.RegularExpressions;

namespace Transmitly.Channel.Voice
{
#if FEATURE_SOURCE_GEN
    internal sealed partial class VoiceChannel(string[]? channelProviderIds = null) : IVoiceChannel
#else
	internal sealed class VoiceChannel(string[]? channelProviderIds = null) : IVoiceChannel
#endif
	{
#if FEATURE_SOURCE_GEN
		[GeneratedRegex(pattern, options)]
		private static partial Regex DefaultRegex();
#endif
		private const string pattern = @"^\+?[1-9]\d{1,14}$";

		private const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

		private readonly Func<IDispatchCommunicationContext, IIdentityAddress>? _fromAddressResolver;

		private static readonly string[] _supportedAddressTypes = [IdentityAddress.Types.Cell(), IdentityAddress.Types.HomePhone(), IdentityAddress.Types.Phone(), IdentityAddress.Types.Mobile()];

		private static readonly Regex _voiceMatchRegex = CreateRegex();

		public IIdentityAddress? From { get; }

		public IVoiceType? VoiceType { get; set; }

		public IContentTemplateConfiguration Message { get; } = new ContentTemplateConfiguration();

		public Type CommunicationType => typeof(IVoice);

		public string Id => Transmitly.Id.Channel.Voice();

		public IEnumerable<string> AllowedChannelProviderIds => channelProviderIds ?? [];

		public ExtendedProperties ExtendedProperties { get; } = new ExtendedProperties();

		public MachineDetection MachineDetection { get; set; }

		public string? DeliveryReportCallbackUrl { get; set; }

		public Func<IDispatchCommunicationContext, Task<string?>>? DeliveryReportCallbackUrlResolver { get; set; }

		internal VoiceChannel(IIdentityAddress? fromAddress, string[]? channelProviderIds = null) : this(channelProviderIds)
		{
			From = fromAddress;
		}

		internal VoiceChannel(Func<IDispatchCommunicationContext, IIdentityAddress> fromAddressResolver, string[]? channelProviderIds = null) : this(channelProviderIds)
		{
			_fromAddressResolver = Guard.AgainstNull(fromAddressResolver);
		}

		public async Task<object> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
		{
			Guard.AgainstNull(communicationContext);
			var message = Guard.AgainstNullOrWhiteSpace(await Message.RenderAsync(communicationContext, true));
			return new VoiceCommunication(message, ExtendedProperties)
			{
				VoiceType = VoiceType,
				From = GetSenderFromAddress(communicationContext),
				To = communicationContext.PlatformIdentities.SelectMany(m => m.Addresses).ToArray(),
				TransportPriority = communicationContext.TransportPriority,
				DeliveryReportCallbackUrl = DeliveryReportCallbackUrl,
				DeliveryReportCallbackUrlResolver = DeliveryReportCallbackUrlResolver
			};
		}

		public bool SupportsIdentityAddress(IIdentityAddress identityAddress)
		{
			return identityAddress != null &&
				(
					string.IsNullOrWhiteSpace(identityAddress.Type) ||
					(
						!string.IsNullOrWhiteSpace(identityAddress.Type) &&
						!_supportedAddressTypes.Contains(identityAddress.Type)
					)
				) &&
				_voiceMatchRegex.IsMatch(identityAddress.Value);
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
	}
}
