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
			if (notification == null || string.IsNullOrWhiteSpace(notification.CommunicationIntent) || notification.TransactionalModel == null)
			{
				return BadRequest();
			}

			var sanitizedCommunicationId = SanitizeForLog(notification.CommunicationId);
			var sanitizedCommunicationIntent = SanitizeForLog(notification.CommunicationIntent);
			var sanitizedPlatformIdentities = FormatPlatformIdentitiesForLog(notification.PlatformIdentities);

			logger.LogDebug("Dispatching user-supplied notification id '{CommunicationId}' to user-supplied platform identities {PlatformIdentities}", sanitizedCommunicationId, sanitizedPlatformIdentities);

			var result = await communicationsClient.DispatchAsync(
				notification.CommunicationIntent,
				notification.PlatformIdentities.Cast<IPlatformIdentityReference>().ToList(),
				notification.TransactionalModel,
				cancellationToken: cancellationToken
			);

			var response = new
			{
				Results = result.Results.Select(x => new
				{
					ResourceId = x?.ResourceId,
					Status = x == null
						? null
						: new
						{
							x.Status.Code,
							x.Status.Type,
							x.Status.Detail
						},
					ChannelProviderId = x?.ChannelProviderId,
					ChannelId = x?.ChannelId,
					PipelineId = x?.PipelineId,
					PipelineIntent = x?.PipelineIntent,
					Exception = x?.Exception
				}).ToList(),
				result.IsSuccessful
			};

			var resultStatuses = string.Join(",", result.Results.Select(x => x!.Status.IsSuccess()));

			if (!resultStatuses.Any())
				resultStatuses = "No notifications dispatched";

			logger.LogInformation("Dispatched user-supplied notification intent '{CommunicationIntent}' to user-supplied platform identities {PlatformIdentities} with result(s) {ResultStatus}", sanitizedCommunicationIntent, sanitizedPlatformIdentities, resultStatuses);
			if (result.IsSuccessful)
			{
				return Ok(response);
			}

			return BadRequest(response);
		}

		private static string FormatPlatformIdentitiesForLog(IEnumerable<TandelyPlatformIdentity>? platformIdentities)
		{
			if (platformIdentities == null)
				return string.Empty;

			return string.Join(
				", ",
				platformIdentities.Select(identity => $"[{SanitizeForLog(identity?.Type)}:{SanitizeForLog(identity?.Id)}]")
			);
		}

		private static string SanitizeForLog(string? value)
		{
			return string.IsNullOrEmpty(value)
				? string.Empty
				: value.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
		}
	}
}
