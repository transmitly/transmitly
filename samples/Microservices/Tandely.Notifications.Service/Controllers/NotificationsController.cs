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
using Transmitly;

namespace Tandely.Notifications.Service.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class NotificationsController(ICommunicationsClient communicationsClient, ILogger<NotificationsController> logger) : ControllerBase
	{
		[HttpPost("dispatch", Name = "DispatchNotification")]
		public async Task<IActionResult> DispatchAsync(DispatchTandelyNotification notification, CancellationToken cancellationToken)
		{
			if (notification == null || string.IsNullOrWhiteSpace(notification.CommunicationId) || notification.TransactionalModel == null)
			{
				return BadRequest();
			}

			logger.LogDebug("Dispatching notification '{CommunicationId}' to {PlatformIdentities}", notification.CommunicationId, notification.PlatformIdentities);

			var result = await communicationsClient.DispatchAsync(
				notification.CommunicationId,
				notification.PlatformIdentities.Cast<IPlatformIdentityReference>().ToList(),
				notification.TransactionalModel,
				cancellationToken: cancellationToken
			);

			var resultStatuses = string.Join(",", result.Results.Select(x => x!.Status.IsSuccess()));

			if (!resultStatuses.Any())
				resultStatuses = "No notifications dispatched";

			logger.LogInformation("Dispatched notification '{CommunicationId}' to {PlatformIdentities} with result(s) {resultStatus}", notification.CommunicationId, notification.PlatformIdentities, resultStatuses);
			if (result.IsSuccessful)
			{
				return Ok(result);
			}

			return BadRequest(result);
		}
	}
}