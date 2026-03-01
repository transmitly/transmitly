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
}
