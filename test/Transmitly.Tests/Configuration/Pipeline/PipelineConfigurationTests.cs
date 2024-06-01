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

using Moq;
using Transmitly.Channel.Configuration;
using Transmitly.Delivery;

namespace Transmitly.Pipeline.Configuration.Tests
{
	[TestClass]
	public class DefaultPipelineProviderConfigurationTests
	{
		[TestMethod]
		public void AddChannel_Should_AddChannelToList()
		{

			var configuration = new DefaultPipelineProviderConfiguration();
			var channel = new Mock<IChannel>();

			configuration.AddChannel(channel.Object);

			Assert.AreEqual(1, configuration.Channels.Count);
			Assert.AreEqual(channel.Object, configuration.Channels.First());
		}

		[TestMethod]
		public void BlindCopy_Should_AddIdentityAddressToList()
		{
			var configuration = new DefaultPipelineProviderConfiguration();

			configuration.BlindCopyIdentityAddress("Identity1", "Identity2");

			Assert.AreEqual(2, configuration.BlindCopyIdentityAddresses.Count);
			Assert.IsTrue(configuration.BlindCopyIdentityAddresses.Contains("Identity1"));
			Assert.IsTrue(configuration.BlindCopyIdentityAddresses.Contains("Identity2"));
		}

		[TestMethod]
		public void UseChannelSendingStrategy_Should_SetChannelSendingStrategyProvider()
		{
			var configuration = new DefaultPipelineProviderConfiguration();
			var sendingStrategy = new Mock<BasePipelineDeliveryStrategyProvider>() { CallBase = true };

			configuration.UsePipelineDeliveryStrategy(sendingStrategy.Object);

			Assert.AreEqual(sendingStrategy.Object, configuration.PipelineDeliveryStrategyProvider);
		}

		[TestMethod]
		public void UseChannelSendingStrategy_Should_HaveDefaultChannelSendingStrategyProvider()
		{
			var configuration = new DefaultPipelineProviderConfiguration();

			Assert.IsNotNull(configuration.PipelineDeliveryStrategyProvider);
			Assert.IsInstanceOfType(configuration.PipelineDeliveryStrategyProvider, typeof(FirstMatchPipelineDeliveryStrategy));
		}

		[TestMethod]
		public void Copy_Should_AddIdentityAddressToList()
		{
			var configuration = new DefaultPipelineProviderConfiguration();

			configuration.CopyIdentityAddress("Identity1", "Identity2");

			Assert.AreEqual(2, configuration.CopyIdentityAddresses.Count);
			Assert.IsTrue(configuration.CopyIdentityAddresses.Contains("Identity1"));
			Assert.IsTrue(configuration.CopyIdentityAddresses.Contains("Identity2"));
		}
	}
}