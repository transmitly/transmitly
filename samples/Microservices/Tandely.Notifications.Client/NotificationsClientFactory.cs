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

using System.Text.Json;
using System.Text.Json.Serialization;
using Transmitly;
using Transmitly.Channel.Configuration;
using Transmitly.Persona.Configuration;
using Transmitly.Pipeline.Configuration;
using Transmitly.PlatformIdentity.Configuration;
using Transmitly.Samples.Shared;
using Transmitly.Template.Configuration;

namespace Tandely.Notifications.Client
{
	sealed class NotificationsClientFactory(NotificationsOptions options) : ICommunicationClientFactory
	{
		private readonly NotificationsOptions _options = Guard.AgainstNull(options);

		public ICommunicationsClient CreateClient(ICreateCommunicationsClientContext context)
		{
			DefaultPipelineFactory pipelineRegistrations = new(context.Pipelines);

			DefaultChannelProviderFactory channelProviderRegistrations = new(context.ChannelProviders);

			DefaultTemplateEngineFactory templateEngineRegistrations = new(context.TemplateEngines);

			DefaultPersonaFactory personaRegistrations = new(context.Personas);

			DefaultPlatformIdentityResolverRegistrationFactory platformIdentityResolverRegistrations = new(context.PlatformIdentityResolvers);

			var defaultClient = new DefaultCommunicationsClient(
				pipelineRegistrations,
				channelProviderRegistrations,
				templateEngineRegistrations,
				personaRegistrations,
				platformIdentityResolverRegistrations,
				context.DeliveryReportProvider
			);
			var jsonOptions = new JsonSerializerOptions()
			{
				PropertyNameCaseInsensitive = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
			jsonOptions.Converters.Add(new JsonExceptionConverter());
			jsonOptions.Converters.Add(new JsonStringEnumConverter());
			return new NotificationsCommunicationsClient(defaultClient, context, platformIdentityResolverRegistrations, _options, jsonOptions);
		}
	}
}
