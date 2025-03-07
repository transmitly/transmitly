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

using Microsoft.AspNetCore.Mvc;
using Tandely.IntegrationEvents;
using Transmitly;

namespace Tandely.Shipping.Service.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ShippingController(ICommunicationsClient communicationsClient, ILogger<ShippingController> logger) : ControllerBase
	{
		[HttpPost("ship", Name = "ShipOrder")]
		public async Task<IActionResult> CreateOrderAsync(ShipOrderViewModel model)
		{
			if (model == null)
				return BadRequest();

			logger.LogDebug("Dispatching ship order {orderId}", model.OrderId);

			var result = await communicationsClient.DispatchAsync(ShippingIntegrationEvent.OrderShipped, [model.Customer], TransactionModel.Create(new
			{
				model.OrderId,
				model.TrackingNumber,
				model.Carrier,
				//TenantId = "tenant-1"
			}));

			logger.LogDebug("{success} Dispatched order confirmation for order {orderId}", result.IsSuccessful ? "Successfully" : "Unsuccessfully", model.OrderId);

			if (result.IsSuccessful)
				return Ok(result.Results?.Select(r => r?.ResourceId));

			return BadRequest(result);
		}
	}
}
