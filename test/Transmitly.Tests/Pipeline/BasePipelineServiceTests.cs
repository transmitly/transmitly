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
}