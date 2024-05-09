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
	/// Manages the ability to verify a specified sender address.
	/// </summary>
	public interface ISenderVerificationCommunicationsClient
	{
		/// <summary>
		/// Initiate the verification process for the provided address.
		/// </summary>
		/// <param name="audienceAddress">Address to verify.</param>
		/// <param name="channelProviderId">Channel provider to verify with.</param>
		/// <param name="channelId">Channel to utilize for verification.</param>
		/// <returns>Verification results.</returns>
		Task<IReadOnlyCollection<IInitiateSenderVerificationResult>> InitiateSenderVerificationAsync(string audienceAddress, string channelProviderId, string channelId);
		/// <summary>
		/// Attempts to verify the provided code with the provided channel provider for the provided channel.
		/// </summary>
		/// <param name="audienceAddress">Address in verification.</param>
		/// <param name="channelProviderId">Channel provider used to send code.</param>
		/// <param name="channelId">Channel used to send code.</param>
		/// <param name="code">Channel provider provided OTP/Code.</param>
		/// <param name="token">Optional nonce or token provided by some channel providers.</param>
		/// <returns>Result of the verification attempt.</returns>
		Task<ISenderVerificationValidationResult> ValidateSenderVerificationAsync(string audienceAddress, string channelProviderId, string channelId, string code, string? token = null);
		/// <summary>
		/// Get the verification status of the provided address across registered channel providers and channels.
		/// </summary>
		/// <param name="audienceAddress">Address to check the validation status of.</param>
		/// <param name="channelProviderId">Optional channel provider to get the status from.</param>
		/// <param name="channelId">Optional channel to get the status for.</param>
		/// <returns>List of verification statues.</returns>
		Task<IReadOnlyCollection<ISenderVerificationStatusResult>> GetSenderVerificationStatusAsync(string audienceAddress, string? channelProviderId = null, string? channelId = null);
		/// <summary>
		/// Gets whether the provided sender address is verified with the registered channel providers and channels.
		/// </summary>
		/// <param name="audienceAddress">Address to check is validated.</param>
		/// <param name="channelProviderId">Optional channel provider filter to check the verification status.</param>
		/// <param name="channelId">Optional channel to check the verification status.</param>
		/// <returns>True, if verified, False if not verified, Null if unknown</returns>
		Task<bool?> IsSenderVerifiedAsync(string audienceAddress, string? channelProviderId = null, string? channelId = null);
		/// <summary>
		/// Retrieves a list of channel providers and channels that support sender verification.
		/// </summary>
		/// <returns>List of channel providers and channels that support sender verification.</returns>
		Task<IReadOnlyCollection<ISenderVerificationSupportedResult>> GetSenderVerificationSupportedChannelProvidersAsync();
	}
}