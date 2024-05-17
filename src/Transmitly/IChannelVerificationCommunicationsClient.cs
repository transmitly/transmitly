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

using Transmitly.Verification;

namespace Transmitly
{
	/// <summary>
	/// Manages the ability to verify a specified address.
	/// </summary>
	public interface IChannelVerificationCommunicationsClient
	{
		/// <summary>
		/// Initiate the verification process for the provided address.
		/// </summary>
		/// <param name="audienceAddress">Address to verify.</param>
		/// <param name="channelProviderId">Channel provider to verify with.</param>
		/// <param name="channelId">Channel to utilize for verification.</param>
		/// <returns>Verification results.</returns>
		Task<IReadOnlyCollection<IStartChannelVerificationResult>> StartChannelVerificationAsync(string audienceAddress, string? channelProviderId, string channelId);
        Task<IReadOnlyCollection<IStartChannelVerificationResult>> StartChannelVerificationAsync(string audienceAddress, string channelId);
        /// <summary>
        /// Attempts to verify the provided code with the provided channel provider for the provided channel.
        /// </summary>
        /// <param name="audienceAddress">Address in verification.</param>
        /// <param name="channelProviderId">Channel provider used to send code.</param>
        /// <param name="channelId">Channel used to send code.</param>
        /// <param name="code">Channel provider provided OTP/Code.</param>
        /// <param name="token">Optional nonce or token provided by some channel providers.</param>
        /// <returns>Result of the verification attempt.</returns>
        Task<IChannelVerificationValidationResult> CheckChannelVerificationAsync(string audienceAddress, string? channelProviderId, string? channelId, string code, string? token = null);
        Task<IChannelVerificationValidationResult> CheckChannelVerificationAsync(string audienceAddress, string code, string? token = null);
        
        /// <summary>
        /// Retrieves a list of channel providers and channels that support sender verification.
        /// </summary>
        /// <returns>List of channel providers and channels that support sender verification.</returns>
        Task<IReadOnlyCollection<IChannelVerificationSupportedResult>> GetChannelVerificationSupportedChannelProvidersAsync();
	}
}