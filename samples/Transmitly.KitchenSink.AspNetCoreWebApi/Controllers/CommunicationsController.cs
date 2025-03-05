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
using Transmitly.Delivery;

namespace Transmitly.KitchenSink.AspNetCoreWebApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public partial class CommunicationsController(ICommunicationsClient communicationsClient, ILogger<CommunicationsController> logger) : ControllerBase
	{
		private readonly ICommunicationsClient _communicationsClient = Guard.AgainstNull(communicationsClient);
		private readonly ILogger<CommunicationsController> _logger = Guard.AgainstNull(logger);

		private IActionResult GetActionResult(IDispatchCommunicationResult result)
		{
			if (result.IsSuccessful)
				return Ok(result);
			return BadRequest(result);
		}

		[HttpPost("dispatch/otp", Name = "OTPCode")]
		[ProducesResponseType(200, Type = typeof(IDispatchCommunicationResult))]
		[ProducesResponseType(500, Type = typeof(IDispatchCommunicationResult))]
		public async Task<IActionResult> DispatchOtp(OtpCodeVM otpCodeVM)
		{
			//If our recipient does not have any preference, we'll send it to any matching recipient address
			var preferredChannels = otpCodeVM.CommunicationPreferences ?? [];
			var result = await _communicationsClient.DispatchAsync(
				PipelineName.OtpCode,
				[otpCodeVM.Recipient],
				TransactionModel.Create(new { code = otpCodeVM.Code }),
				channelPreferences: preferredChannels);

			return GetActionResult(result);

		}

		[HttpPost("dispatch", Name = "Dispatch")]
		[ProducesResponseType(200, Type = typeof(IDispatchCommunicationResult))]
		[ProducesResponseType(500, Type = typeof(IDispatchCommunicationResult))]
		public async Task<IActionResult> Dispatch(DispatchVM dispatchVM, CancellationToken cancellationToken)
		{
			var result = await _communicationsClient.DispatchAsync(
					Guard.AgainstNullOrWhiteSpace(dispatchVM.PipelineName),
					dispatchVM.Recipients,
					dispatchVM.TransationModel,
					dispatchVM.AllowedChannelIds,
					dispatchVM.Culture,
					cancellationToken
			);

			return GetActionResult(result);
		}

		[HttpPost("channel/provider/update", Name = "DeliveryReport")]
		public IActionResult ChannelProviderDeliveryReport(ChannelProviderDeliveryReportRequest providerReport)
		{
			_communicationsClient.DeliverReports(providerReport.DeliveryReports);
			return Ok();
		}
	}
}