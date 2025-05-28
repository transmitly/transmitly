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

namespace Transmitly.Pipeline.Configuration.Tests;

[TestClass]
public class PipelineFactoryTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	private DefaultPipelineFactory _pipelineRegistrationStore;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	[TestInitialize]
	public void Initialize()
	{
		var pipelineRegistrations = new List<IPipeline>();
		_pipelineRegistrationStore = new DefaultPipelineFactory(pipelineRegistrations);
	}

	[TestMethod]
	public async Task GetAllAsync_ReturnsAllPipelineRegistrations()
	{
		// Arrange

		// Act
		var result = await _pipelineRegistrationStore.GetAllAsync();

		// Assert
		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public async Task GetAsync_ReturnsPipelineRegistrationByName()
	{
		// Arrange
		var pipelineName = "example";

		// Act
		var result = await _pipelineRegistrationStore.GetAsync(pipelineName);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Count);
	}
}