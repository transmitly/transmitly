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
using Transmitly.Channel.Configuration;
using Transmitly.Pipeline.Configuration;
using Transmitly.Tests.Mocks;

namespace Transmitly.Tests.Configuration.Pipeline;

[TestClass]
public class BasePipelineServiceTests
{
	[TestMethod]
	public async Task GetMatchingPipelinesAsyncReturnsPipelineNotFoundWhenNoPipelines()
	{
		var mockFactory = new Mock<IPipelineFactory>();
		mockFactory.Setup(f => f.GetAsync(It.IsAny<string>(), It.IsAny<string?>()))
			.ReturnsAsync(new List<IPipeline>());
		var service = new MockPipelineService(mockFactory.Object);

		var result = await service.GetMatchingPipelinesAsync("intent", null, new List<string>());

		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Pipelines.Count);
		Assert.AreEqual(1, result.Errors.Count);
		Assert.AreEqual(PredefinedCommunicationStatuses.PipelineNotFound.Code, result.Errors.First().Code);
	}

	[TestMethod]
	public async Task GetMatchingPipelinesAsyncReturnsDispatchRequirementsNotAllowedWhenDispatchRequirementsDisabledAndFilterPresent()
	{
		var mockConfiguration = new Mock<IPipelineConfiguration>();
		mockConfiguration.SetupAllProperties().Setup(s => s.IsDispatchRequirementsAllowed).Returns(false);
		var mockPipeline = new Mock<IPipeline>();
		mockPipeline.SetupAllProperties().Setup(s => s.Configuration).Returns(mockConfiguration.Object);

		var mockFactory = new Mock<IPipelineFactory>();
		mockFactory.Setup(f => f.GetAsync(It.IsAny<string>(), It.IsAny<string?>()))
			.ReturnsAsync(new List<IPipeline> { mockPipeline.Object });
		var service = new MockPipelineService(mockFactory.Object);

		var result = await service.GetMatchingPipelinesAsync("intent", null, new List<string> { "channel1" });

		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Pipelines.Count);
		Assert.AreEqual(1, result.Errors.Count);
		Assert.AreEqual(PredefinedCommunicationStatuses.DispatchRequirementsNotAllowed.Code, result.Errors.First().Code);
	}

	[TestMethod]
	public async Task GetMatchingPipelinesAsyncReturnsPipelinesWhenDispatchRequirementsAllowed()
	{
		var mockConfiguration = new Mock<IPipelineConfiguration>();
		mockConfiguration.SetupAllProperties().Setup(s => s.IsDispatchRequirementsAllowed).Returns(true);
		var mockChannel = new Mock<IChannel>();
		mockChannel.Setup(c => c.Id).Returns("channel1");
		mockConfiguration.Setup(p => p.Channels).Returns(new List<IChannel> { mockChannel.Object });
		var mockPipeline = new Mock<IPipeline>();
		mockPipeline.SetupAllProperties().Setup(s => s.Configuration).Returns(mockConfiguration.Object);
		var mockFactory = new Mock<IPipelineFactory>();
		mockFactory.Setup(f => f.GetAsync(It.IsAny<string>(), It.IsAny<string?>()))
			.ReturnsAsync(new List<IPipeline> { mockPipeline.Object });
		var service = new MockPipelineService(mockFactory.Object);

		var result = await service.GetMatchingPipelinesAsync("intent", null, new List<string> { "channel1" });

		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Pipelines.Count);
		Assert.AreEqual(0, result.Errors.Count);
		Assert.AreEqual(mockPipeline.Object.Id, result.Pipelines.First().Intent);
	}

	[TestMethod]
	public async Task GetMatchingPipelinesAsync_EmptyAllowedChannelFilter_ReturnsAllPipelines()
	{
		var mockConfiguration = new Mock<IPipelineConfiguration>();
		mockConfiguration.Setup(c => c.IsDispatchRequirementsAllowed).Returns(true);
		var mockSmsChannel = new Mock<IChannel>();
		mockSmsChannel.Setup(c => c.Id).Returns("sms");
		var mockEmailChannel = new Mock<IChannel>();
		mockEmailChannel.Setup(c => c.Id).Returns("email");
		mockConfiguration.Setup(c => c.Channels).Returns(new[] { mockSmsChannel.Object, mockEmailChannel.Object });

		var mockPipeline = new Mock<IPipeline>();
		mockPipeline.Setup(p => p.Configuration).Returns(mockConfiguration.Object);

		var mockFactory = new Mock<IPipelineFactory>();
		mockFactory.Setup(f => f.GetAsync(It.IsAny<string>(), It.IsAny<string?>()))
			.ReturnsAsync(new List<IPipeline> { mockPipeline.Object });

		var service = new MockPipelineService(mockFactory.Object);

		var result = await service.GetMatchingPipelinesAsync("intent", null, Array.Empty<string>());

		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Pipelines.Count);
		Assert.AreEqual(0, result.Errors.Count);
	}

	[TestMethod]
	public async Task GetMatchingPipelinesAsync_AllFilterChannelsExist_ReturnsPipeline()
	{
		// Arrange
		var mockConfiguration = new Mock<IPipelineConfiguration>();
		mockConfiguration.Setup(c => c.IsDispatchRequirementsAllowed).Returns(true);
		var mockSmsChannel = new Mock<IChannel>();
		mockSmsChannel.Setup(c => c.Id).Returns("sms");
		var mockEmailChannel = new Mock<IChannel>();
		mockEmailChannel.Setup(c => c.Id).Returns("email");
		var mockVoiceChannel = new Mock<IChannel>();
		mockVoiceChannel.Setup(c => c.Id).Returns("voice");
		mockConfiguration.Setup(c => c.Channels).Returns(new[] { mockSmsChannel.Object, mockEmailChannel.Object, mockVoiceChannel.Object });

		var mockPipeline = new Mock<IPipeline>();
		mockPipeline.Setup(p => p.Configuration).Returns(mockConfiguration.Object);

		var mockFactory = new Mock<IPipelineFactory>();
		mockFactory.Setup(f => f.GetAsync(It.IsAny<string>(), It.IsAny<string?>()))
			.ReturnsAsync(new List<IPipeline> { mockPipeline.Object });

		var service = new MockPipelineService(mockFactory.Object);

#pragma warning disable CA1861 //Avoid constant arrays as arguments
		var result = await service.GetMatchingPipelinesAsync("intent", null, new[] { "sms", "email" });
#pragma warning restore CA1861 //Avoid constant arrays as arguments
		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Pipelines.Count);
		Assert.AreEqual(0, result.Errors.Count);
	}

	[TestMethod]
	public async Task GetMatchingPipelinesAsync_SomeFilterChannelsMissing_ReturnsDispatchChannelFilterMismatch()
	{
		var mockConfiguration = new Mock<IPipelineConfiguration>();
		mockConfiguration.Setup(c => c.IsDispatchRequirementsAllowed).Returns(true);
		var mockSmsChannel = new Mock<IChannel>();
		mockSmsChannel.Setup(c => c.Id).Returns("sms");
		var mockEmailChannel = new Mock<IChannel>();
		mockEmailChannel.Setup(c => c.Id).Returns("email");
		mockConfiguration.Setup(c => c.Channels).Returns(new[] { mockSmsChannel.Object, mockEmailChannel.Object });

		var mockPipeline = new Mock<IPipeline>();
		mockPipeline.Setup(p => p.Configuration).Returns(mockConfiguration.Object);

		var mockFactory = new Mock<IPipelineFactory>();
		mockFactory.Setup(f => f.GetAsync(It.IsAny<string>(), It.IsAny<string?>()))
			.ReturnsAsync(new List<IPipeline> { mockPipeline.Object });

		var service = new MockPipelineService(mockFactory.Object);

#pragma warning disable CA1861 //Avoid constant arrays as arguments
		var result = await service.GetMatchingPipelinesAsync("intent", null, new[] { "sms", "voice" });
#pragma warning restore CA1861 //Avoid constant arrays as arguments

		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Errors.Count);
		Assert.AreEqual(PredefinedCommunicationStatuses.DispatchChannelFilterMismatch.Code, result.Errors.First().Code);
	}
}