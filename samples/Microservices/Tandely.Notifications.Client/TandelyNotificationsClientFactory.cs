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

using Transmitly;
using Transmitly.Channel.Configuration;
using Transmitly.Persona.Configuration;
using Transmitly.Pipeline.Configuration;
using Transmitly.PlatformIdentity.Configuration;
using Transmitly.Template.Configuration;

namespace Tandely.Notifications.Client
{
    sealed class TandelyNotificationsClientFactory : ICommunicationClientFactory
    {
        private readonly TandelyNotificationsOptions _options;

        public TandelyNotificationsClientFactory(TandelyNotificationsOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public ICommunicationsClient CreateClient(ICreateCommunicationsClientContext context)
        {
            DefaultPipelineFactory pipelineRegistrations = new DefaultPipelineFactory(context.Pipelines);

            DefaultChannelProviderFactory channelProviderRegistrations = new DefaultChannelProviderFactory(context.ChannelProviders);

            DefaultTemplateEngineFactory templateEngineRegistrations = new DefaultTemplateEngineFactory(context.TemplateEngines);

            DefaultPersonaFactory personaRegistrations = new DefaultPersonaFactory(context.Personas);

            DefaultPlatformIdentityResolverRegistrationFactory platformIdentityResolverRegistrations = new DefaultPlatformIdentityResolverRegistrationFactory(context.PlatformIdentityResolvers);


            var defaultClient = new DefaultCommunicationsClient(
                pipelineRegistrations,
                channelProviderRegistrations,
                templateEngineRegistrations,
                personaRegistrations,
                platformIdentityResolverRegistrations,
                context.DeliveryReportProvider
            );

            return new TandelyNotificationsCommunicationsClient(defaultClient, context, platformIdentityResolverRegistrations, _options);
        }
    }
}
