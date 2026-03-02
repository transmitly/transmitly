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
using Transmitly;

namespace Transmitly.Channel.Configuration.Push;

internal static class PushNotificationTemplateConfigurationHelpers
{
	internal static void AddTemplate(
		Dictionary<string, IContentTemplateConfiguration> templates,
		string key,
		string? value)
	{
		Guard.AgainstNull(templates);
		key = Guard.AgainstNullOrWhiteSpace(key);

		if (value is null)
		{
			return;
		}

		AddTemplate(templates, key, c => c.AddStringTemplate(value));
	}

	internal static void AddTemplate(
		Dictionary<string, IContentTemplateConfiguration> templates,
		string key,
		Action<IContentTemplateConfiguration> content)
	{
		Guard.AgainstNull(templates);
		key = Guard.AgainstNullOrWhiteSpace(key);
		Guard.AgainstNull(content);

		var value = new ContentTemplateConfiguration();
		content(value);

#if NET5_0_OR_GREATER
		templates.TryAdd(key, value);
#else
		if (!templates.ContainsKey(key))
			templates.Add(key, value);
#endif
	}

	internal static void AddTemplate(
		Dictionary<string, IContentTemplateConfiguration> templates,
		string key,
		Action<IContentTemplateConfiguration> content,
		Func<IDispatchCommunicationContext, bool> addIfCondition)
	{
		Guard.AgainstNull(addIfCondition);
		var template = BuildTemplate(content);
		AddTemplate(templates, key, context => ResolveTemplateAsync(template, context, addIfCondition, addIfNotNull: false));
	}

	internal static void AddTemplateIfNotNull(
		Dictionary<string, IContentTemplateConfiguration> templates,
		string key,
		Action<IContentTemplateConfiguration> content)
	{
		var template = BuildTemplate(content);
		AddTemplate(templates, key, context => ResolveTemplateAsync(template, context, addIfCondition: null, addIfNotNull: true));
	}

	internal static void AddTemplate(
		Dictionary<string, IContentTemplateConfiguration> templates,
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		Guard.AgainstNull(contentResolver);
		AddTemplate(templates, key, c => c.AddTemplateResolver(contentResolver));
	}

	internal static void AddTemplateIfNotNull(
		Dictionary<string, IContentTemplateConfiguration> templates,
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		Guard.AgainstNull(contentResolver);
		AddTemplate(templates, key, context => ResolveContentAsync(contentResolver, context, addIfCondition: null, addIfNotNull: true));
	}

	internal static void AddTemplateIfNotNull(
		Dictionary<string, IContentTemplateConfiguration> templates,
		string key,
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		Func<IDispatchCommunicationContext, bool> addIfCondition)
	{
		Guard.AgainstNull(addIfCondition);
		Guard.AgainstNull(contentResolver);
		AddTemplate(templates, key, context => ResolveContentAsync(contentResolver, context, addIfCondition, addIfNotNull: true));
	}

	private static ContentTemplateConfiguration BuildTemplate(Action<IContentTemplateConfiguration> content)
	{
		Guard.AgainstNull(content);
		var template = new ContentTemplateConfiguration();
		content(template);
		return template;
	}

	private static async Task<string?> ResolveTemplateAsync(
		IContentTemplateConfiguration content,
		IDispatchCommunicationContext context,
		Func<IDispatchCommunicationContext, bool>? addIfCondition,
		bool addIfNotNull)
	{
		if (addIfCondition is not null && !addIfCondition(context))
		{
			return null;
		}

		var value = await content.RenderAsync(context, false);
		return addIfNotNull && string.IsNullOrWhiteSpace(value)
			? null
			: value;
	}

	private static async Task<string?> ResolveContentAsync(
		Func<IDispatchCommunicationContext, Task<string?>> contentResolver,
		IDispatchCommunicationContext context,
		Func<IDispatchCommunicationContext, bool>? addIfCondition,
		bool addIfNotNull)
	{
		if (addIfCondition is not null && !addIfCondition(context))
		{
			return null;
		}

		var value = await contentResolver(context);
		return addIfNotNull && string.IsNullOrWhiteSpace(value)
			? null
			: value;
	}
}
