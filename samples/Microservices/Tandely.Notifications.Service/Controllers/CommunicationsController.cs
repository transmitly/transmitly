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

using Transmitly;
using Microsoft.AspNetCore.Mvc;
using Tandely.IntegrationEvents;

namespace Tandely.Notifications.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommunicationsController : ControllerBase
    {
        private readonly ICommunicationsClient _communicationsClient;
        private readonly ILogger<CommunicationsController> _logger;

        public CommunicationsController(ICommunicationsClient communicationsClient, ILogger<CommunicationsController> logger)
        {
            _communicationsClient = communicationsClient;
            _logger = logger;
        }

        [HttpPost(Name = "DispatchExampleCommunication")]
        public async Task<IActionResult> DispatchAsync(DispatchTandelyNotification dispatchObj, CancellationToken cancellationToken)
        {
            if (dispatchObj == null || string.IsNullOrWhiteSpace(dispatchObj.CommunicationId) || dispatchObj.TransactionalModel == null)
            {
                return BadRequest();
            }

            var result = await _communicationsClient.DispatchAsync(
                dispatchObj.CommunicationId,
                dispatchObj.PlatformIdentities.Cast<IIdentityReference>().ToList(),
                dispatchObj.TransactionalModel,
                cancellationToken: cancellationToken
            );

            if (result.IsSuccessful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
