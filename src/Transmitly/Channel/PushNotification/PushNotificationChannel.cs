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
using Transmitly.Template.Configuration;

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

	public IExtendedProperties ExtendedProperties => _configuration.ExtendedProperties;

	public async Task<IPushNotification> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
	{
		Guard.AgainstNull(communicationContext);

		var title = await _configuration.Title.RenderAsync(communicationContext);
		var body = await _configuration.Body.RenderAsync(communicationContext);
		var imageUrl = await _configuration.ImageUrl.RenderAsync(communicationContext);
		var data = await RenderTemplateDictionaryAsync(_configuration.Data, communicationContext);
		var headers = await RenderTemplateDictionaryAsync(_configuration.Headers, communicationContext);
		var android = await RenderAndroidContentAsync(_configuration.Android, communicationContext);
		var apple = await RenderAppleContentAsync(_configuration.Apple, communicationContext);
		var web = await RenderWebContentAsync(_configuration.Web, communicationContext);
		var recipients = communicationContext.PlatformIdentities.SelectMany(a => a.Addresses).ToList();

		return new PushNotificationCommunication(recipients, ExtendedProperties, title, body, imageUrl, data, headers, android, apple, web);
	}

	public bool SupportsIdentityAddress(IPlatformIdentityAddress identityAddress)
	{
		if (identityAddress == null)
		{
			return false;
		}

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

	private static async Task<IReadOnlyDictionary<string, string>?> RenderTemplateDictionaryAsync(
		IReadOnlyDictionary<string, IContentTemplateConfiguration>? templateDictionary,
		IDispatchCommunicationContext communicationContext)
	{
		if (templateDictionary is null || templateDictionary.Count == 0)
		{
			return null;
		}

		var values = new Dictionary<string, string>(templateDictionary.Count, StringComparer.Ordinal);
		foreach (var template in templateDictionary)
		{
			var renderedValue = await template.Value.RenderAsync(communicationContext, false);
			if (renderedValue is not null && !string.IsNullOrWhiteSpace(renderedValue))
			{
				values[template.Key] = renderedValue;
			}
		}

		return values.Count == 0 ? null : values;
	}

	private static async Task<(string? Title, string? Body, string? ImageUrl, IReadOnlyDictionary<string, string>? Data, IReadOnlyDictionary<string, string>? Headers)>
		RenderContentAsync(IPushContent content, IDispatchCommunicationContext communicationContext)
	{
		var title = await content.Title.RenderAsync(communicationContext, false);
		var body = await content.Body.RenderAsync(communicationContext, false);
		var imageUrl = await content.ImageUrl.RenderAsync(communicationContext, false);
		var data = await RenderTemplateDictionaryAsync(content.Data, communicationContext);
		var headers = await RenderTemplateDictionaryAsync(content.Headers, communicationContext);
		return (title, body, imageUrl, data, headers);
	}

	private static bool HasRenderedContent(
		string? title,
		string? body,
		string? imageUrl,
		IReadOnlyDictionary<string, string>? data,
		IReadOnlyDictionary<string, string>? headers)
	{
		return !string.IsNullOrWhiteSpace(title) ||
			!string.IsNullOrWhiteSpace(body) ||
			!string.IsNullOrWhiteSpace(imageUrl) ||
			(data?.Count > 0) ||
			(headers?.Count > 0);
	}

	private static async Task<IAndroidPushNotificationContent?> RenderAndroidContentAsync(
		IAndroidPushNotification? android,
		IDispatchCommunicationContext communicationContext)
	{
		if (android is null)
		{
			return null;
		}

		var (Title, Body, ImageUrl, Data, Headers) = await RenderContentAsync(android, communicationContext);
		var hasOptionValues = !string.IsNullOrWhiteSpace(android.CollapseId) ||
			android.Priority.HasValue ||
			android.TimeToLive.HasValue ||
			!string.IsNullOrWhiteSpace(android.TargetApplicationId) ||
			android.AllowDeliveryBeforeFirstUnlock.HasValue ||
			!string.IsNullOrWhiteSpace(android.Icon) ||
			!string.IsNullOrWhiteSpace(android.AccentColor) ||
			!string.IsNullOrWhiteSpace(android.Sound) ||
			!string.IsNullOrWhiteSpace(android.Tag) ||
			!string.IsNullOrWhiteSpace(android.ClickAction) ||
			!string.IsNullOrWhiteSpace(android.TitleLocalizationKey) ||
			(android.TitleLocalizationArguments?.Count > 0) ||
			!string.IsNullOrWhiteSpace(android.BodyLocalizationKey) ||
			(android.BodyLocalizationArguments?.Count > 0) ||
			!string.IsNullOrWhiteSpace(android.NotificationChannelId) ||
			!string.IsNullOrWhiteSpace(android.Ticker) ||
			android.IsSticky.HasValue ||
			android.EventTimestamp.HasValue ||
			android.IsLocalOnly.HasValue ||
			android.DisplayPriority.HasValue ||
			(android.VibrateTimings?.Count > 0) ||
			android.UseDefaultVibrateTimings.HasValue ||
			android.UseDefaultSound.HasValue ||
			android.LightSettings != null ||
			android.UseDefaultLightSettings.HasValue ||
			android.Visibility.HasValue ||
			android.NotificationCount.HasValue;

		if (!HasRenderedContent(
			Title,
			Body,
			ImageUrl,
			Data,
			Headers) &&
			!hasOptionValues)
		{
			return null;
		}

		return new AndroidPushNotificationContent
		{
			Title = Title,
			Body = Body,
			ImageUrl = ImageUrl,
			Data = Data,
			Headers = Headers,
			CollapseId = android.CollapseId,
			Priority = android.Priority,
			TimeToLive = android.TimeToLive,
			TargetApplicationId = android.TargetApplicationId,
			AllowDeliveryBeforeFirstUnlock = android.AllowDeliveryBeforeFirstUnlock,
			Icon = android.Icon,
			AccentColor = android.AccentColor,
			Sound = android.Sound,
			Tag = android.Tag,
			ClickAction = android.ClickAction,
			TitleLocalizationKey = android.TitleLocalizationKey,
			TitleLocalizationArguments = android.TitleLocalizationArguments,
			BodyLocalizationKey = android.BodyLocalizationKey,
			BodyLocalizationArguments = android.BodyLocalizationArguments,
			NotificationChannelId = android.NotificationChannelId,
			Ticker = android.Ticker,
			IsSticky = android.IsSticky,
			EventTimestamp = android.EventTimestamp,
			IsLocalOnly = android.IsLocalOnly,
			DisplayPriority = android.DisplayPriority,
			VibrateTimings = android.VibrateTimings,
			UseDefaultVibrateTimings = android.UseDefaultVibrateTimings,
			UseDefaultSound = android.UseDefaultSound,
			LightSettings = android.LightSettings,
			UseDefaultLightSettings = android.UseDefaultLightSettings,
			Visibility = android.Visibility,
			NotificationCount = android.NotificationCount
		};
	}

	private static async Task<IApplePushNotificationContent?> RenderAppleContentAsync(
		IApplePushNotification? apple,
		IDispatchCommunicationContext communicationContext)
	{
		if (apple is null)
		{
			return null;
		}

		var (Title, Body, ImageUrl, Data, Headers) = await RenderContentAsync(apple, communicationContext);
		var hasOptionValues = !string.IsNullOrWhiteSpace(apple.Subtitle) ||
			!string.IsNullOrWhiteSpace(apple.SubtitleLocalizationKey) ||
			apple.SubtitleLocalizationArguments.Count > 0 ||
			!string.IsNullOrWhiteSpace(apple.ActionLocalizationKey) ||
			!string.IsNullOrWhiteSpace(apple.BodyLocalizationKey) ||
			(apple.BodyLocalizationArguments?.Count > 0) ||
			!string.IsNullOrWhiteSpace(apple.TitleLocalizationKey) ||
			(apple.TitleLocalizationArguments?.Count > 0) ||
			apple.Badge.HasValue ||
			!string.IsNullOrWhiteSpace(apple.Sound) ||
			apple.CriticalSound != null ||
			apple.IsBackgroundUpdate.HasValue ||
			apple.IsContentMutable.HasValue ||
			!string.IsNullOrWhiteSpace(apple.Category) ||
			!string.IsNullOrWhiteSpace(apple.ThreadId) ||
			!string.IsNullOrWhiteSpace(apple.LaunchImage) ||
			!string.IsNullOrWhiteSpace(apple.LiveActivityToken) ||
			apple.InterruptionLevel.HasValue ||
			apple.RelevanceScore.HasValue ||
			!string.IsNullOrWhiteSpace(apple.TargetContentId);

		if (!HasRenderedContent(
			Title,
			Body,
			ImageUrl,
			Data,
			Headers) &&
			!hasOptionValues)
		{
			return null;
		}

		return new ApplePushNotificationContent
		{
			Title = Title,
			Body = Body,
			ImageUrl = ImageUrl,
			Data = Data,
			Headers = Headers,
			Subtitle = apple.Subtitle,
			SubtitleLocalizationKey = apple.SubtitleLocalizationKey,
			SubtitleLocalizationArguments = apple.SubtitleLocalizationArguments,
			ActionLocalizationKey = apple.ActionLocalizationKey,
			BodyLocalizationKey = apple.BodyLocalizationKey,
			BodyLocalizationArguments = apple.BodyLocalizationArguments,
			TitleLocalizationKey = apple.TitleLocalizationKey,
			TitleLocalizationArguments = apple.TitleLocalizationArguments,
			Badge = apple.Badge,
			Sound = apple.Sound,
			CriticalSound = apple.CriticalSound,
			IsBackgroundUpdate = apple.IsBackgroundUpdate,
			IsContentMutable = apple.IsContentMutable,
			Category = apple.Category,
			ThreadId = apple.ThreadId,
			LaunchImage = apple.LaunchImage,
			LiveActivityToken = apple.LiveActivityToken,
			InterruptionLevel = apple.InterruptionLevel,
			RelevanceScore = apple.RelevanceScore,
			TargetContentId = apple.TargetContentId
		};
	}

	private static async Task<IWebPushNotificationContent?> RenderWebContentAsync(
		IWebPushNotification? web,
		IDispatchCommunicationContext communicationContext)
	{
		if (web is null)
		{
			return null;
		}

		var (Title, Body, ImageUrl, Data, Headers) = await RenderContentAsync(web, communicationContext);
		var icon = await web.Icon.RenderAsync(communicationContext, false);
		var badge = await web.Badge.RenderAsync(communicationContext, false);
		var language = await web.Language.RenderAsync(communicationContext, false);

		var hasOptionValues = !string.IsNullOrWhiteSpace(icon) ||
			!string.IsNullOrWhiteSpace(badge) ||
			!string.IsNullOrWhiteSpace(language) ||
			web.Renotify.HasValue ||
			web.RequireInteraction.HasValue ||
			web.IsSilent.HasValue ||
			!string.IsNullOrWhiteSpace(web.Tag) ||
			web.Timestamp.HasValue ||
			(web.VibratePattern?.Count > 0) ||
			web.Direction.HasValue ||
			(web.Actions?.Count > 0);

		if (!HasRenderedContent(
			Title,
			Body,
			ImageUrl,
			Data,
			Headers) &&
			!hasOptionValues)
		{
			return null;
		}

		return new WebPushNotificationContent
		{
			Title = Title,
			Body = Body,
			ImageUrl = ImageUrl,
			Data = Data,
			Headers = Headers,
			Icon = icon,
			Badge = badge,
			Language = language,
			Renotify = web.Renotify,
			RequireInteraction = web.RequireInteraction,
			IsSilent = web.IsSilent,
			Tag = web.Tag,
			Timestamp = web.Timestamp,
			VibratePattern = web.VibratePattern,
			Direction = web.Direction,
			Actions = web.Actions
		};
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
