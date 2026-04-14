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

///<inheritdoc cref="IPlatformIdentityProfileEnricherFactory"/>
public abstract class BasePlatformIdentityProfileEnricherRegistrationFactory(IEnumerable<IPlatformIdentityProfileEnricherRegistration> enrichers, ILoggerFactory loggerFactory) : IPlatformIdentityProfileEnricherFactory
{
	private readonly List<IPlatformIdentityProfileEnricherRegistration> _platformIdentityProfileEnricherRegistrations = [.. Guard.AgainstNull(enrichers)];
	private readonly ILoggerFactory _loggerFactory = Guard.AgainstNull(loggerFactory);
	protected IReadOnlyCollection<IPlatformIdentityProfileEnricherRegistration> Registrations => _platformIdentityProfileEnricherRegistrations.AsReadOnly();

	public virtual Task<IReadOnlyList<IPlatformIdentityProfileEnricherRegistration>> GetAllEnrichersAsync()
	{
		return Task.FromResult<IReadOnlyList<IPlatformIdentityProfileEnricherRegistration>>(
			_platformIdentityProfileEnricherRegistrations
				.OrderBy(x => x.Order)
				.ToList()
				.AsReadOnly()
		);
	}

	public Task<IReadOnlyList<IPlatformIdentityProfileEnricherRegistration>> GetPlatformIdentityProfileEnrichersByTypes(params string[] platformIdentityTypes)
	{
		platformIdentityTypes ??= [];

		return Task.FromResult<IReadOnlyList<IPlatformIdentityProfileEnricherRegistration>>(
			_platformIdentityProfileEnricherRegistrations
				.Where(x => string.IsNullOrEmpty(x.PlatformIdentityType) || platformIdentityTypes.Contains(x.PlatformIdentityType, StringComparer.InvariantCultureIgnoreCase))
				.OrderBy(x => x.Order)
				.ToList()
				.AsReadOnly()
		);
	}

	public virtual Task<IPlatformIdentityProfileEnricher?> GetPlatformIdentityProfileEnricher(IPlatformIdentityProfileEnricherRegistration platformIdentityProfileEnricherRegistration)
	{
		Guard.AgainstNull(platformIdentityProfileEnricherRegistration);

		return Task.FromResult(DefaultActivator.CreateInstance<IPlatformIdentityProfileEnricher>(platformIdentityProfileEnricherRegistration.EnricherType, _loggerFactory));
	}
}

