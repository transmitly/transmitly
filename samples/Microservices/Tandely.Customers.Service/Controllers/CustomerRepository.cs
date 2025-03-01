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

using Bogus;

namespace Tandely.Customers.Service.Controllers
{
    public sealed class CustomerRepository
    {
        private readonly Dictionary<string, Customer> _identities = [];

        public CustomerRepository()
        {
            Randomizer.Seed = new Random(12345670);
            var faker = new UserFaker();

            for (var i = 0; i < 10; i++)
            {
                var user = faker.Generate();
                _identities.Add(user.Id.ToString(), user);
            }
        }

        internal IReadOnlyCollection<Customer> GetAllUsers()
        {
            return _identities.Values.Cast<Customer>().ToList();
        }

        internal Customer? GetCustomer(Guid id)
        {
            return _identities.TryGetValue(id.ToString(), out var user) ? user : null;
        }
    }
}