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

using System.Collections.ObjectModel;

namespace Transmitly.Pipeline.Configuration;

/// <summary>
/// Default factory for pipeline registrations.
/// </summary>
/// <param name="pipelineRegistrations">The registered pipelines.</param>
/// <exception cref="ArgumentNullException">If the provided pipeline registrations are null</exception>
public sealed class DefaultPipelineFactory(IEnumerable<IPipeline> pipelineRegistrations) : IPipelineFactory
{
	private readonly ReadOnlyCollection<IPipeline> _pipelineRegistrations = Guard.AgainstNull(pipelineRegistrations).ToList().AsReadOnly();

	public Task<IReadOnlyCollection<IPipeline>> GetAllAsync()
	{
		return Task.FromResult<IReadOnlyCollection<IPipeline>>(_pipelineRegistrations);
	}

	public Task<IReadOnlyCollection<IPipeline>> GetAsync(string pipelineIntent, string? pipelineId = null)
	{
		return Task.FromResult<IReadOnlyCollection<IPipeline>>(
			_pipelineRegistrations
			.Where(x =>
				x.Intent == pipelineIntent &&
				(
					string.IsNullOrWhiteSpace(pipelineId) || x.Id == pipelineId
				)
			)
			.ToList()
			.AsReadOnly()
		);
	}
}