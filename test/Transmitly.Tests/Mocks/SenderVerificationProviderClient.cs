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

namespace Transmitly.Tests
{
	internal sealed class UnitTestSenderVerificationProviderClient : ISenderVerificationChannelProviderClient
	{
		public UnitTestSenderVerificationProviderClient()
		{

		}
		public static bool InitiateSuccessful { get; set; } = true;
		public static bool? SenderVerified { get; set; } = true;
		public static bool IsValidated { get; set; } = true;
		public static bool IsValidatedSuccessful { get; set; } = true;
		public static string ChannelProviderId { get; set; } = "unit-test-channel-provider";
		public static List<string> AllowedChannelIds { get; set; } = [];
		public static bool ValidateCode { get; set; } = true;
		public static string Code { get; set; } = "012345";
		public Task<IReadOnlyCollection<IInitiateSenderVerificationResult>> InitiateSenderVerification(ISenderVerificationContext senderVerificationContext)
		{
			EnsureChannelAllowed(senderVerificationContext.ChannelId);
			return Task.FromResult<IReadOnlyCollection<IInitiateSenderVerificationResult>>([new InitiateSenderVerificationResult(SenderVerificationStatus.Delivered, ChannelProviderId, senderVerificationContext.ChannelId!, null)]);
		}

		private static void EnsureChannelAllowed(string? channelId)
		{
			if (!(AllowedChannelIds.Count == 0 || Array.Exists(AllowedChannelIds.ToArray(), x => x.Equals(channelId))))
				throw new Exception("ChannelId not allowed.");
		}

		public Task<ISenderVerificationStatusResult> IsSenderVerified(ISenderVerificationContext senderVerificationContext, string? token = null)
		{
			EnsureChannelAllowed(senderVerificationContext.ChannelId);
			return Task.FromResult<ISenderVerificationStatusResult>(new SenderVerifiedStatus(SenderVerified, "unit-test-channel-provider", senderVerificationContext.ChannelId));
		}

		public Task<ISenderVerificationValidationResult> ValidateSenderVerification(ISenderVerificationContext senderVerificationContext, string code, string? nonce = null)
		{
			EnsureChannelAllowed(senderVerificationContext.ChannelId);
			var isValidated = IsValidated;
			if (ValidateCode)
				isValidated = code == Code;
			return Task.FromResult<ISenderVerificationValidationResult>(new SenderVerificationValidationResult(IsValidatedSuccessful, isValidated, ChannelProviderId, senderVerificationContext.ChannelId!, senderVerificationContext.SenderAddress.Value));


		}
	}
}