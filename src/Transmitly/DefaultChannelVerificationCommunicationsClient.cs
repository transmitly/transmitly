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
using Transmitly.ChannelProvider.Configuration;
using Transmitly.Exceptions;
using Transmitly.Verification;
using Transmitly.Verification.Configuration;

namespace Transmitly
{
    public sealed class DefaultChannelVerificationCommunicationsClient(IChannelVerificationConfiguration channelVerificationConfiguration, IChannelProviderFactory channelProviderFactory) : IChannelVerificationCommunicationsClient
    {
        private readonly IChannelProviderFactory _channelProviderFactory = Guard.AgainstNull(channelProviderFactory);
        private readonly IChannelVerificationConfiguration _channelVerificationConfiguration = Guard.AgainstNull(channelVerificationConfiguration);

        private ChannelVerificationContext CreateContext(string identityAddress, string? channelProviderId, string? channelId)
        {
            return new ChannelVerificationContext(
                identityAddress.AsIdentityAddress(),
                channelProviderId,
                channelId,
                _channelVerificationConfiguration
            );
        }

        public async Task<IReadOnlyCollection<IChannelVerificationSupportedResult>> GetChannelVerificationSupportedChannelProvidersAsync()
        {
            var channelProviders = await _channelProviderFactory.GetAllAsync().ConfigureAwait(false);

            return channelProviders
                .SelectMany(m =>
                    m.ChannelVerificationClientRegistrations.Select(x =>
                         (IChannelVerificationSupportedResult)new ChannelVerificationSupportedResult(m.Id, x.SupportedChannelIds)
                    )
                )
                .ToList()
                .AsReadOnly();
        }

        public async Task<IReadOnlyCollection<IStartChannelVerificationResult>> StartChannelVerificationAsync(string identityAddress, string? channelProviderId, string channelId)
        {
            var channelProviders = await _channelProviderFactory.GetAllAsync().ConfigureAwait(false);
            var client = channelProviders
                .Where(f => string.IsNullOrWhiteSpace(channelProviderId) || f.Id.Equals(channelProviderId, StringComparison.OrdinalIgnoreCase))
                .Select(x =>
                    x.ChannelVerificationClientRegistrations
                        .FirstOrDefault(a =>
                            a.SupportedChannelIds.Any(a => a.Equals(channelId, StringComparison.OrdinalIgnoreCase))
                        )
                )
                .FirstOrDefault();

            if (client == null)
                return [new StartChannelVerificationResult(ChannelVerificationStatus.NotSupported, channelProviderId ?? string.Empty, channelId)];

            var instance = Guard.AgainstNull(await _channelProviderFactory.ResolveChannelVerificationClientAsync(client));

            return await instance.StartChannelVerificationAsync(CreateContext(identityAddress, channelProviderId, channelId)).ConfigureAwait(false);
        }

        public Task<IReadOnlyCollection<IStartChannelVerificationResult>> StartChannelVerificationAsync(string identityAddress, string channelId)
        {
            return StartChannelVerificationAsync(identityAddress, null, channelId);
        }

        public async Task<IChannelVerificationValidationResult> CheckChannelVerificationAsync(string identityAddress, string? channelProviderId, string? channelId, string code, string? token = null)
        {
            var channelProviders = await _channelProviderFactory.GetAllAsync().ConfigureAwait(false);
            var client = channelProviders
                .Where(f => string.IsNullOrWhiteSpace(channelProviderId) || f.Id.Equals(channelProviderId, StringComparison.OrdinalIgnoreCase))
                .Select(x =>
                    x.ChannelVerificationClientRegistrations
                        .FirstOrDefault(a =>
                            string.IsNullOrWhiteSpace(channelId) || a.SupportedChannelIds.Any(a => a.Equals(channelId, StringComparison.OrdinalIgnoreCase))
                        )
                )
                .ToList();

            if (client.Count > 1)
                throw new CommunicationsException($"Unexpected number of supported channel provider sender verification clients. Expected=1, Actual={client.Count}");

            var instance = Guard.AgainstNull(await _channelProviderFactory.ResolveChannelVerificationClientAsync(client[0]!).ConfigureAwait(false));

            return await instance.CheckChannelVerificationAsync(CreateContext(identityAddress, channelProviderId, channelId), code, token).ConfigureAwait(false);
        }

        public Task<IChannelVerificationValidationResult> CheckChannelVerificationAsync(string identityAddress, string code, string? token = null)
        {
            return CheckChannelVerificationAsync(identityAddress, null, null, code, token);
        }
    }
}