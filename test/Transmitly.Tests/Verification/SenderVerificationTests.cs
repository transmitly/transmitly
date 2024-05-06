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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transmitly.Tests.Verification
{
	[TestClass]
	public class SenderVerificationTests
	{
		[TestMethod]
		public async Task SenderVerificationWithFirstSucessfulChannelProvider()
		{
			//var commClient = new Mock<ICommunicationsClient>();
			//var client = commClient.Object;
			//var verificationProviders = await client.GetSenderVerificationSupportedChannelProvidersAsync();
			//var sender = "123456789";

			//foreach (var provider in verificationProviders)
			//{
			//	//are we validated with ANY channel or channel provider
			//	var isValidated = await client.IsSenderVerifiedAsync(sender);
				
			//	// null = no provider supports checking or unknown
			//	// true = validated
			//	// false = not validated
			//	if (isValidated.HasValue && !isValidated.Value)
			//	{
			//		var result = await client.InitiateSenderVerificationAsync(sender, provider.ChannelProviderId, provider.ChannelId);
			//		if (result.IsSuccessful)
			//		{
			//			result.Code;
			//			break;
			//		}
			//	}
			//}
		}
	}
}
