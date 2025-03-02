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
using Swashbuckle.AspNetCore.Filters;
using Transmitly;

namespace Tandely.Orders.Service.Controllers
{
    public class CreateOrderViewModelExample : IExamplesProvider<CreateOrderViewModel>
    {
        public CreateOrderViewModel GetExamples()
        {
            return new CreateOrderViewModel
            {
                Id = new Randomizer().Number(100000, 999999).ToString(),
                Date = DateTime.UtcNow,
                Total = 100.21,
                Customers = new List<IdentityReference> {
                    new IdentityReference("Customer", "f96390f7-7175-3847-1df6-43a0eb5f7b60")
                }
            };
        }
    }
}
