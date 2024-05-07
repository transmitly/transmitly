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


using Transmitly.Exceptions;

namespace Transmitly.Verification.Configuration
{
	public sealed class EmptySenderVerificationCommunicationsClient : ISenderVerificationCommunicationsClient
	{
		private readonly string Message = $"You must call {nameof(CommunicationsClientBuilder)}.{nameof(CommunicationsClientBuilder.AddSenderVerificationSupport)} before being able to use sender verification services.";

		public Task<IReadOnlyCollection<ISenderVerificationStatus>> GetSenderVerificationStatusAsync(string audienceAddress, string? channelProviderId = null, string? channelId = null)
		{
			throw new CommunicationsException(Message);
		}

		public Task<IReadOnlyCollection<ISenderVerificationSupportedResult>> GetSenderVerificationSupportedChannelProvidersAsync()
		{
			throw new CommunicationsException(Message);
		}

		public Task<IInitiateSenderVerificationResult> InitiateSenderVerificationAsync(string audienceAddress, string channelProviderId, string channelId)
		{
			throw new CommunicationsException(Message);
		}

		public Task<bool?> IsSenderVerifiedAsync(string audienceAddress, string? channelProviderId = null, string? channelId = null)
		{
			throw new CommunicationsException(Message);
		}

		public Task<ISenderVerificationValidationResult> ValidateSenderVerificationAsync(string audienceAddress, string channelProviderId, string channelId, string code, string? nonce = null)
		{
			throw new CommunicationsException(Message);
		}
	}
}