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

namespace Transmitly.Channel.Configuration.Email;

///<inheritdoc cref="IEmailChannelConfiguration"/>
sealed class EmailChannelConfiguration(Func<IDispatchCommunicationContext, IPlatformIdentityAddress>? fromAddressResolver = null) : IEmailChannelConfiguration
{
	public IContentTemplateConfiguration Subject { get; } = new ContentTemplateConfiguration();

	public IContentTemplateConfiguration HtmlBody { get; } = new ContentTemplateConfiguration();

	public IContentTemplateConfiguration TextBody { get; } = new ContentTemplateConfiguration();

	public Func<IDispatchCommunicationContext, IPlatformIdentityAddress>? FromAddressResolver => fromAddressResolver;

	public Func<IDispatchCommunicationContext, Task<string?>>? DeliveryReportCallbackUrlResolver { get; set; }

	public IReadOnlyCollection<string>? RecipientAddressPurposes { get; private set; }

	public IReadOnlyCollection<string>? BlindCopyRecipientPurposes { get; private set; }

	public IReadOnlyCollection<string>? ChannelProviderFilter { get; private set; }

	public IReadOnlyCollection<string>? CopyRecipientPurposes { get; private set; }

	private readonly Lazy<IExtendedProperties> _extendedProprties = new(() => new ExtendedProperties());
	public IExtendedProperties ExtendedProperties => _extendedProprties.Value;

	public IChannelConfiguration AddBlindCopyRecipientAddressPurpose(params string[] purposes)
	{
		BlindCopyRecipientPurposes = [.. purposes];
		return this;
	}

	public IChannelConfiguration AddChannelProviderFilter(params string[] channelProviderIds)
	{
		ChannelProviderFilter = [.. channelProviderIds];
		return this;
	}

	public IChannelConfiguration AddCopyRecipientAddressPurpose(params string[] purposes)
	{
		CopyRecipientPurposes = [.. purposes];
		return this;
	}

	public IChannelConfiguration AddDeliveryReportCallbackUrlResolver(Func<IDispatchCommunicationContext, Task<string?>> callbackResolver)
	{
		DeliveryReportCallbackUrlResolver = Guard.AgainstNull(callbackResolver);
		return this;
	}

	public IChannelConfiguration AddRecipientAddressPurpose(params string[] purposes)
	{
		RecipientAddressPurposes = [.. purposes];
		return this;
	}
}