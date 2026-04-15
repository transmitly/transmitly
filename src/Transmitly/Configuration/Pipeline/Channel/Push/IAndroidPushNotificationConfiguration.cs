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

/// <summary>
/// Android specific push configuration builder.
/// </summary>
public interface IAndroidPushNotificationConfiguration : IAndroidPushNotification
{
	/// <summary>
	/// Adds an Android custom data value.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="value">Custom data value.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddData(string key, string? value);
	/// <summary>
	/// Adds an Android custom data template.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddData(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds an Android custom data resolver.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="contentResolver">Resolver for custom data content.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddData(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds an Android custom data template when the provided condition returns true.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddData(string key, Action<IContentTemplateConfiguration> content, Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Adds an Android custom data template that will be emitted when the rendered value is not null or whitespace.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddDataIfNotNull(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds an Android custom data resolver that will be emitted when the resolved value is not null or whitespace.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="contentResolver">Resolver for custom data content.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddDataIfNotNull(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds an Android custom data resolver that will be emitted when the value is not null or whitespace and the condition returns true.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="contentResolver">Resolver for custom data content.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddDataIfNotNull(
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Adds an Android header value.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="value">Header value.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddHeader(string key, string? value);
	/// <summary>
	/// Adds an Android header template.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddHeader(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds an Android header resolver.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="contentResolver">Resolver for header content.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddHeader(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds an Android header template when the provided condition returns true.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddHeader(string key, Action<IContentTemplateConfiguration> content, Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Adds an Android header template that will be emitted when the rendered value is not null or whitespace.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddHeaderIfNotNull(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds an Android header resolver that will be emitted when the resolved value is not null or whitespace.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="contentResolver">Resolver for header content.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddHeaderIfNotNull(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds an Android header resolver that will be emitted when the value is not null or whitespace and the condition returns true.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="contentResolver">Resolver for header content.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddHeaderIfNotNull(
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Sets Android collapse identifier.
	/// </summary>
	/// <param name="collapseId">Collapse identifier.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddCollapseId(string? collapseId);
	/// <summary>
	/// Sets Android delivery priority.
	/// </summary>
	/// <param name="priority">Delivery priority.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddPriority(AndroidNotificationPriority priority);
	/// <summary>
	/// Sets Android time to live.
	/// </summary>
	/// <param name="timeToLive">Time to live value.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddTimeToLive(TimeSpan? timeToLive);
	/// <summary>
	/// Sets Android target application package identifier.
	/// </summary>
	/// <param name="targetApplicationId">Target application package identifier.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddTargetApplicationId(string? targetApplicationId);
	/// <summary>
	/// Sets whether delivery is allowed before first device unlock.
	/// </summary>
	/// <param name="allowDeliveryBeforeFirstUnlock">Flag that controls delivery before first device unlock.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddAllowDeliveryBeforeFirstUnlock(bool? allowDeliveryBeforeFirstUnlock);
	/// <summary>
	/// Sets Android notification icon resource name.
	/// </summary>
	/// <param name="icon">Icon resource name.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddIcon(string? icon);
	/// <summary>
	/// Sets Android notification accent color.
	/// </summary>
	/// <param name="accentColor">Accent color in hexadecimal format (for example, <c>#RRGGBB</c>).</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddAccentColor(string? accentColor);
	/// <summary>
	/// Sets Android notification sound.
	/// </summary>
	/// <param name="sound">Sound name.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddSound(string? sound);
	/// <summary>
	/// Sets Android notification replacement tag.
	/// </summary>
	/// <param name="tag">Notification tag.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddTag(string? tag);
	/// <summary>
	/// Sets Android click action.
	/// </summary>
	/// <param name="clickAction">Click action value.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddClickAction(string? clickAction);
	/// <summary>
	/// Sets Android title localization key.
	/// </summary>
	/// <param name="titleLocalizationKey">Localization key.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddTitleLocalizationKey(string? titleLocalizationKey);
	/// <summary>
	/// Sets Android title localization arguments.
	/// </summary>
	/// <param name="titleLocalizationArguments">Localization arguments.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddTitleLocalizationArguments(params string[] titleLocalizationArguments);
	/// <summary>
	/// Sets Android body localization key.
	/// </summary>
	/// <param name="bodyLocalizationKey">Localization key.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddBodyLocalizationKey(string? bodyLocalizationKey);
	/// <summary>
	/// Sets Android body localization arguments.
	/// </summary>
	/// <param name="bodyLocalizationArguments">Localization arguments.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddBodyLocalizationArguments(params string[] bodyLocalizationArguments);
	/// <summary>
	/// Sets Android notification channel identifier.
	/// </summary>
	/// <param name="notificationChannelId">Notification channel identifier.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddNotificationChannelId(string? notificationChannelId);
	/// <summary>
	/// Sets Android ticker text.
	/// </summary>
	/// <param name="ticker">Ticker text.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddTicker(string? ticker);
	/// <summary>
	/// Sets whether the notification should remain after click.
	/// </summary>
	/// <param name="isSticky">Sticky flag.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddSticky(bool? isSticky);
	/// <summary>
	/// Sets the Android notification event timestamp.
	/// </summary>
	/// <param name="eventTimestamp">Event timestamp.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddEventTimestamp(DateTimeOffset? eventTimestamp);
	/// <summary>
	/// Sets whether this notification is local-only.
	/// </summary>
	/// <param name="isLocalOnly">Local-only flag.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddLocalOnly(bool? isLocalOnly);
	/// <summary>
	/// Sets Android notification display priority.
	/// </summary>
	/// <param name="displayPriority">Display priority.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddDisplayPriority(AndroidNotificationDisplayPriority? displayPriority);
	/// <summary>
	/// Sets Android vibration timings.
	/// </summary>
	/// <param name="vibrateTimings">Vibration timings.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddVibrateTimings(params TimeSpan[] vibrateTimings);
	/// <summary>
	/// Sets whether default vibration timings should be used.
	/// </summary>
	/// <param name="useDefaultVibrateTimings">Default vibration flag.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddUseDefaultVibrateTimings(bool? useDefaultVibrateTimings);
	/// <summary>
	/// Sets whether default sound should be used.
	/// </summary>
	/// <param name="useDefaultSound">Default sound flag.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddUseDefaultSound(bool? useDefaultSound);
	/// <summary>
	/// Sets Android light settings.
	/// </summary>
	/// <param name="lightSettings">Light settings value.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddLightSettings(AndroidNotificationLightSettings? lightSettings);
	/// <summary>
	/// Sets whether default light settings should be used.
	/// </summary>
	/// <param name="useDefaultLightSettings">Default light settings flag.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddUseDefaultLightSettings(bool? useDefaultLightSettings);
	/// <summary>
	/// Sets Android notification visibility.
	/// </summary>
	/// <param name="visibility">Visibility setting.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddVisibility(AndroidNotificationVisibility? visibility);
	/// <summary>
	/// Sets Android notification count.
	/// </summary>
	/// <param name="notificationCount">Notification count.</param>
	/// <returns></returns>
	IAndroidPushNotificationConfiguration AddNotificationCount(int? notificationCount);
}