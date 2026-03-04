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

namespace Transmitly.Channel.Push;

internal sealed class AndroidPushNotificationContent : PushNotificationContent, IAndroidPushNotificationContent
{
	public string? CollapseId { get; internal set; }

	public AndroidNotificationPriority? Priority { get; internal set; }

	public TimeSpan? TimeToLive { get; internal set; }

	public string? TargetApplicationId { get; internal set; }

	public bool? AllowDeliveryBeforeFirstUnlock { get; internal set; }

	public string? Icon { get; internal set; }

	public string? AccentColor { get; internal set; }

	public string? Sound { get; internal set; }

	public string? Tag { get; internal set; }

	public string? ClickAction { get; internal set; }

	public string? TitleLocalizationKey { get; internal set; }

	public IReadOnlyCollection<string>? TitleLocalizationArguments { get; internal set; }

	public string? BodyLocalizationKey { get; internal set; }

	public IReadOnlyCollection<string>? BodyLocalizationArguments { get; internal set; }

	public string? NotificationChannelId { get; internal set; }

	public string? Ticker { get; internal set; }

	public bool? IsSticky { get; internal set; }

	public DateTimeOffset? EventTimestamp { get; internal set; }

	public bool? IsLocalOnly { get; internal set; }

	public AndroidNotificationDisplayPriority? DisplayPriority { get; internal set; }

	public IReadOnlyCollection<TimeSpan>? VibrateTimings { get; internal set; }

	public bool? UseDefaultVibrateTimings { get; internal set; }

	public bool? UseDefaultSound { get; internal set; }

	public AndroidNotificationLightSettings? LightSettings { get; internal set; }

	public bool? UseDefaultLightSettings { get; internal set; }

	public AndroidNotificationVisibility? Visibility { get; internal set; }

	public int? NotificationCount { get; internal set; }
}

