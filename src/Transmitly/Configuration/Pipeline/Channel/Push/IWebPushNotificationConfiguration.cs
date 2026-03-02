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
/// Web specific push configuration builder.
/// </summary>
public interface IWebPushNotificationConfiguration : IWebPushNotification
{
	/// <summary>
	/// Adds a Web custom data value.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="value">Custom data value.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddData(string key, string? value);
	/// <summary>
	/// Adds a Web custom data template.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddData(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds a Web custom data template that will be emitted when the rendered value is not null or whitespace.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddDataIfNotNull(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds a Web custom data resolver.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="contentResolver">Resolver for custom data content.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddData(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds a Web custom data resolver that will be emitted when the resolved value is not null or whitespace.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="contentResolver">Resolver for custom data content.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddDataIfNotNull(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds a Web custom data template when the provided condition returns true.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddData(string key, Action<IContentTemplateConfiguration> content, Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Adds a Web custom data resolver that will be emitted when the value is not null or whitespace and the condition returns true.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="contentResolver">Resolver for custom data content.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddDataIfNotNull(
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Adds a Web header value.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="value">Header value.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddHeader(string key, string? value);
	/// <summary>
	/// Adds a Web header template.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddHeader(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds a Web header template that will be emitted when the rendered value is not null or whitespace.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddHeaderIfNotNull(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds a Web header resolver.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="contentResolver">Resolver for header content.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddHeader(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds a Web header resolver that will be emitted when the resolved value is not null or whitespace.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="contentResolver">Resolver for header content.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddHeaderIfNotNull(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds a Web header template when the provided condition returns true.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddHeader(string key, Action<IContentTemplateConfiguration> content, Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Adds a Web header resolver that will be emitted when the value is not null or whitespace and the condition returns true.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="contentResolver">Resolver for header content.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddHeaderIfNotNull(
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Sets Web notification icon.
	/// </summary>
	/// <param name="icon">Icon URL.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddIcon(string? icon);
	/// <summary>
	/// Sets Web notification badge.
	/// </summary>
	/// <param name="badge">Badge URL.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddBadge(string? badge);
	/// <summary>
	/// Sets Web notification language tag.
	/// </summary>
	/// <param name="language">Language tag.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddLanguage(string? language);
	/// <summary>
	/// Sets whether existing notifications should be renotified.
	/// </summary>
	/// <param name="renotify">Renotify flag.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddRenotify(bool? renotify);
	/// <summary>
	/// Sets whether the notification should remain active until user interaction.
	/// </summary>
	/// <param name="requireInteraction">Require interaction flag.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddRequireInteraction(bool? requireInteraction);
	/// <summary>
	/// Sets whether the notification should be silent.
	/// </summary>
	/// <param name="isSilent">Silent flag.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddSilent(bool? isSilent);
	/// <summary>
	/// Sets Web notification tag.
	/// </summary>
	/// <param name="tag">Tag value.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddTag(string? tag);
	/// <summary>
	/// Sets Web notification timestamp.
	/// </summary>
	/// <param name="timestamp">Timestamp value.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddTimestamp(DateTimeOffset? timestamp);
	/// <summary>
	/// Sets Web notification vibration pattern.
	/// </summary>
	/// <param name="pattern">Vibration pattern values.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddVibratePattern(params int[] pattern);
	/// <summary>
	/// Sets Web notification text direction.
	/// </summary>
	/// <param name="direction">Display direction.</param>
	/// <returns></returns>
	IWebPushNotificationConfiguration AddDirection(WebPushDisplayDirection? direction);
}
