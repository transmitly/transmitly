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
		private const string pattern = @"^\+[1-9]\d{1,14}$";

		private const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

		private static readonly string[] _supportedAddressTypes = [AudienceAddress.Types.Cell(), AudienceAddress.Types.HomePhone(), AudienceAddress.Types.Phone(), AudienceAddress.Types.Mobile()];

		private static readonly Regex _voiceMatchRegex = CreateRegex();

		public IAudienceAddress? From { get; set; }

		public IVoiceType? VoiceType { get; }

		public IContentTemplateConfiguration Message { get; } = new ContentTemplateConfiguration();

		public Type CommunicationType => typeof(IVoice);

		public string Id => Transmitly.Id.Channel.Voice();

		public IEnumerable<string> AllowedChannelProviderIds => channelProviderIds ?? [];

		public ExtendedProperties ExtendedProperties { get; } = new ExtendedProperties();

		public async Task<object> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
		{
			var message = Guard.AgainstNullOrWhiteSpace(await Message.RenderAsync(communicationContext, true));
			return new VoiceCommunication(message, ExtendedProperties)
			{
				VoiceType = VoiceType,
				From = From,
				To = communicationContext.RecipientAudiences.SelectMany(m => m.Addresses).ToArray(),
				TransportPriority = communicationContext.TransportPriority
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
				_voiceMatchRegex.IsMatch(audienceAddress.Value);
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
