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

namespace Transmitly.PlatformIdentity.Configuration
{
    ///<inheritdoc/>
    public abstract class BasePlatformIdentityResolverRegistrationFactory(IEnumerable<IPlatformIdentityResolverRegistration> resolvers) : IPlatformIdentityResolverFactory
    {
        private readonly List<IPlatformIdentityResolverRegistration> _platformIdentityResolverRegistrations = Guard.AgainstNull(resolvers).ToList();
        protected IReadOnlyCollection<IPlatformIdentityResolverRegistration> Registrations => _platformIdentityResolverRegistrations.AsReadOnly();

        ///<inheritdoc/>
        public virtual Task<IReadOnlyList<IPlatformIdentityResolverRegistration>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<IPlatformIdentityResolverRegistration>>(_platformIdentityResolverRegistrations);
        }

        public Task<IReadOnlyList<IPlatformIdentityResolverRegistration>> GetAsync(params string[] platformIdentityTypes)
        {
            return Task.FromResult<IReadOnlyList<IPlatformIdentityResolverRegistration>>(
                _platformIdentityResolverRegistrations
                .Where(x => string.IsNullOrEmpty(x.PlatformIdentityType) || platformIdentityTypes.Contains(x.PlatformIdentityType, StringComparer.InvariantCultureIgnoreCase))
                .ToList()
            );
        }

        public Task<IPlatformIdentityResolver?> ResolveResolver(IPlatformIdentityResolverRegistration platformIdentityResolverRegistration)
        {
            Guard.AgainstNull(platformIdentityResolverRegistration);

            return Task.FromResult(Activator.CreateInstance(platformIdentityResolverRegistration.ResolverType) as IPlatformIdentityResolver);
        }
    }
}