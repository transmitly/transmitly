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
using System.Text.Json;
using Transmitly.ChannelProvider.ProviderResponse;

namespace Transmitly.KitchenSink.AspNetCoreWebApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public partial class CommunicationsController(ICommunicationsClient communicationsClient) : ControllerBase
	{
		private readonly ICommunicationsClient _communicationsClient = Guard.AgainstNull(communicationsClient);

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
			var allowedChannels = otpCodeVM.CommunicationPreferences ?? [];
			var result = await _communicationsClient.DispatchAsync(
				PipelineName.OtpCode,
				[otpCodeVM.Recipient],
				ContentModel.Create(new { code = otpCodeVM.Code }),
				allowedChannels: allowedChannels);

			return GetActionResult(result);

		}

		[HttpPost("dispatch/sendgrid-template", Name = "SendGridTemplate")]
		[ProducesResponseType(200, Type = typeof(IDispatchCommunicationResult))]
		[ProducesResponseType(500, Type = typeof(IDispatchCommunicationResult))]
		public async Task<IActionResult> DispatchSendGridTemplateMessage(SendGridTemplateVM templateVM)
		{
			var result = await _communicationsClient.DispatchAsync(
				PipelineName.OtpCode,
				[templateVM.Recipient],
				templateVM.Model);

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
					dispatchVM.ContentModel,
					dispatchVM.AllowedChannelIds,
					dispatchVM.Culture,
					cancellationToken
			);

			return GetActionResult(result);
		}

		[HttpPost("callback/status", Name = "CallbackStatus")]
		public IActionResult ReceiveStatus(IReadOnlyCollection<ChannelProviderReport> providerReport)
		{
			Console.WriteLine("[Incoming] {0}", JsonSerializer.Serialize(providerReport, new JsonSerializerOptions { WriteIndented = true }));
			return Ok();
		}
	}
}