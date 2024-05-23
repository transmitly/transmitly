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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transmitly.Tests.Integration;

namespace Transmitly.Tests.Identity
{
    [TestClass]
    public class PlatformIdentityResolverTests
    {
        [TestMethod]
        public async Task MyTestMethod()
        {
            const string FromAddress = "unit-test-address-from";
            const string PipelineName = "unit-test-pipeline";
            const string ChannelProviderId = "unit-test-channel-provider";
            const string ExpectedMessage = "Your OTP Code: {{Code}}";
            const string ChannelId = "unit-test-channel";

            var builder = new CommunicationsClientBuilder()
                .ChannelProvider.Add<OptionalConfigurationTestChannelProviderClient, UnitTestCommunication>(
                ChannelProviderId,
                ChannelId, ChannelId
             ).
            AddPipeline(PipelineName, options =>
            {
                var channel = new UnitTestChannel(FromAddress, ChannelId, ChannelProviderId);
                channel.Subject.AddStringTemplate(ExpectedMessage);
                options.AddChannel(channel);

            }).
             AddPlatformIdentityResolver<TestPlatformIdentityRepository>();

            var client = builder.BuildClient();

            var result = await client.DispatchAsync(PipelineName, [new IdentityReference("test-identity", Guid.NewGuid().ToString())], ContentModel.Create(new { }));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsNotNull(result.Results);
            Assert.AreEqual(1, result.Results.Count);
            var singleResult = result.Results.First();
            Assert.AreEqual(DispatchStatus.Dispatched, singleResult?.DispatchStatus);
        }
    }
}
