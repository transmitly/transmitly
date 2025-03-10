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

namespace Transmitly.Pipeline.Configuration
{
	/// <summary>
	/// Represents a store for pipeline registrations.
	/// </summary>
	public interface IPipelineFactory
	{
		/// <summary>
		/// Retrieves all pipelines asynchronously.
		/// </summary>
		/// <returns>A read-only list of pipelines.</returns>
		Task<IReadOnlyCollection<IPipeline>> GetAllAsync();

		/// <summary>
		/// Retrieves a pipeline by pipeline name asynchronously.
		/// </summary>
		/// <param name="communicationIntentId">Id of the intent of the pipeline.</param>
		/// <param name="pipelineName">Name of the pipeline.
		/// <para>Must be unique per <paramref name="communicationIntentId"/></para></param>
		/// <returns>The pipeline, or null if not found.</returns>
		Task<IReadOnlyCollection<IPipeline>> GetAsync(string communicationIntentId, string? pipelineName);
	}

}