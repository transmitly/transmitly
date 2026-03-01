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

internal sealed class PushNotificationChannelConfiguration() : IPushNotificationChannelConfiguration
{
	private Dictionary<string, IContentTemplateConfiguration>? _data;
	private Dictionary<string, IContentTemplateConfiguration>? _headers;

	public IContentTemplateConfiguration Title { get; } = new ContentTemplateConfiguration();

	public IContentTemplateConfiguration Body { get; } = new ContentTemplateConfiguration();

	public IContentTemplateConfiguration ImageUrl { get; } = new ContentTemplateConfiguration();

	public IReadOnlyDictionary<string, IContentTemplateConfiguration>? Data => _data;

	public IReadOnlyDictionary<string, IContentTemplateConfiguration>? Headers => _headers;

	public IAndroidPushNotification? Android { get; } = new AndroidPushNotification();

	public IApplePushNotification? Apple { get; } = new ApplePushNotification();

	public IWebPushNotification? Web { get; } = new WebPushNotification();

	public IReadOnlyCollection<string>? RecipientAddressPurposes { get; private set; }

	public IReadOnlyCollection<string>? BlindCopyRecipientPurposes { get; private set; }

	public IReadOnlyCollection<string>? CopyRecipientPurposes { get; private set; }

	public IReadOnlyCollection<string>? ChannelProviderFilter { get; private set; }

	public Func<IDispatchCommunicationContext, Task<string?>>? DeliveryReportCallbackUrlResolver { get; private set; }

	private readonly Lazy<IExtendedProperties> _extendedProprties = new(() => new ExtendedProperties());

	public IExtendedProperties ExtendedProperties => _extendedProprties.Value;

	public IChannelConfiguration AddBlindCopyRecipientAddressPurpose(params string[] purposes)
	{
		BlindCopyRecipientPurposes = purposes;
		return this;
	}

	public IChannelConfiguration AddChannelProviderFilter(params string[] channelProviderIds)
	{
		ChannelProviderFilter = channelProviderIds;
		return this;
	}

	public IChannelConfiguration AddCopyRecipientAddressPurpose(params string[] purposes)
	{
		CopyRecipientPurposes = purposes;
		return this;
	}

	public IChannelConfiguration AddDeliveryReportCallbackUrlResolver(Func<IDispatchCommunicationContext, Task<string?>> callbackResolver)
	{
		DeliveryReportCallbackUrlResolver = Guard.AgainstNull(callbackResolver);
		return this;
	}

	public IChannelConfiguration AddRecipientAddressPurpose(params string[] purposes)
	{
		RecipientAddressPurposes = purposes;
		return this;
	}

	public IPushNotificationChannelConfiguration AddData(string key, Action<IContentTemplateConfiguration> content)
	{
		_data ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplate(_data, key, content);
		return this;
	}

	public IPushNotificationChannelConfiguration AddData(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		_data ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplate(_data, key, contentResolver);
		return this;
	}

	public IPushNotificationChannelConfiguration AddHeader(string key, Action<IContentTemplateConfiguration> content)
	{
		_headers ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplate(_headers, key, content);
		return this;
	}

	public IPushNotificationChannelConfiguration AddHeader(string key, Func<IDispatchCommunicationContext, Task<string?>> contentResolver)
	{
		_headers ??= [];
		PushNotificationTemplateConfigurationHelpers.AddTemplate(_headers, key, contentResolver);
		return this;
	}

	public IPushNotificationChannelConfiguration AddAndroid(Action<IAndroidPushNotificationConfiguration> android)
	{
		Guard.AgainstNull(android);
		if (Android is IAndroidPushNotificationConfiguration androidConfiguration)
		{
			android(androidConfiguration);
		}
		return this;
	}

	public IPushNotificationChannelConfiguration AddApple(Action<IApplePushNotificationConfiguration> apple)
	{
		Guard.AgainstNull(apple);
		if (Apple is IApplePushNotificationConfiguration appleConfiguration)
		{
			apple(appleConfiguration);
		}
		return this;
	}

	public IPushNotificationChannelConfiguration AddWeb(Action<IWebPushNotificationConfiguration> web)
	{
		Guard.AgainstNull(web);
		if (Web is IWebPushNotificationConfiguration webConfiguration)
		{
			web(webConfiguration);
		}
		return this;
	}
}
