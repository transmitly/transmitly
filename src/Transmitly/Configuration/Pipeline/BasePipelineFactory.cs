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

namespace Transmitly.Pipeline.Configuration
{
	public abstract class BasePipelineFactory(IEnumerable<IPipeline> pipelines) : IPipelineFactory
	{
		private readonly List<IPipeline> _pipelines = Guard.AgainstNull(pipelines).ToList();
		protected IReadOnlyCollection<IPipeline> Registrations => _pipelines.AsReadOnly();
		///<inheritdoc/>
		public virtual Task<IReadOnlyCollection<IPipeline>> GetAllAsync()
		{
			return Task.FromResult<IReadOnlyCollection<IPipeline>>(_pipelines);
		}
		///<inheritdoc/>
		public virtual Task<IReadOnlyCollection<IPipeline>> GetAsync(string pipelineName, string platformIdentityType){
			return Task.FromResult<IReadOnlyCollection<IPipeline>>(
				_pipelines
				.Where(x => x.PlatformIdentityType == platformIdentityType && pipelineName == x.PipelineName)
				.ToList()
			);
		}
		///<inheritdoc/>
		public virtual Task<IPipeline?> GetAsync(string pipelineName){
			return Task.FromResult<IPipeline?>(
				_pipelines
				.Find(x => x.PipelineName == pipelineName)
			);
		}
		///<inheritdoc/>
		public virtual Task<IReadOnlyCollection<IPipeline>> GetByPlatformIdentityTypeAsync(string platformIdentityType)
		{
			return Task.FromResult<IReadOnlyCollection<IPipeline>>(
				_pipelines
				.Where(x => x.PlatformIdentityType == platformIdentityType)
				.ToList()
			);
		}
	}
}