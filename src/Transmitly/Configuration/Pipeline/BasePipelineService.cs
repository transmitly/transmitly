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
	public abstract class BasePipelineService(IPipelineFactory pipelineFactory) : IPipelineService
	{
		private readonly IPipelineFactory _pipelineFactory = Guard.AgainstNull(pipelineFactory);

		public virtual async Task<PipelineLookupResult> FindAsync(string pipelineIntent, string? pipelineId, IReadOnlyCollection<string> allowedChannelFilter)
		{
			var pipelines = await _pipelineFactory.GetAsync(pipelineIntent).ConfigureAwait(false);
			if (pipelines.Count == 0)
				return new PipelineLookupResult(Array.Empty<IPipeline>(), [PredefinedCommunicationStatuses.PipelineNotFound]);

			var pipelinesWithDispatchRequirementsDisabled = pipelines
				.Where(p => !p.Configuration.IsDispatchRequirementsAllowed)
				.ToList();

			if (pipelinesWithDispatchRequirementsDisabled.Count > 0 && allowedChannelFilter.Count > 0)
			{
				var results = pipelinesWithDispatchRequirementsDisabled
					.Select(p => new CommunicationsStatus(
						p.Id ?? p.Intent,
						PredefinedCommunicationStatuses.DispatchRequirementsNotAllowed.Detail,
						PredefinedCommunicationStatuses.DispatchRequirementsNotAllowed.Code))
					.ToList().AsReadOnly();
				return new PipelineLookupResult(Array.Empty<IPipeline>(), results);
			}

			return new PipelineLookupResult(pipelines, Array.Empty<CommunicationsStatus>());
		}
	}
}