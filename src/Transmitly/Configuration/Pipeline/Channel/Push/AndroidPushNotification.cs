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
	public string? CollapseId { get; private set; }

	public AndroidNotificationPriority? Priority { get; private set; }

	public TimeSpan? TimeToLive { get; private set; }

	public string? TargetApplicationId { get; private set; }

	public bool? AllowDeliveryBeforeFirstUnlock { get; private set; }

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
}
