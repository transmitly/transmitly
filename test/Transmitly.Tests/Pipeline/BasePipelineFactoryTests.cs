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

using Transmitly.Tests.Mocks;

namespace Transmitly.Pipeline.Configuration.Tests
{
	[TestClass]
	public class BasePipelineFactoryTests
	{
		[TestMethod]
		public async Task GetAllAsyncReturnsAllPipelines()
		{
			var pipelines = new List<IPipeline>
			{
				new MockPipeline { Intent = "A", Configuration = new MockPipelineConfiguration() },
				new MockPipeline { Intent = "B", Configuration = new MockPipelineConfiguration() }
			};
			var factory = new MockPipelineFactory(pipelines);

			var result = await factory.GetAllAsync();

			Assert.AreEqual(2, result.Count);
			Assert.IsTrue(result.Any(p => p.Intent == "A"));
			Assert.IsTrue(result.Any(p => p.Intent == "B"));
		}

		[TestMethod]
		public async Task GetAsyncByIntentReturnsMatchingPipelines()
		{
			var pipelines = new List<IPipeline>
			{
				new MockPipeline { Intent = "A", Configuration = new MockPipelineConfiguration() },
				new MockPipeline { Intent = "B", Configuration = new MockPipelineConfiguration() }
			};
			var factory = new MockPipelineFactory(pipelines);

			var result = await factory.GetAsync("A");

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("A", result.First().Intent);
		}

		[TestMethod]
		public async Task GetAsyncByIntentAndIdReturnsMatchingPipeline()
		{
			var pipelines = new List<IPipeline>
			{
				new MockPipeline { Intent = "A", Id = "1", Configuration = new MockPipelineConfiguration() },
				new MockPipeline { Intent = "A", Id = "2", Configuration = new MockPipelineConfiguration() }
			};
			var factory = new MockPipelineFactory(pipelines);

			var result = await factory.GetAsync("A", "2");

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("2", result.First().Id);
		}

		[TestMethod]
		public async Task GetAsyncWithNullOrEmptyPipelineIdReturnsAllMatchingIntent()
		{
			var pipelines = new List<IPipeline>
			{
				new MockPipeline { Intent = "A", Id = "1", Configuration = new MockPipelineConfiguration() },
				new MockPipeline { Intent = "A", Id = "2", Configuration = new MockPipelineConfiguration() }
			};
			var factory = new MockPipelineFactory(pipelines);

			var result1 = await factory.GetAsync("A", null);
			var result2 = await factory.GetAsync("A", "");

			Assert.AreEqual(2, result1.Count);
			Assert.AreEqual(2, result2.Count);
		}

		[TestMethod]
		[DataRow("  ")]
		[DataRow(null)]
		public async Task GetAsyncWithNullOrEmptyIntentThrows(string? value)
		{
			var factory = new MockPipelineFactory(new List<IPipeline>());
			await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => factory.GetAsync(value!));
		}
	}
}