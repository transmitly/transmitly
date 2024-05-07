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
	sealed class DefaultSenderVerificationCommunicationsClient(ISenderVerificationConfiguration senderVerificationConfiguration, IChannelProviderFactory channelProviderFactory) : ISenderVerificationCommunicationsClient
	{
		private readonly IChannelProviderFactory _channelProviderFactory = Guard.AgainstNull(channelProviderFactory);
		private readonly ISenderVerificationConfiguration _senderVerificationConfiguration = Guard.AgainstNull(senderVerificationConfiguration);
		public Task<IReadOnlyCollection<ISenderVerificationStatus>> GetSenderVerificationStatusAsync(string audienceAddress, string? channelProviderId = null, string? channelId = null)
		{
			var config = _senderVerificationConfiguration;
			throw new NotImplementedException();
		}

		public Task<bool?> IsSenderVerifiedAsync(string audienceAddress, string? channelProviderId = null, string? channelId = null)
		{
			throw new NotImplementedException();
		}

		public async Task<IReadOnlyCollection<ISenderVerificationSupportedResult>> GetSenderVerificationSupportedChannelProvidersAsync()
		{
			var channelProviders = await _channelProviderFactory.GetAllAsync().ConfigureAwait(false);

			return channelProviders
				.SelectMany(m =>
					m.SenderVerificationClientRegistrations.Select(x =>
						 (ISenderVerificationSupportedResult)new SenderVerificationSupportedResult(x.IsRequired, m.Id, x.SupportedChannelIds)
					)
				)
				.ToList()
				.AsReadOnly();
		}

		public async Task<IInitiateSenderVerificationResult> InitiateSenderVerificationAsync(string audienceAddress, string channelProviderId, string channelId)
		{
			var channelProviders = await _channelProviderFactory.GetAllAsync().ConfigureAwait(false);
			var client = channelProviders
				.Where(f => f.Id.Equals(channelProviderId, StringComparison.OrdinalIgnoreCase))
				.Select(x =>
					x.SenderVerificationClientRegistrations
						.FirstOrDefault(a =>
							a.SupportedChannelIds.Any(a => a.Equals(channelId, StringComparison.OrdinalIgnoreCase))
						)
				)
				.ToList();
			if (client.Count != 1)
				throw new CommunicationsException($"Unexpected number of supported channel provider sender verification clients. Expected=1, Actual={client.Count}");

			var firstClient = Guard.AgainstNull(client[0]);
			var instance = await _channelProviderFactory.ResolveSenderVerificationClientAsync(firstClient);

			return await instance.InitiateSenderVerification(new SenderVerificationContext(audienceAddress.AsAudienceAddress(), channelProviderId, channelId)).ConfigureAwait(false);
		}

		public async Task<ISenderVerificationValidationResult> ValidateSenderVerificationAsync(string audienceAddress, string channelProviderId, string channelId, string code, string? nonce = null)
		{
			var channelProviders = await _channelProviderFactory.GetAllAsync().ConfigureAwait(false);
			var client = channelProviders
				.Where(f => f.Id.Equals(channelProviderId, StringComparison.OrdinalIgnoreCase))
				.Select(x =>
					x.SenderVerificationClientRegistrations
						.FirstOrDefault(a =>
							a.SupportedChannelIds.Any(a => a.Equals(channelId, StringComparison.OrdinalIgnoreCase))
						)
				)
				.ToList();
			if (client.Count != 1)
				throw new CommunicationsException($"Unexpected number of supported channel provider sender verification clients. Expected=1, Actual={client.Count}");

			var instance = await _channelProviderFactory.ResolveSenderVerificationClientAsync(client[0]!);

			return await instance.ValidateSenderVerification(new SenderVerificationContext(audienceAddress.AsAudienceAddress(), channelProviderId, channelId), code, nonce).ConfigureAwait(false);
		}
	}
}