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

internal sealed class ApplePushNotification : PushContentConfigurationBase, IApplePushNotificationConfiguration
{
	private static readonly IReadOnlyCollection<string> _emptyArgs = Array.Empty<string>();

	public string? Subtitle { get; private set; }

	public string? SubtitleLocalizationKey { get; private set; }

	public IReadOnlyCollection<string> SubtitleLocalizationArguments { get; private set; } = _emptyArgs;

	public string? ActionLocalizationKey { get; private set; }

	public string? BodyLocalizationKey { get; private set; }

	public IReadOnlyCollection<string>? BodyLocalizationArguments { get; private set; }

	public string? TitleLocalizationKey { get; private set; }

	public IReadOnlyCollection<string>? TitleLocalizationArguments { get; private set; }

	public int? Badge { get; private set; }

	public string? Sound { get; private set; }

	public bool? IsBackgroundUpdate { get; private set; }

	public bool? IsContentMutable { get; private set; }

	public string? Category { get; private set; }

	public string? ThreadId { get; private set; }

	public IApplePushNotificationConfiguration AddData(string key, Action<IContentTemplateConfiguration> content)
	{
		AddDataTemplate(key, content);
		return this;
	}

	public IApplePushNotificationConfiguration AddData(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		AddDataTemplate(key, contentResolver);
		return this;
	}

	public IApplePushNotificationConfiguration AddHeader(string key, Action<IContentTemplateConfiguration> content)
	{
		AddHeaderTemplate(key, content);
		return this;
	}

	public IApplePushNotificationConfiguration AddHeader(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		AddHeaderTemplate(key, contentResolver);
		return this;
	}

	public IApplePushNotificationConfiguration AddSubtitle(string? subtitle)
	{
		Subtitle = subtitle;
		return this;
	}

	public IApplePushNotificationConfiguration AddSubtitleLocalizationKey(string? subtitleLocalizationKey)
	{
		SubtitleLocalizationKey = subtitleLocalizationKey;
		return this;
	}

	public IApplePushNotificationConfiguration AddSubtitleLocalizationArguments(params string[] subtitleLocalizationArguments)
	{
		SubtitleLocalizationArguments = subtitleLocalizationArguments ?? _emptyArgs;
		return this;
	}

	public IApplePushNotificationConfiguration AddActionLocalizationKey(string? actionLocalizationKey)
	{
		ActionLocalizationKey = actionLocalizationKey;
		return this;
	}

	public IApplePushNotificationConfiguration AddBodyLocalizationKey(string? bodyLocalizationKey)
	{
		BodyLocalizationKey = bodyLocalizationKey;
		return this;
	}

	public IApplePushNotificationConfiguration AddBodyLocalizationArguments(params string[] bodyLocalizationArguments)
	{
		BodyLocalizationArguments = bodyLocalizationArguments;
		return this;
	}

	public IApplePushNotificationConfiguration AddTitleLocalizationKey(string? titleLocalizationKey)
	{
		TitleLocalizationKey = titleLocalizationKey;
		return this;
	}

	public IApplePushNotificationConfiguration AddTitleLocalizationArguments(params string[] titleLocalizationArguments)
	{
		TitleLocalizationArguments = titleLocalizationArguments;
		return this;
	}

	public IApplePushNotificationConfiguration AddBadge(int? badge)
	{
		Badge = badge;
		return this;
	}

	public IApplePushNotificationConfiguration AddSound(string? sound)
	{
		Sound = sound;
		return this;
	}

	public IApplePushNotificationConfiguration AddBackgroundUpdate(bool? isBackgroundUpdate)
	{
		IsBackgroundUpdate = isBackgroundUpdate;
		return this;
	}

	public IApplePushNotificationConfiguration AddContentMutable(bool? isContentMutable)
	{
		IsContentMutable = isContentMutable;
		return this;
	}

	public IApplePushNotificationConfiguration AddCategory(string? category)
	{
		Category = category;
		return this;
	}

	public IApplePushNotificationConfiguration AddThreadId(string? threadId)
	{
		ThreadId = threadId;
		return this;
	}
}
