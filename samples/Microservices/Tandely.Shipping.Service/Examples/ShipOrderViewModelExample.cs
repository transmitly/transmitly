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

namespace Tandely.Shipping.Service.Controllers
{
	public class ShipOrderViewModelExample : IExamplesProvider<ShipOrderViewModel>
	{
		readonly Randomizer _random = new Randomizer(12345670);

		public ShipOrderViewModel GetExamples()
		{
			return new ShipOrderViewModel
			{
				OrderId = _random.Number(100000, 999999).ToString(),
				TrackingNumber = _random.AlphaNumeric(25),
				Customer = new IdentityReference("Customer", "f1ae5bd5-c18d-a9eb-bae5-8aba36d2d1eb"),
				Carrier = new Faker().PickRandomParam(["UPS", "FedEx", "USPS"])
			};
		}
	}
}
