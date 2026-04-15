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

internal abstract class PushContentConfigurationBase : IPushContent
{
	private static readonly IReadOnlyDictionary<string, IContentTemplateConfiguration> _empty =
		new Dictionary<string, IContentTemplateConfiguration>(0);
	private Dictionary<string, IContentTemplateConfiguration>? _data;
	private Dictionary<string, IContentTemplateConfiguration>? _headers;

	public IContentTemplateConfiguration Title { get; } = new ContentTemplateConfiguration();

	public IContentTemplateConfiguration Body { get; } = new ContentTemplateConfiguration();

	public IContentTemplateConfiguration ImageUrl { get; } = new ContentTemplateConfiguration();

	public IReadOnlyDictionary<string, IContentTemplateConfiguration> Data => _data ?? _empty;

	public IReadOnlyDictionary<string, IContentTemplateConfiguration>? Headers => _headers;

	protected void AddDataTemplate(string key, Action<IContentTemplateConfiguration> content)
	{
		_data ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplate(_data, key, content);
	}

	protected void AddDataTemplate(string key, string? value)
	{
		_data ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplate(_data, key, value);
	}

	protected void AddDataTemplate(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		_data ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplate(_data, key, contentResolver);
	}

	protected void AddDataTemplate(string key, Action<IContentTemplateConfiguration> content, Func<IDispatchCommunicationContext, bool> addIfCondition)
	{
		_data ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplate(_data, key, content, addIfCondition);
	}

	protected void AddDataTemplateIfNotNull(string key, Action<IContentTemplateConfiguration> content)
	{
		_data ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplateIfNotNull(_data, key, content);
	}

	protected void AddDataTemplateIfNotNull(
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		Func<IDispatchCommunicationContext, bool> addIfCondition)
	{
		_data ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplateIfNotNull(_data, key, contentResolver, addIfCondition);
	}

	protected void AddDataTemplateIfNotNull(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		_data ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplateIfNotNull(_data, key, contentResolver);
	}

	protected void AddHeaderTemplate(string key, Action<IContentTemplateConfiguration> content)
	{
		_headers ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplate(_headers, key, content);
	}

	protected void AddHeaderTemplate(string key, string? value)
	{
		_headers ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplate(_headers, key, value);
	}

	protected void AddHeaderTemplate(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		_headers ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplate(_headers, key, contentResolver);
	}

	protected void AddHeaderTemplate(string key, Action<IContentTemplateConfiguration> content, Func<IDispatchCommunicationContext, bool> addIfCondition)
	{
		_headers ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplate(_headers, key, content, addIfCondition);
	}

	protected void AddHeaderTemplateIfNotNull(string key, Action<IContentTemplateConfiguration> content)
	{
		_headers ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplateIfNotNull(_headers, key, content);
	}

	protected void AddHeaderTemplateIfNotNull(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		_headers ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplateIfNotNull(_headers, key, contentResolver);
	}

	protected void AddHeaderTemplateIfNotNull(
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		Func<IDispatchCommunicationContext, bool> addIfCondition)
	{
		_headers ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplateIfNotNull(_headers, key, contentResolver, addIfCondition);
	}
}
