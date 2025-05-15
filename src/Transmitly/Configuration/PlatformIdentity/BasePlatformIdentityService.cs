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

using Transmitly.Exceptions;

namespace Transmitly.PlatformIdentity.Configuration
{
	public abstract class BasePlatformIdentityService(IPlatformIdentityResolverFactory identityResolverFactory) : IPlatformIdentityService
	{
		private readonly IPlatformIdentityResolverFactory _platformIdentityResolverRegistrations = Guard.AgainstNull(identityResolverFactory);

		public virtual async Task<IReadOnlyCollection<IPlatformIdentityProfile>> ResolveIdentityProfilesAsync(IReadOnlyCollection<IPlatformIdentityReference> identityReferences)
		{
			var uniqueTypes = Guard.AgainstNullOrEmpty(identityReferences?.ToList()).Select(s => s.Type).Distinct().ToArray();

			var resolvers = await _platformIdentityResolverRegistrations.GetPlatformIdentityResolversByTypes(uniqueTypes).ConfigureAwait(false);

			List<IPlatformIdentityProfile> results = [];

			foreach (var resolver in resolvers)
			{
				var resolverInstance = await _platformIdentityResolverRegistrations.GetPlatformIdentityResolver(resolver).ConfigureAwait(false)
					?? throw new CommunicationsException("Unable to get an instance of platform identity resolver");

				var filteredByTypeReferences = identityReferences
					.Where(x =>
						string.IsNullOrEmpty(resolver.PlatformIdentityType) ||
						string.Equals(resolver.PlatformIdentityType, x.Type, StringComparison.OrdinalIgnoreCase)
					)
					.ToList()
					.AsReadOnly();

				var resolvedIdentities = await resolverInstance.ResolveIdentityProfiles(filteredByTypeReferences).ConfigureAwait(false);

				if (resolvedIdentities != null)
					results.AddRange(resolvedIdentities);
			}

			return results;
		}
	}
}