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
using Transmitly.PlatformIdentity.Configuration;
using Bogus;
using Transmitly;

namespace Tandely.Notifications.Service
{
    public class PlatformIdentityRepository : IPlatformIdentityResolver
    {
        public PlatformIdentityRepository()
        {
            Randomizer.Seed = new Random(1234567);
            var faker = new UserFaker();

            for (var i = 0; i < 10; i++)
            {
                var user = faker.Generate();
                _identities.Add(user.Id.ToString(), user);
            }

        }
        
        public IReadOnlyCollection<User> GetAllUsers()
        {
            return _identities.Values.Cast<User>().ToList();
        }

        public Dictionary<string, IPlatformIdentity> _identities = new Dictionary<string, IPlatformIdentity>();

        public Task<IReadOnlyCollection<IPlatformIdentity>?> Resolve(IReadOnlyCollection<IIdentityReference> identityReferences)
        {
            var result = identityReferences
                .SelectMany(reference =>
                    _identities.Values.Where(identity =>
                        (!string.IsNullOrEmpty(reference.Id) && !string.IsNullOrEmpty(reference.Type) && identity.Id == reference.Id && identity.Type == reference.Type) ||
                        (!string.IsNullOrEmpty(reference.Id) && string.IsNullOrEmpty(reference.Type) && identity.Id == reference.Id) ||
                        (string.IsNullOrEmpty(reference.Id) && !string.IsNullOrEmpty(reference.Type) && identity.Type == reference.Type)))
                .ToList();

            return Task.FromResult<IReadOnlyCollection<IPlatformIdentity>?>(result.Any() ? result : null);
        }
    }
}
