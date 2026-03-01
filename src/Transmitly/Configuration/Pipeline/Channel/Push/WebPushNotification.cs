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

internal sealed class WebPushNotification : PushContentConfigurationBase, IWebPushNotificationConfiguration
{
	public IContentTemplateConfiguration Icon { get; } = new ContentTemplateConfiguration();

	public IContentTemplateConfiguration Badge { get; } = new ContentTemplateConfiguration();

	public IContentTemplateConfiguration Language { get; } = new ContentTemplateConfiguration();

	public bool? Renotify { get; private set; }

	public bool? RequireInteraction { get; private set; }

	public bool? IsSilent { get; private set; }

	public string? Tag { get; private set; }

	public DateTimeOffset? Timestamp { get; private set; }

	public IReadOnlyCollection<int>? VibratePattern { get; private set; }

	public WebPushDisplayDirection? Direction { get; private set; }

	public IWebPushNotificationConfiguration AddData(string key, Action<IContentTemplateConfiguration> content)
	{
		AddDataTemplate(key, content);
		return this;
	}

	public IWebPushNotificationConfiguration AddData(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		AddDataTemplate(key, contentResolver);
		return this;
	}

	public IWebPushNotificationConfiguration AddHeader(string key, Action<IContentTemplateConfiguration> content)
	{
		AddHeaderTemplate(key, content);
		return this;
	}

	public IWebPushNotificationConfiguration AddHeader(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		AddHeaderTemplate(key, contentResolver);
		return this;
	}

	public IWebPushNotificationConfiguration AddIcon(string? icon)
	{
		if (icon is not null && !string.IsNullOrWhiteSpace(icon))
		{
			Icon.AddStringTemplate(icon);
		}
		return this;
	}

	public IWebPushNotificationConfiguration AddBadge(string? badge)
	{
		if (badge is not null && !string.IsNullOrWhiteSpace(badge))
		{
			Badge.AddStringTemplate(badge);
		}
		return this;
	}

	public IWebPushNotificationConfiguration AddLanguage(string? language)
	{
		if (language is not null && !string.IsNullOrWhiteSpace(language))
		{
			Language.AddStringTemplate(language);
		}
		return this;
	}

	public IWebPushNotificationConfiguration AddRenotify(bool? renotify)
	{
		Renotify = renotify;
		return this;
	}

	public IWebPushNotificationConfiguration AddRequireInteraction(bool? requireInteraction)
	{
		RequireInteraction = requireInteraction;
		return this;
	}

	public IWebPushNotificationConfiguration AddSilent(bool? isSilent)
	{
		IsSilent = isSilent;
		return this;
	}

	public IWebPushNotificationConfiguration AddTag(string? tag)
	{
		Tag = tag;
		return this;
	}

	public IWebPushNotificationConfiguration AddTimestamp(DateTimeOffset? timestamp)
	{
		Timestamp = timestamp;
		return this;
	}

	public IWebPushNotificationConfiguration AddVibratePattern(params int[] pattern)
	{
		VibratePattern = pattern;
		return this;
	}

	public IWebPushNotificationConfiguration AddDirection(WebPushDisplayDirection? direction)
	{
		Direction = direction;
		return this;
	}
}
