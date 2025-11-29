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
using Transmitly.Channel.Configuration.Push;
using Transmitly.Channel.Push;

namespace Transmitly;

#if FEATURE_SOURCE_GEN
sealed partial class PushNotificationChannel(IPushNotificationChannelConfiguration configuration) : IChannel<IPushNotification>
#else
sealed class PushNotificationChannel(IPushNotificationChannelConfiguration configuration) : IChannel<IPushNotification>
#endif
{
	private readonly IPushNotificationChannelConfiguration _configuration = Guard.AgainstNull(configuration);
	private static readonly string[] _supportedAddressTypes = [PlatformIdentityAddress.Types.DeviceToken(), PlatformIdentityAddress.Types.Topic()];
	private static readonly HashSet<string> _deviceTokenKeys = new(StringComparer.OrdinalIgnoreCase)
	{
		PlatformIdentityAddress.Types.DeviceToken(), "devicetoken", "device_token", "push-token", "pushtoken", "push_token", "token"
	};
	private static readonly HashSet<string> _topicKeys = new(StringComparer.OrdinalIgnoreCase)
	{
		PlatformIdentityAddress.Types.Topic(), "topic", "push-topic", "pushtopic", "push_topic", "/topics"
	};
	private static readonly Regex _deviceTokenPattern = CreateDeviceTokenRegex();

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

	public bool SupportsIdentityAddress(IPlatformIdentityAddress identityAddress)
	{
		if (identityAddress.Type is string type && _supportedAddressTypes.Contains(type))
		{
			return true;
		}

		return IsDeviceToken(identityAddress) || IsTopic(identityAddress);
	}

	async Task<object> IChannel.GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
	{
		return await GenerateCommunicationAsync(communicationContext);
	}

	private static bool IsDeviceToken(IPlatformIdentityAddress identityAddress)
	{
		if (MatchesConvention(identityAddress, _deviceTokenKeys))
		{
			return true;
		}

		return _deviceTokenPattern.IsMatch(identityAddress.Value);
	}

	private static bool IsTopic(IPlatformIdentityAddress identityAddress)
	{
		if (MatchesConvention(identityAddress, _topicKeys))
		{
			return true;
		}

		return identityAddress.Value.StartsWith("/topics/", StringComparison.OrdinalIgnoreCase) ||
			identityAddress.Value.StartsWith("topic:", StringComparison.OrdinalIgnoreCase);
	}

	private static bool MatchesConvention(IPlatformIdentityAddress identityAddress, HashSet<string> keys)
	{
		if (identityAddress.Type is string type && keys.Contains(type))
		{
			return true;
		}

		if (identityAddress.Purposes is not null && identityAddress.Purposes.Any(p => p is not null && keys.Contains(p)))
		{
			return true;
		}

		if (identityAddress.AddressParts.Keys.Any(keys.Contains))
		{
			return true;
		}

		return identityAddress.Attributes.Keys.Any(keys.Contains);
	}

	private const string DeviceTokenPattern = @"^(?:[A-Fa-f0-9]{64}|[A-Za-z0-9_-]{20,})$";
	private const RegexOptions DeviceTokenRegexOptions = RegexOptions.Compiled;

#if FEATURE_SOURCE_GEN
	[GeneratedRegex(DeviceTokenPattern, DeviceTokenRegexOptions, 2000)]
	private static partial Regex DeviceTokenRegex();
#endif

	private static Regex CreateDeviceTokenRegex()
	{
#if FEATURE_SOURCE_GEN
		return DeviceTokenRegex();
#else
		TimeSpan matchTimeout = TimeSpan.FromSeconds(2);
		try
		{
			var domainTimeout = AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT");
			if (domainTimeout is not TimeSpan)
			{
				return new Regex(DeviceTokenPattern, DeviceTokenRegexOptions, matchTimeout);
			}
		}
		catch
		{
			// ignore and fallback
		}

		return new Regex(DeviceTokenPattern, DeviceTokenRegexOptions);
#endif
	}
}
