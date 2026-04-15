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
/// Apple specific push configuration builder.
/// </summary>
public interface IApplePushNotificationConfiguration : IApplePushNotification
{
	/// <summary>
	/// Adds an Apple custom data value.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="value">Custom data value.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddData(string key, string? value);
	/// <summary>
	/// Adds an Apple custom data template.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddData(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds an Apple custom data resolver.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="contentResolver">Resolver for custom data content.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddData(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds an Apple custom data template when the provided condition returns true.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddData(string key, Action<IContentTemplateConfiguration> content, Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Adds an Apple custom data template that will be emitted when the rendered value is not null or whitespace.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddDataIfNotNull(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds an Apple custom data resolver that will be emitted when the resolved value is not null or whitespace.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="contentResolver">Resolver for custom data content.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddDataIfNotNull(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds an Apple custom data resolver that will be emitted when the value is not null or whitespace and the condition returns true.
	/// </summary>
	/// <param name="key">Custom data key.</param>
	/// <param name="contentResolver">Resolver for custom data content.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddDataIfNotNull(
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Adds an Apple header value.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="value">Header value.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddHeader(string key, string? value);
	/// <summary>
	/// Adds an Apple header resolver.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="contentResolver">Resolver for header content.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddHeader(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds an Apple header template.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddHeader(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds an Apple header template when the provided condition returns true.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddHeader(string key, Action<IContentTemplateConfiguration> content, Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Adds an Apple header template that will be emitted when the rendered value is not null or whitespace.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="content">Template configuration action.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddHeaderIfNotNull(string key, Action<IContentTemplateConfiguration> content);
	/// <summary>
	/// Adds an Apple header resolver that will be emitted when the resolved value is not null or whitespace.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="contentResolver">Resolver for header content.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddHeaderIfNotNull(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver);
	/// <summary>
	/// Adds an Apple header resolver that will be emitted when the value is not null or whitespace and the condition returns true.
	/// </summary>
	/// <param name="key">Header key.</param>
	/// <param name="contentResolver">Resolver for header content.</param>
	/// <param name="addIfCondition">Condition that controls whether the key should be emitted.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddHeaderIfNotNull(
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		Func<IDispatchCommunicationContext, bool> addIfCondition);
	/// <summary>
	/// Sets Apple notification subtitle.
	/// </summary>
	/// <param name="subtitle">Subtitle value.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddSubtitle(string? subtitle);
	/// <summary>
	/// Sets Apple localized subtitle key.
	/// </summary>
	/// <param name="subtitleLocalizationKey">Localized subtitle key.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddSubtitleLocalizationKey(string? subtitleLocalizationKey);
	/// <summary>
	/// Sets Apple localized subtitle arguments.
	/// </summary>
	/// <param name="subtitleLocalizationArguments">Localized subtitle arguments.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddSubtitleLocalizationArguments(params string[] subtitleLocalizationArguments);
	/// <summary>
	/// Sets Apple action localization key.
	/// </summary>
	/// <param name="actionLocalizationKey">Action localization key.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddActionLocalizationKey(string? actionLocalizationKey);
	/// <summary>
	/// Sets Apple body localization key.
	/// </summary>
	/// <param name="bodyLocalizationKey">Body localization key.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddBodyLocalizationKey(string? bodyLocalizationKey);
	/// <summary>
	/// Sets Apple body localization arguments.
	/// </summary>
	/// <param name="bodyLocalizationArguments">Body localization arguments.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddBodyLocalizationArguments(params string[] bodyLocalizationArguments);
	/// <summary>
	/// Sets Apple title localization key.
	/// </summary>
	/// <param name="titleLocalizationKey">Title localization key.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddTitleLocalizationKey(string? titleLocalizationKey);
	/// <summary>
	/// Sets Apple title localization arguments.
	/// </summary>
	/// <param name="titleLocalizationArguments">Title localization arguments.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddTitleLocalizationArguments(params string[] titleLocalizationArguments);
	/// <summary>
	/// Sets Apple badge count.
	/// </summary>
	/// <param name="badge">Badge count.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddBadge(int? badge);
	/// <summary>
	/// Sets Apple sound name.
	/// </summary>
	/// <param name="sound">Sound name.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddSound(string? sound);
	/// <summary>
	/// Sets Apple critical sound configuration.
	/// </summary>
	/// <param name="criticalSound">Critical sound configuration.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddCriticalSound(AppleCriticalSound? criticalSound);
	/// <summary>
	/// Sets whether this is a background update notification.
	/// </summary>
	/// <param name="isBackgroundUpdate">Background update flag.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddBackgroundUpdate(bool? isBackgroundUpdate);
	/// <summary>
	/// Sets whether content can be modified by a notification service extension.
	/// </summary>
	/// <param name="isContentMutable">Content mutable flag.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddContentMutable(bool? isContentMutable);
	/// <summary>
	/// Sets Apple notification category identifier.
	/// </summary>
	/// <param name="category">Category identifier.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddCategory(string? category);
	/// <summary>
	/// Sets Apple notification thread identifier.
	/// </summary>
	/// <param name="threadId">Thread identifier.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddThreadId(string? threadId);
	/// <summary>
	/// Sets Apple notification launch image.
	/// </summary>
	/// <param name="launchImage">Launch image.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddLaunchImage(string? launchImage);
	/// <summary>
	/// Sets Apple live activity token.
	/// </summary>
	/// <param name="liveActivityToken">Live activity token.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddLiveActivityToken(string? liveActivityToken);
	/// <summary>
	/// Sets Apple interruption level.
	/// </summary>
	/// <param name="interruptionLevel">Interruption level.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddInterruptionLevel(AppleNotificationInterruptionLevel? interruptionLevel);
	/// <summary>
	/// Sets Apple relevance score.
	/// </summary>
	/// <param name="relevanceScore">Relevance score.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddRelevanceScore(double? relevanceScore);
	/// <summary>
	/// Sets Apple target content identifier.
	/// </summary>
	/// <param name="targetContentId">Target content identifier.</param>
	/// <returns></returns>
	IApplePushNotificationConfiguration AddTargetContentId(string? targetContentId);
}
