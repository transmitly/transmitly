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
using Transmitly.Channel.Configuration;
using Transmitly.Channel.Configuration.Voice;

namespace Transmitly.Channel.Voice;

#if FEATURE_SOURCE_GEN
internal sealed partial class VoiceChannel(IVoiceChannelConfiguration configuration) : IChannel<IVoice>
#else
internal sealed class VoiceChannel(IVoiceChannelConfiguration configuration) : IChannel<IVoice>
#endif
{
	readonly IVoiceChannelConfiguration _configuration = Guard.AgainstNull(configuration);

	private const string pattern = @"^\+?[1-9]\d{1,14}$";

	private const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

	//		private readonly Func<IDispatchCommunicationContext, IIdentityAddress>? _fromAddressResolver;

	private static readonly string[] _supportedAddressTypes = [PlatformIdentityAddress.Types.Cell(), PlatformIdentityAddress.Types.HomePhone(), PlatformIdentityAddress.Types.Phone(), PlatformIdentityAddress.Types.Mobile()];

	private static readonly Regex _voiceMatchRegex = CreateRegEx();


#if FEATURE_SOURCE_GEN
	[GeneratedRegex(pattern, options, 2000)]
	private static partial Regex DefaultRegEx();
#endif
	//Source=https://github.com/Microsoft/referencesource/blob/master/System.ComponentModel.DataAnnotations/DataAnnotations/EmailAddressAttribute.cs
	private static Regex CreateRegEx()
	{
#if FEATURE_SOURCE_GEN
		return DefaultRegEx();
#else
		// Set explicit regex match timeout, sufficient enough for email parsing
		// Unless the global REGEX_DEFAULT_MATCH_TIMEOUT is already set
		TimeSpan matchTimeout = TimeSpan.FromSeconds(2);

		try
		{
			var domainTimeout = AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT");
			if (domainTimeout is not TimeSpan)
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
#endif
	}

	public Type CommunicationType => typeof(IVoice);

	public string Id => Transmitly.Id.Channel.Voice();

	public IEnumerable<string> AllowedChannelProviderIds => _configuration.ChannelProviderFilter ?? Array.Empty<string>();

	public IExtendedProperties ExtendedProperties => new ExtendedProperties();

	public async Task<IVoice> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
	{
		Guard.AgainstNull(communicationContext);
		var message = Guard.AgainstNullOrWhiteSpace(await _configuration.Message.RenderAsync(communicationContext, true));
		return new VoiceCommunication(message, ExtendedProperties)
		{
			VoiceType = _configuration.VoiceType,
			From = GetSenderFromAddress(communicationContext),
			To = [.. communicationContext.PlatformIdentities.SelectMany(m => m.Addresses)],
			TransportPriority = communicationContext.TransportPriority,
			DeliveryReportCallbackUrlResolver = _configuration.DeliveryReportCallbackUrlResolver
		};
	}

	private IPlatformIdentityAddress? GetSenderFromAddress(IDispatchCommunicationContext communicationContext)
	{
		return _configuration.FromAddressResolver != null
			? _configuration.FromAddressResolver(communicationContext)
			: null;
	}

	public bool SupportsIdentityAddress(IPlatformIdentityAddress identityAddress)
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

	async Task<object> IChannel.GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
	{
		return await GenerateCommunicationAsync(communicationContext);
	}
}