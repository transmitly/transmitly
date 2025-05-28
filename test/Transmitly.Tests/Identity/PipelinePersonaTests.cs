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

using Transmitly.Tests.Integration;

namespace Transmitly.Tests.Identity;

[TestClass]
public class PipelinePersonaTests
{


	[TestMethod]
	public async Task PipelineShouldOnlyFireForMatchingPersonas()
	{
		const string FromAddress = "unit-test-address-from";
		const string PipelineName = "unit-test-pipeline";
		const string ChannelProviderId = "unit-test-channel-provider";
		const string ExpectedMessage = "Your OTP Code: {{Code}}";
		const string ChannelId = "unit-test-channel";
		const string ExpectedPersona = "Unit Test Persona";
		const string PlatformIdentityType = "test-identity";

		var builder = new CommunicationsClientBuilder()
				.ChannelProvider.Add<OptionalConfigurationTestChannelProviderDispatcher, UnitTestCommunication>(
				ChannelProviderId,
				ChannelId, ChannelId
			 ).
			AddPipeline(PipelineName, options =>
			{
				var channel = new UnitTestChannel(FromAddress, ChannelId, ChannelProviderId);
				channel.Configuration.Subject.AddStringTemplate(ExpectedMessage);
				options.AddChannel(channel);
				options.AddPersonaFilter(ExpectedPersona);
			})
			.AddPersona<MockPlatformIdentity1>(ExpectedPersona, PlatformIdentityType, x => x.IsPersona)
			.AddPersona<MockPlatformIdentity1>("OtherPersona", PlatformIdentityType, x => !x.IsPersona)
			.AddPlatformIdentityResolver<MockPlatformIdentityRepository>();

		var client = builder.BuildClient();

		var result = await client.DispatchAsync(PipelineName, [new IdentityReference(PlatformIdentityType, Guid.NewGuid().ToString()), new IdentityReference(PlatformIdentityType, Guid.NewGuid().ToString())], TransactionModel.Create(new { }));

		Assert.IsNotNull(result);
		Assert.IsTrue(result.IsSuccessful);
		
		Assert.AreEqual(1, result.Results.Count);
		var singleResult = result.Results.First();
		Assert.IsTrue(singleResult?.Status.IsSuccess());
	}
}
