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

using Moq;
using Transmitly.Tests;

namespace Transmitly.Pipeline.Configuration.Tests
{
	[TestClass()]
	public class PipelineModuleTests
	{
		[TestMethod]
		public void ModuleShouldConfigureChannelConfig()
		{
			var config = new DefaultPipelineProviderConfiguration();
			var module = new Mock<PipelineModule>("test-name", "test-category")
			{
				CallBase = true
			};
			var channel = new UnitTestChannel("unit-test-address");
			module.Setup(x => x.Load(It.IsAny<IPipelineChannelConfiguration>())).Callback<IPipelineChannelConfiguration>((config) =>
			{
				Assert.IsNotNull(config);
				config.AddChannel(channel);
				config.MessagePriority = MessagePriority.Highest;
				config.TransportPriority = TransportPriority.Lowest;
			}).Verifiable();

			module.Object.Load(config);

			Assert.AreEqual(1, config.Channels.Count);
			Assert.AreSame(channel, config.Channels.First());
			Assert.AreEqual(MessagePriority.Highest, config.MessagePriority);
			Assert.AreEqual(TransportPriority.Lowest, config.TransportPriority);
		}

		[TestMethod()]
		public async Task ShouldAddModule()
		{
			var module = new Mock<PipelineModule>("test-name", "test-category")
			{
				CallBase = true
			};
			module.Setup(x => x.Load(It.IsAny<IPipelineChannelConfiguration>())).Callback<IPipelineChannelConfiguration>((config) =>
			{
				Assert.IsNotNull(config);
				config.AddChannel(new UnitTestChannel("unit-test-address"));
				config.MessagePriority = MessagePriority.Highest;
				config.TransportPriority = TransportPriority.Lowest;
			}).Verifiable();

			var builder = new CommunicationsClientBuilder().AddChannelProvider<MinimalConfigurationTestChannelProviderClient,object>("test-channel-provider");
			var result = builder.AddPipelineModule(module.Object);
			Assert.IsNotNull(result);
			Assert.AreSame(builder, result);
			var client = result.BuildClient();
			var sentResult = await client.DispatchAsync("test-name", "unit-test-address", new { });
			Assert.AreEqual(1, sentResult.Results.Count);
			Assert.IsTrue(sentResult.Results.All(x => x?.DispatchStatus == DispatchStatus.Delivered));
			var deliveryResult = sentResult.Results.First();
			Assert.AreEqual("unit-test-channel", sentResult.Results?.First()?.ChannelId);
			Assert.AreEqual("test-channel-provider", deliveryResult?.ChannelProviderId);
		}


	}
}