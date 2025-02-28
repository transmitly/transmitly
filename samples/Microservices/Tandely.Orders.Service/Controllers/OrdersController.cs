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

namespace Tandely.Orders.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ICommunicationsClient _communicationsClient;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ICommunicationsClient communicationsClient, ILogger<OrdersController> logger)
        {
            _communicationsClient = communicationsClient;
            _logger = logger;
        }

        [HttpPost("create", Name = "ConfirmOrder")]
        public async Task<IActionResult> CreateOrderAsync(CreateOrderViewModel model)
        {
            if (model == null)
                return BadRequest();

            var result = await _communicationsClient.DispatchAsync(OrdersIntegrationEvent.OrderConfirmation, model.Customers, TransactionModel.Create(new
            {
                Order = new
                {
                    model.Id,
                    model.Date,
                    model.Total
                }
            }));

            if (result.IsSuccessful)
                return Ok(result.Results?.Select(r => r?.ResourceId));
            return BadRequest(result);
        }
    }
}
