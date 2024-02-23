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
using Transmitly.Template.Configuration;

namespace Transmitly.Tests
{
	internal sealed class UnitTestChannel : IChannel
	{
		private string _handlesAddressStartsWith = "unit-test-address";

		public UnitTestChannel(string fromAddress, string channelId = "unit-test-channel", params string[] allowedChannelProviders)
		{
			if (allowedChannelProviders is null || !allowedChannelProviders.Any())
			{
				AllowedChannelProviderIds = [];
			}
			else
			{
				AllowedChannelProviderIds = allowedChannelProviders;
			}
			FromAddress = Guard.AgainstNullOrWhiteSpace(fromAddress).AsAudienceAddress();

			Id = channelId;
		}

		public IAudienceAddress FromAddress { get; set; }
		public IContentTemplateConfiguration Subject { get; } = new ContentTemplateConfiguration();

		public string Id { get; }

		public IEnumerable<string> AllowedChannelProviderIds { get; }

		public bool SupportsAudienceAddress(IAudienceAddress audienceAddress)
		{
			return audienceAddress.Value?.StartsWith(_handlesAddressStartsWith) ?? false;
		}
		public UnitTestChannel HandlesAddressStartsWith(string handlingAddressStarts)
		{
			_handlesAddressStartsWith = Guard.AgainstNullOrWhiteSpace(handlingAddressStarts);
			return this;
		}
		public async Task<object> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
		{
			var subjectTemplate = Subject.TemplateRegistrations.FirstOrDefault(f => f.CultureInfo == communicationContext.CultureInfo);
			if (subjectTemplate == null)
				return new UnitTestCommunication("NO TEMPLATE");

			var subjectContent = await communicationContext.TemplateEngine.RenderAsync(subjectTemplate, communicationContext) ?? string.Empty;

			return new UnitTestCommunication(subjectContent);
		}
	}
}