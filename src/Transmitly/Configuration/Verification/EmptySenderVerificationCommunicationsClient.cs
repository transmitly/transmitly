﻿// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
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
	public sealed class EmptyChannelVerificationCommunicationsClient : IChannelVerificationCommunicationsClient
	{
		private static readonly string Message = $"You must call {nameof(CommunicationsClientBuilder)}.{nameof(CommunicationsClientBuilder.AddChannelVerificationSupport)} before being able to use sender verification services.";

        public Task<IChannelVerificationValidationResult> CheckChannelVerificationAsync(string identityAddress, string? channelProviderId, string? channelId, string code, string? token = null)
        {
            throw new CommunicationsException(Message);
        }

        public Task<IChannelVerificationValidationResult> CheckChannelVerificationAsync(string identityAddress, string code, string? token = null)
        {
            throw new CommunicationsException(Message);
        }

        public Task<IReadOnlyCollection<IChannelVerificationSupportedResult>> GetChannelVerificationSupportedChannelProvidersAsync()
        {
            throw new CommunicationsException(Message);
        }

        public Task<IReadOnlyCollection<IStartChannelVerificationResult>> StartChannelVerificationAsync(string identityAddress, string? channelProviderId, string channelId)
        {
            throw new CommunicationsException(Message);
        }

        public Task<IReadOnlyCollection<IStartChannelVerificationResult>> StartChannelVerificationAsync(string identityAddress, string channelId)
        {
            throw new CommunicationsException(Message);
        }
    }
}