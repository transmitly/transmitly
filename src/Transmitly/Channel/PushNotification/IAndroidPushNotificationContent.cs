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

using Transmitly.Channel.Configuration.Push;

namespace Transmitly;

/// <summary>
/// Android specific push notification content payload.
/// </summary>
public interface IAndroidPushNotificationContent : IPushNotificationContent
{
	/// <summary>
	/// Android collapse identifier.
	/// </summary>
	string? CollapseId { get; }
	/// <summary>
	/// Android delivery priority.
	/// </summary>
	AndroidNotificationPriority? Priority { get; }
	/// <summary>
	/// Android time to live.
	/// </summary>
	TimeSpan? TimeToLive { get; }
	/// <summary>
	/// Android target application package identifier.
	/// </summary>
	string? TargetApplicationId { get; }
	/// <summary>
	/// Whether delivery is allowed before first device unlock.
	/// </summary>
	bool? AllowDeliveryBeforeFirstUnlock { get; }
	/// <summary>
	/// Android notification icon resource name.
	/// </summary>
	string? Icon { get; }
	/// <summary>
	/// Android notification accent color.
	/// </summary>
	string? AccentColor { get; }
	/// <summary>
	/// Android notification sound.
	/// </summary>
	string? Sound { get; }
	/// <summary>
	/// Android notification replacement tag.
	/// </summary>
	string? Tag { get; }
	/// <summary>
	/// Android click action.
	/// </summary>
	string? ClickAction { get; }
	/// <summary>
	/// Android title localization key.
	/// </summary>
	string? TitleLocalizationKey { get; }
	/// <summary>
	/// Android title localization arguments.
	/// </summary>
	IReadOnlyCollection<string>? TitleLocalizationArguments { get; }
	/// <summary>
	/// Android body localization key.
	/// </summary>
	string? BodyLocalizationKey { get; }
	/// <summary>
	/// Android body localization arguments.
	/// </summary>
	IReadOnlyCollection<string>? BodyLocalizationArguments { get; }
	/// <summary>
	/// Android notification channel identifier.
	/// </summary>
	string? NotificationChannelId { get; }
	/// <summary>
	/// Android ticker text.
	/// </summary>
	string? Ticker { get; }
	/// <summary>
	/// Whether the notification remains after click.
	/// </summary>
	bool? IsSticky { get; }
	/// <summary>
	/// Event timestamp associated with this notification.
	/// </summary>
	DateTimeOffset? EventTimestamp { get; }
	/// <summary>
	/// Whether this notification is only relevant to the local device.
	/// </summary>
	bool? IsLocalOnly { get; }
	/// <summary>
	/// Visual display priority for the Android notification.
	/// </summary>
	AndroidNotificationDisplayPriority? DisplayPriority { get; }
	/// <summary>
	/// Android vibration timing pattern.
	/// </summary>
	IReadOnlyCollection<TimeSpan>? VibrateTimings { get; }
	/// <summary>
	/// Whether to use default vibration timings.
	/// </summary>
	bool? UseDefaultVibrateTimings { get; }
	/// <summary>
	/// Whether to use the default notification sound.
	/// </summary>
	bool? UseDefaultSound { get; }
	/// <summary>
	/// Android notification light settings.
	/// </summary>
	AndroidNotificationLightSettings? LightSettings { get; }
	/// <summary>
	/// Whether to use default notification light settings.
	/// </summary>
	bool? UseDefaultLightSettings { get; }
	/// <summary>
	/// Android notification visibility.
	/// </summary>
	AndroidNotificationVisibility? Visibility { get; }
	/// <summary>
	/// Badge/count represented by this notification.
	/// </summary>
	int? NotificationCount { get; }
}