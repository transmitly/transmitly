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

namespace Tandely.Customers.Service
{
    public class UserFaker : Faker<Customer>
    {
        public UserFaker()
        {
            RuleFor(u => u.Id, f => f.Random.Guid());
            RuleFor(u => u.EmailAddress, f => $"{f.Person.FirstName}{f.Person.LastName[0]}@example.com".ToLowerInvariant());
            RuleFor(u => u.FirstName, f => f.Person.FirstName);
            RuleFor(u => u.LastName, f => f.Person.LastName);
            RuleFor(u => u.MobilePhone, f => f.Random.Bool(0.5f) ? null : f.Phone.PhoneNumber("+1888#######"));
            RuleFor(u => u.DeviceToken, f => f.Random.Bool(0.75f) ? null : $"{f.Random.AlphaNumeric(11)}:APA91b{f.Random.AlphaNumeric(134)}");
            RuleFor(u => u.LoyaltyPoints, f => f.Random.Number(0, 1000));
            RuleFor(u => u.ChannelPreference, (f, u) =>
            {
                var availableChannels = new List<string>();

                if (!string.IsNullOrEmpty(u.EmailAddress))
                    availableChannels.Add("Email");

                if (!string.IsNullOrEmpty(u.MobilePhone))
                    availableChannels.Add("SMS");

                if (!string.IsNullOrEmpty(u.DeviceToken))
                    availableChannels.Add("Push");

                return availableChannels.Count > 0
                    ? f.PickRandom(availableChannels)  // Pick a valid option
                    : "Email"; // Default to Email if nothing is available
            });
        }
    }
}
