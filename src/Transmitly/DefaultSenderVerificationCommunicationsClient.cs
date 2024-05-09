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

		private SenderVerificationContext CreateContext(string audienceAddress, string? channelProviderId, string? channelId)
		{
			return new SenderVerificationContext(
				audienceAddress.AsAudienceAddress(),
				channelProviderId,
				channelId,
				_senderVerificationConfiguration
			);
		}

		public async Task<IReadOnlyCollection<ISenderVerificationStatusResult>> GetSenderVerificationStatusAsync(string audienceAddress, string? channelProviderId = null, string? channelId = null)
		{
			List<ISenderVerificationStatusResult> results = [];


			if (_senderVerificationConfiguration?.OnIsSenderVerified != null)
			{
				results.Add(await _senderVerificationConfiguration.OnIsSenderVerified(CreateContext(audienceAddress, channelProviderId, channelId)));
			}
			var channelProviders = await _channelProviderFactory.GetAllAsync().ConfigureAwait(false);
			var clientRegistrations = channelProviders
			.Where(f =>
				string.IsNullOrEmpty(channelProviderId) ||
				f.Id.Equals(channelProviderId, StringComparison.OrdinalIgnoreCase)
			)
			.Select(x =>
				new
				{
					Registration = x.SenderVerificationClientRegistrations
					.FirstOrDefault(a =>
					string.IsNullOrEmpty(channelId) ||
						a.SupportedChannelIds.Any(a => a.Equals(channelId, StringComparison.OrdinalIgnoreCase))
					),
					ChannelProviderId = x.Id
				}
			)
			.Where(x => x != null)
			.ToList();

			foreach (var registration in clientRegistrations)
			{
				foreach (var channel in registration.Registration!.SupportedChannelIds)
				{
					var client = await _channelProviderFactory.ResolveSenderVerificationClientAsync(registration.Registration);
					results.Add(await client.IsSenderVerified(CreateContext(audienceAddress, registration.ChannelProviderId, channelId)));
				}
			}
			return results;
		}

		public async Task<bool?> IsSenderVerifiedAsync(string audienceAddress, string? channelProviderId = null, string? channelId = null)
		{
			bool? result = null;

			if (_senderVerificationConfiguration?.OnIsSenderVerified != null)
			{
				var configResult = await _senderVerificationConfiguration.OnIsSenderVerified(CreateContext(audienceAddress, channelProviderId, channelId));
				result = configResult.IsVerified;
			}

			if (result == null)
			{
				var channelProviders = await _channelProviderFactory.GetAllAsync().ConfigureAwait(false);
				var clientRegistrations = channelProviders
				.Where(f =>
					string.IsNullOrEmpty(channelProviderId) ||
					f.Id.Equals(channelProviderId, StringComparison.OrdinalIgnoreCase)
				)
				.Select(x =>
					new
					{
						Registration =
						x.SenderVerificationClientRegistrations
							.FirstOrDefault(a =>
							string.IsNullOrEmpty(channelId) ||
								a.SupportedChannelIds.Any(a => a.Equals(channelId, StringComparison.OrdinalIgnoreCase))
							),
						ChannelProviderId = x.Id
					}
				)
				.Where(x => x != null)
				.ToList();

				foreach (var registration in clientRegistrations)
				{
					var client = await _channelProviderFactory.ResolveSenderVerificationClientAsync(registration.Registration!);
					foreach (var channel in registration.Registration!.SupportedChannelIds)
					{
						var status = await client.IsSenderVerified(CreateContext(audienceAddress, registration.ChannelProviderId, channel));
						result = status.IsVerified;
						if (result != null)
							return result;
					}
				}
			}
			return result;
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

		public async Task<IReadOnlyCollection<IInitiateSenderVerificationResult>> InitiateSenderVerificationAsync(string audienceAddress, string channelProviderId, string channelId)
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

			return await instance.InitiateSenderVerification(CreateContext(audienceAddress, channelProviderId, channelId)).ConfigureAwait(false);
		}

		public async Task<ISenderVerificationValidationResult> ValidateSenderVerificationAsync(string audienceAddress, string channelProviderId, string channelId, string code, string? token = null)
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

			var instance = await _channelProviderFactory.ResolveSenderVerificationClientAsync(client[0]!).ConfigureAwait(false);

			return await instance.ValidateSenderVerification(CreateContext(audienceAddress, channelProviderId, channelId), code, token).ConfigureAwait(false);
		}
	}
}