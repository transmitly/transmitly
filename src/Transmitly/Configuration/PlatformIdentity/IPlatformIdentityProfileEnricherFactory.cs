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

namespace Transmitly.PlatformIdentity.Configuration;

/// <summary>
/// Represents a store for registering platform identity profile enrichers.
/// </summary>
public interface IPlatformIdentityProfileEnricherFactory
{
	/// <summary>
	/// Gets all the registered platform identity profile enrichers.
	/// </summary>
	/// <returns>A read-only list of registered enrichers.</returns>
	Task<IReadOnlyList<IPlatformIdentityProfileEnricherRegistration>> GetAllEnrichersAsync();

	/// <summary>
	/// Gets registered profile enrichers based on the specified platform identity types.
	/// </summary>
	/// <param name="platformIdentityTypes">The platform identity types.</param>
	/// <returns>A read-only list of matching enrichers.</returns>
	Task<IReadOnlyList<IPlatformIdentityProfileEnricherRegistration>> GetPlatformIdentityProfileEnrichersByTypes(params string[] platformIdentityTypes);

	/// <summary>
	/// Resolves the registered platform identity profile enricher.
	/// </summary>
	/// <param name="platformIdentityProfileEnricherRegistration">Registration to instantiate.</param>
	/// <returns>Enricher if found; otherwise, null.</returns>
	Task<IPlatformIdentityProfileEnricher?> GetPlatformIdentityProfileEnricher(IPlatformIdentityProfileEnricherRegistration platformIdentityProfileEnricherRegistration);
}

