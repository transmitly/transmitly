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
/// Push notification channel configuration.
/// </summary>
public interface IPushNotificationChannelConfiguration : IChannelConfiguration
{
	/// <summary>
	/// Default push title templates.
	/// </summary>
	IContentTemplateConfiguration Title { get; }
	/// <summary>
	/// Default push body templates.
	/// </summary>
	IContentTemplateConfiguration Body { get; }
	/// <summary>
	/// Default push image URL templates.
	/// </summary>
	IContentTemplateConfiguration ImageUrl { get; }

	/// <summary>
	/// Android specific push configuration.
	/// </summary>
	IAndroidPushNotification? Android { get; }
	/// <summary>
	/// Apple specific push configuration.
	/// </summary>
	IApplePushNotification? Apple { get; }
	/// <summary>
	/// Web specific push configuration.
	/// </summary>
	IWebPushNotification? Web { get; }
	/// <summary>
	/// Default push custom data templates.
	/// </summary>
	IReadOnlyDictionary<string, IContentTemplateConfiguration>? Data { get; }
	/// <summary>
	/// Default push header templates.
	/// </summary>
	IReadOnlyDictionary<string, IContentTemplateConfiguration>? Headers { get; }

	/// <summary>
	/// Adds a default custom data value.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="value">Custom data value.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddData(string key, string? value);
	/// <summary>
	/// Adds a default custom data template.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddData(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds a default custom data resolver.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="contentResolver">Resolver for custom data content.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddData(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds a default custom data template when the provided condition returns true.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddData(string key, Action<IContentTemplateConfiguration> content, Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Adds a default custom data template that will be emitted when the rendered value is not null or whitespace.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddDataIfNotNull(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds a default custom data resolver that will be emitted when the resolved value is not null or whitespace.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="contentResolver">Resolver for custom data content.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddDataIfNotNull(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds a default custom data resolver that will be emitted when the value is not null or whitespace and the condition returns true.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="contentResolver">Resolver for custom data content.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddDataIfNotNull(
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Adds a default header value.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="value">Header value.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddHeader(string key, string? value);
	/// <summary>
	/// Adds a default header resolver.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="contentResolver">Resolver for header content.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddHeader(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds a default header template.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddHeader(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds a default header template when the provided condition returns true.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddHeader(string key, Action<IContentTemplateConfiguration> content, Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Adds a default header template that will be emitted when the rendered value is not null or whitespace.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddHeaderIfNotNull(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds a default header resolver that will be emitted when the resolved value is not null or whitespace.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="contentResolver">Resolver for header content.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddHeaderIfNotNull(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds a default header resolver that will be emitted when the value is not null or whitespace and the condition returns true.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="contentResolver">Resolver for header content.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddHeaderIfNotNull(
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Configures Android specific push settings.
	/// </summary>
	/// <param name="android">Android configuration action.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddAndroid(Action<IAndroidPushNotificationConfiguration> android);
	/// <summary>
	/// Configures Apple specific push settings.
	/// </summary>
	/// <param name="apple">Apple configuration action.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddApple(Action<IApplePushNotificationConfiguration> apple);
	/// <summary>
	/// Configures Web specific push settings.
	/// </summary>
	/// <param name="web">Web configuration action.</param>
	/// <returns></returns>
	IPushNotificationChannelConfiguration AddWeb(Action<IWebPushNotificationConfiguration> web);
}