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

namespace Transmitly.Audience.Configuration
{
	/// <summary>
	/// Represents a store for registering audience resolvers in memory.
	/// </summary>
	internal class InMemoryAudienceResolverRegistrationStore : IAudienceResolverRegistrationStore
	{
		private readonly List<IAudienceResolver> _audienceResolverRegistrations;

		/// <summary>
		/// Initializes a new instance of the <see cref="InMemoryAudienceResolverRegistrationStore"/> class.
		/// </summary>
		/// <param name="audienceResolverRegistrations">The audience resolver registrations.</param>
		internal InMemoryAudienceResolverRegistrationStore(IEnumerable<IAudienceResolver> audienceResolverRegistrations)
		{
			_audienceResolverRegistrations = audienceResolverRegistrations?.ToList() ?? throw new ArgumentNullException(nameof(audienceResolverRegistrations));
		}

		/// <summary>
		/// Gets all the registered audience resolvers.
		/// </summary>
		/// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of audience resolvers.</returns>
		public Task<IReadOnlyList<IAudienceResolver>> GetAllAsync()
		{
			return Task.FromResult<IReadOnlyList<IAudienceResolver>>(_audienceResolverRegistrations);
		}

		/// <summary>
		/// Gets the audience resolvers based on the specified audience type identifier.
		/// </summary>
		/// <param name="audienceTypeIdentifier">The audience type identifier.</param>
		/// <param name="includeGenericResolvers">A flag indicating whether to include generic resolvers.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of audience resolvers.</returns>
		public Task<IReadOnlyList<IAudienceResolver>> GetAsync(string audienceTypeIdentifier, bool includeGenericResolvers = false)
		{
			return Task.FromResult<IReadOnlyList<IAudienceResolver>>(
				_audienceResolverRegistrations
				.Where(x => x.AudienceTypeIdentifier == audienceTypeIdentifier || (includeGenericResolvers && string.IsNullOrWhiteSpace(x.AudienceTypeIdentifier)))
				.ToList()
			);
		}
	}
}