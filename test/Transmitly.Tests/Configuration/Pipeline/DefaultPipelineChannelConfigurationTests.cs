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

using System.Collections.Generic;
using Moq;
using Transmitly.Channel.Configuration;
using Transmitly.Delivery;
using Transmitly.Pipeline.Configuration;

namespace Transmitly.Tests.Configuration.Pipeline
{
	[TestClass]
	public class DefaultPipelineChannelConfigurationTests
	{
		private Mock<IPipelineConfiguration> _mockPipelineConfiguration;
		private Mock<IChannel> _mockChannel;
		private DefaultPipelineChannelConfiguration _configuration;

		[TestInitialize]
		public void Setup()
		{
			_mockPipelineConfiguration = new Mock<IPipelineConfiguration>();
			_mockChannel = new Mock<IChannel>();
			_mockChannel.SetupAllProperties();
			_mockPipelineConfiguration.SetupAllProperties();
			_configuration = new DefaultPipelineChannelConfiguration(_mockPipelineConfiguration.Object, _mockChannel.Object);
		}

		[TestMethod]
		public void ChannelRegistrationsShouldReturnPipelineChannelRegistrations()
		{
			var expected = new List<IChannelRegistration>();
			_mockPipelineConfiguration.Setup(p => p.ChannelRegistrations).Returns(expected);

			var result = _configuration.ChannelRegistrations;

			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void PersonaFiltersShouldReturnPipelinePersonaFilters()
		{
			var expected = new List<string>();
			_mockPipelineConfiguration.Setup(p => p.PersonaFilters).Returns(expected);

			var result = _configuration.PersonaFilters;

			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void PipelineDeliveryStrategyProviderShouldReturnPipelineDeliveryStrategyProvider()
		{
			var expected = new Mock<BasePipelineDeliveryStrategyProvider>().Object;
			_mockPipelineConfiguration.Setup(p => p.PipelineDeliveryStrategyProvider).Returns(expected);

			var result = _configuration.PipelineDeliveryStrategyProvider;

			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TransportPriorityShouldGetAndSetTransportPriority()
		{
			var expected = TransportPriority.High;
			_mockPipelineConfiguration.SetupProperty(p => p.TransportPriority, TransportPriority.Normal);

			_configuration.TransportPriority = expected;
			var result = _configuration.TransportPriority;

			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void MessagePriorityShouldGetAndSetMessagePriority()
		{
			var expected = MessagePriority.High;
			_mockPipelineConfiguration.SetupProperty(p => p.MessagePriority, MessagePriority.Normal);

			_configuration.MessagePriority = expected;
			var result = _configuration.MessagePriority;

			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void AddChannelShouldCallPipelineAddChannel()
		{
			var mockChannel = new Mock<IChannel>().Object;

			_configuration.AddChannel(mockChannel);

			_mockPipelineConfiguration.Verify(p => p.AddChannel(mockChannel), Times.Once);
		}

		[TestMethod]
		public void AddPersonaFilterShouldCallPipelineAddPersonaFilter()
		{
			var personaName = "test-persona";

			_configuration.AddPersonaFilter(personaName);

			_mockPipelineConfiguration.Verify(p => p.AddPersonaFilter(personaName), Times.Once);
		}

		[TestMethod]
		public void AddPlatformIdentityTypeFilterShouldCallPipelineAddPlatformIdentityTypeFilter()
		{
			var platformIdentityType = "test-platform";

			_configuration.AddPlatformIdentityTypeFilter(platformIdentityType);

			_mockPipelineConfiguration.Verify(p => p.AddPlatformIdentityTypeFilter(platformIdentityType), Times.Once);
		}

		[TestMethod]
		public void BlindCopyAddressShouldSetBlindCopyAddress()
		{
			var address = "test@example.com";

			_configuration.BlindCopyAddress(address);

			Assert.AreEqual(address, _configuration.Registration.BlindCopyAddress);
		}

		[TestMethod]
		public void BlindCopyAddressPurposeShouldSetBlindCopyAddressPurpose()
		{
			var purpose = "test-purpose";

			_configuration.BlindCopyAddressPurpose(purpose);

			Assert.AreEqual(purpose, _configuration.Registration.BlindCopyAddressPurpose);
		}

		[TestMethod]
		public void CopyAddressShouldSetBlindCopyAddress()
		{
			var address = "test@example.com";

			_configuration.CopyAddress(address);

			Assert.AreEqual(address, _configuration.Registration.CopyAddress);
		}

		[TestMethod]
		public void CopyAddressPurposeShouldSetBlindCopyAddressPurpose()
		{
			var purpose = "test-purpose";

			_configuration.CopyAddressPurpose(purpose);

			Assert.AreEqual(purpose, _configuration.Registration.CopyAddressPurpose);
		}

		[TestMethod]
		public void IdShouldCallPipelineId()
		{
			var id = "test-id";

			_configuration.Id(id);

			_mockPipelineConfiguration.Verify(p => p.Id(id), Times.Once);
		}

		[TestMethod]
		public void PipelineIdShouldReturnPipelineIdFromPipelineConfiguration()
		{
			var expectedPipelineId = "test-pipeline-id";
			_mockPipelineConfiguration.Setup(p => p.PipelineId).Returns(expectedPipelineId);

			var pipelineId = _configuration.PipelineId;

			Assert.AreEqual(expectedPipelineId, pipelineId);
		}

		[TestMethod]
		public void UsePipelineDeliveryStrategy_ShouldCallPipelineUsePipelineDeliveryStrategy()
		{
			var mockDeliveryStrategyProvider = new Mock<BasePipelineDeliveryStrategyProvider>();

			_configuration.UsePipelineDeliveryStrategy(mockDeliveryStrategyProvider.Object);

			_mockPipelineConfiguration.Verify(p => p.UsePipelineDeliveryStrategy(mockDeliveryStrategyProvider.Object), Times.Once);
		}
	}
}