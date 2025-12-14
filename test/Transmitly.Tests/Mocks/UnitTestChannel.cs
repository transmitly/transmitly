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

using Transmitly.Channel.Configuration;
using Transmitly.Util;

namespace Transmitly.Tests;

internal class UnitTestChannel : IChannel
{
	public UnitTestChannel(string fromAddress, string channelId = "unit-test-channel", params string[] allowedChannelProviders)
	{
		if (allowedChannelProviders is null)
		{
			AllowedChannelProviderIds = [];
		}
		else
		{
			AllowedChannelProviderIds = allowedChannelProviders;
		}
		FromAddress = Guard.AgainstNullOrWhiteSpace(fromAddress).AsIdentityAddress();

		Id = channelId;
		Configuration = new UnitTestChannelConfiguration();
	}
	public UnitTestChannelConfiguration Configuration { get; }
	public IEnumerable<string> AllowedChannelProviderIds { get; }

	public IExtendedProperties ExtendedProperties { get; } = new ExtendedProperties();

	public bool SupportsIdentityAddress(IPlatformIdentityAddress identityAddress)
	{
		return identityAddress.Value?.StartsWith(_handlesAddressStartsWith) ?? false;
	}
	public UnitTestChannel HandlesAddressStartsWith(string handlingAddressStarts)
	{
		_handlesAddressStartsWith = Guard.AgainstNullOrWhiteSpace(handlingAddressStarts);
		return this;
	}
	public async Task<object> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
	{
		var subjectTemplate = Configuration.Subject.TemplateRegistrations.FirstOrDefault(f => f.CultureInfo.Equals(communicationContext.CultureInfo));
		if (subjectTemplate == null)
			return new UnitTestCommunication("NO TEMPLATE");

		var subjectContent = await communicationContext.TemplateEngine.RenderAsync(subjectTemplate, communicationContext) ?? string.Empty;

		return new UnitTestCommunication(subjectContent);
	}

	private string _handlesAddressStartsWith = "unit-test-address";

	public IPlatformIdentityAddress FromAddress { get; set; }

	public string Id { get; }

	public Type CommunicationType => typeof(UnitTestCommunication);
}