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

namespace Transmitly.PlatformIdentity.Configuration;

public abstract class BasePlatformIdentityProfileEnricherService(IPlatformIdentityProfileEnricherFactory identityProfileEnricherFactory) : IPlatformIdentityProfileEnricherService
{
	private readonly IPlatformIdentityProfileEnricherFactory _platformIdentityProfileEnrichers = Guard.AgainstNull(identityProfileEnricherFactory);

	public virtual async Task EnrichIdentityProfilesAsync(IReadOnlyCollection<IPlatformIdentityProfile> identityProfiles)
	{
		foreach (var profile in Guard.AgainstNull(identityProfiles))
		{
			IReadOnlyList<IPlatformIdentityProfileEnricherRegistration> enrichers;
			if (profile.Type is { Length: > 0 } profileType)
			{
				enrichers = await _platformIdentityProfileEnrichers.GetPlatformIdentityProfileEnrichersByTypes(profileType).ConfigureAwait(false);
			}
			else
			{
				enrichers = await _platformIdentityProfileEnrichers.GetPlatformIdentityProfileEnrichersByTypes().ConfigureAwait(false);
			}

			foreach (var enricher in enrichers)
			{
				var enricherInstance = await _platformIdentityProfileEnrichers.GetPlatformIdentityProfileEnricher(enricher).ConfigureAwait(false)
					?? throw new CommunicationsException("Unable to get an instance of platform identity profile enricher");

				await enricherInstance.EnrichIdentityProfileAsync(profile).ConfigureAwait(false);
			}
		}
	}
}