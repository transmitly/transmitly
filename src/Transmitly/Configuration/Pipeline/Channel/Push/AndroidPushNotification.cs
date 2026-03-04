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

namespace Transmitly.Channel.Configuration.Push;

internal sealed class AndroidPushNotification : PushContentConfigurationBase, IAndroidPushNotificationConfiguration
{
	private static readonly IReadOnlyCollection<string> _emptyArgs = Array.Empty<string>();

	public string? CollapseId { get; private set; }

	public AndroidNotificationPriority? Priority { get; private set; }

	public TimeSpan? TimeToLive { get; private set; }

	public string? TargetApplicationId { get; private set; }

	public bool? AllowDeliveryBeforeFirstUnlock { get; private set; }

	public string? Icon { get; private set; }

	public string? AccentColor { get; private set; }

	public string? Sound { get; private set; }

	public string? Tag { get; private set; }

	public string? ClickAction { get; private set; }

	public string? TitleLocalizationKey { get; private set; }

	public IReadOnlyCollection<string>? TitleLocalizationArguments { get; private set; }

	public string? BodyLocalizationKey { get; private set; }

	public IReadOnlyCollection<string>? BodyLocalizationArguments { get; private set; }

	public string? NotificationChannelId { get; private set; }

	public string? Ticker { get; private set; }

	public bool? IsSticky { get; private set; }

	public DateTimeOffset? EventTimestamp { get; private set; }

	public bool? IsLocalOnly { get; private set; }

	public AndroidNotificationDisplayPriority? DisplayPriority { get; private set; }

	public IReadOnlyCollection<TimeSpan>? VibrateTimings { get; private set; }

	public bool? UseDefaultVibrateTimings { get; private set; }

	public bool? UseDefaultSound { get; private set; }

	public AndroidNotificationLightSettings? LightSettings { get; private set; }

	public bool? UseDefaultLightSettings { get; private set; }

	public AndroidNotificationVisibility? Visibility { get; private set; }

	public int? NotificationCount { get; private set; }

	public IAndroidPushNotificationConfiguration AddData(string key, Action<IContentTemplateConfiguration> content)
	{
		AddDataTemplate(key, content);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddData(string key, string? value)
	{
		AddDataTemplate(key, value);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddData(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		AddDataTemplate(key, contentResolver);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddDataIfNotNull(string key, Action<IContentTemplateConfiguration> content)
	{
		AddDataTemplateIfNotNull(key, content);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddDataIfNotNull(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		AddDataTemplateIfNotNull(key, contentResolver);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddData(string key, Action<IContentTemplateConfiguration> content, Func<IDispatchCommunicationContext, bool> addIfCondition)
	{
		AddDataTemplate(key, content, addIfCondition);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddDataIfNotNull(
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		Func<IDispatchCommunicationContext, bool> addIfCondition)
	{
		AddDataTemplateIfNotNull(key, contentResolver, addIfCondition);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddHeader(string key, Action<IContentTemplateConfiguration> content)
	{
		AddHeaderTemplate(key, content);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddHeader(string key, string? value)
	{
		AddHeaderTemplate(key, value);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddHeader(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		AddHeaderTemplate(key, contentResolver);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddHeaderIfNotNull(string key, Action<IContentTemplateConfiguration> content)
	{
		AddHeaderTemplateIfNotNull(key, content);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddHeaderIfNotNull(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		AddHeaderTemplateIfNotNull(key, contentResolver);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddHeader(string key, Action<IContentTemplateConfiguration> content, Func<IDispatchCommunicationContext, bool> addIfCondition)
	{
		AddHeaderTemplate(key, content, addIfCondition);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddHeaderIfNotNull(
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		Func<IDispatchCommunicationContext, bool> addIfCondition)
	{
		AddHeaderTemplateIfNotNull(key, contentResolver, addIfCondition);
		return this;
	}

	public IAndroidPushNotificationConfiguration AddCollapseId(string? collapseId)
	{
		CollapseId = collapseId;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddPriority(AndroidNotificationPriority priority)
	{
		Priority = priority;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddTimeToLive(TimeSpan? timeToLive)
	{
		TimeToLive = timeToLive;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddTargetApplicationId(string? targetApplicationId)
	{
		TargetApplicationId = targetApplicationId;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddAllowDeliveryBeforeFirstUnlock(bool? allowDeliveryBeforeFirstUnlock)
	{
		AllowDeliveryBeforeFirstUnlock = allowDeliveryBeforeFirstUnlock;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddIcon(string? icon)
	{
		Icon = icon;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddAccentColor(string? accentColor)
	{
		AccentColor = accentColor;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddSound(string? sound)
	{
		Sound = sound;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddTag(string? tag)
	{
		Tag = tag;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddClickAction(string? clickAction)
	{
		ClickAction = clickAction;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddTitleLocalizationKey(string? titleLocalizationKey)
	{
		TitleLocalizationKey = titleLocalizationKey;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddTitleLocalizationArguments(params string[] titleLocalizationArguments)
	{
		TitleLocalizationArguments = titleLocalizationArguments ?? _emptyArgs;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddBodyLocalizationKey(string? bodyLocalizationKey)
	{
		BodyLocalizationKey = bodyLocalizationKey;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddBodyLocalizationArguments(params string[] bodyLocalizationArguments)
	{
		BodyLocalizationArguments = bodyLocalizationArguments ?? _emptyArgs;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddNotificationChannelId(string? notificationChannelId)
	{
		NotificationChannelId = notificationChannelId;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddTicker(string? ticker)
	{
		Ticker = ticker;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddSticky(bool? isSticky)
	{
		IsSticky = isSticky;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddEventTimestamp(DateTimeOffset? eventTimestamp)
	{
		EventTimestamp = eventTimestamp;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddLocalOnly(bool? isLocalOnly)
	{
		IsLocalOnly = isLocalOnly;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddDisplayPriority(AndroidNotificationDisplayPriority? displayPriority)
	{
		DisplayPriority = displayPriority;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddVibrateTimings(params TimeSpan[] vibrateTimings)
	{
		VibrateTimings = vibrateTimings;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddUseDefaultVibrateTimings(bool? useDefaultVibrateTimings)
	{
		UseDefaultVibrateTimings = useDefaultVibrateTimings;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddUseDefaultSound(bool? useDefaultSound)
	{
		UseDefaultSound = useDefaultSound;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddLightSettings(AndroidNotificationLightSettings? lightSettings)
	{
		LightSettings = lightSettings;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddUseDefaultLightSettings(bool? useDefaultLightSettings)
	{
		UseDefaultLightSettings = useDefaultLightSettings;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddVisibility(AndroidNotificationVisibility? visibility)
	{
		Visibility = visibility;
		return this;
	}

	public IAndroidPushNotificationConfiguration AddNotificationCount(int? notificationCount)
	{
		NotificationCount = notificationCount;
		return this;
	}
}