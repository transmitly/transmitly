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
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Transmitly.ChannelProvider.Twilio.Configuration.Sms;
using Transmitly.Delivery;
using Transmitly.Samples.Shared;
using Transmitly.Util;

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
			var preferredChannels = otpCodeVM.CommunicationPreferences ?? [];
			var result = await _communicationsClient.DispatchAsync(
				PipelineName.OtpCode,
				[otpCodeVM.Recipient],
				TransactionModel.Create(new { code = otpCodeVM.Code }),
				dispatchChannelPreferences: preferredChannels);

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
					null,
					dispatchVM.Culture,
					cancellationToken
			);

			return GetActionResult(result);
		}

		[HttpPost("dispatch/order-shipment-update", Name = "DispatchOrderShipmentUpdate")]
		[ProducesResponseType(200, Type = typeof(IDispatchCommunicationResult))]
		[ProducesResponseType(500, Type = typeof(IDispatchCommunicationResult))]
		public async Task<IActionResult> DispatchOrderShipmentUpdate(OrderShipmentUpdateVM request, CancellationToken cancellationToken)
		{
			// This route is intentionally explicit and separate from "dispatch".
			// It demonstrates how profile/content enrichers can complete communication data
			// from read models in other bounded contexts (Customer, Loyalty, Orders, Fulfillment).
			var recipient = new DispatchPlatformIdentityProfile
			{
				Id = request.CustomerId,
				Type = request.CustomerType,
				Addresses = [.. request.Addresses],
				Attributes = new Dictionary<string, string> { ["orderId"] = request.OrderId }
			};

			var result = await _communicationsClient.DispatchAsync(
					PipelineName.OrderShipmentUpdate,
					[recipient],
					TransactionModel.Create(new { orderId = request.OrderId }),
					request.AllowedChannelIds,
					null,
					request.Culture,
					cancellationToken
			);

			return GetActionResult(result);
		}

		// The route real providers (Twilio, Infobip, ...) call with status updates.
		// The model binder turns their webhook into Transmitly delivery reports, which go to your
		// delivery report handlers (see Program.cs). Providers ignore the response; we return the reports so you can see them.
		[HttpPost("channel/provider/update", Name = "DeliveryReport")]
		public IActionResult ChannelProviderDeliveryReport(ChannelProviderDeliveryReportRequest providerReport)
		{
			_communicationsClient.DispatchAsync(providerReport.DeliveryReports);
			return Ok(providerReport.DeliveryReports);
		}

		private static readonly HttpClient Http = new();

		// Sends the webhook Twilio would send when an SMS changes status to the route above, signed the way Twilio signs it.
		// Returns the webhook that was sent and the delivery reports Transmitly built from it.
		// Needs Twilio enabled with an AuthToken in appsettings.json, the same as receiving real Twilio webhooks.
		[HttpPost("channel/provider/update/simulate/twilio-sms", Name = "SimulateTwilioSmsDeliveryReport")]
		public async Task<IActionResult> SimulateTwilioSmsDeliveryReport([FromServices] TransmitlyConfiguration tlyConfig, SmsStatus status = SmsStatus.Delivered, CancellationToken cancellationToken = default)
		{
			var twilio = tlyConfig.ChannelProviders.Twilio.FirstOrDefault(t => t.IsEnabled);
			if (string.IsNullOrWhiteSpace(twilio?.AuthToken))
				return Problem(
					title: "Twilio isn't enabled",
					detail: "Twilio webhooks are only accepted when Twilio is configured. In appsettings.json, under Transmitly:ChannelProviders:Twilio, set IsEnabled to true and set your AuthToken and AccountSid, then restart.");

			// At dispatch, Transmitly adds this context to the callback URL it gives the provider.
			// It's how the route above knows which provider, channel and pipeline a webhook belongs to.
			var url = new Uri(Url.Link("DeliveryReport", null)!)
				.AddPipelineContext(null, PipelineName.FirstPipeline, null, Id.Channel.Sms(), Id.ChannelProvider.Twilio(twilio.Id))
				.AbsoluteUri;

			var form = new Dictionary<string, string>
			{
				["MessageSid"] = "SM" + Guid.NewGuid().ToString("N"),
				["MessageStatus"] = typeof(SmsStatus).GetField(status.ToString())?.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? "unknown",
				["AccountSid"] = twilio.AccountSid ?? string.Empty,
				["From"] = "+15005550006",
				["To"] = "+15005550009",
				["ApiVersion"] = "2010-04-01"
			};

			// Twilio signs the full URL plus every form field (sorted by name) with your auth token.
			// See: https://www.twilio.com/docs/usage/webhooks/webhooks-security
			var signedData = url + string.Concat(form.OrderBy(f => f.Key, StringComparer.Ordinal).Select(f => f.Key + f.Value));
			var signature = Convert.ToBase64String(HMACSHA1.HashData(Encoding.UTF8.GetBytes(twilio.AuthToken), Encoding.UTF8.GetBytes(signedData)));

			using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(form) };
			request.Headers.Add("X-Twilio-Signature", signature);
			using var response = await Http.SendAsync(request, cancellationToken);
			var body = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
				return StatusCode((int)response.StatusCode, body);

			return Ok(new
			{
				webhook = new { url, headers = new { XTwilioSignature = signature }, form },
				deliveryReports = JsonDocument.Parse(body).RootElement
			});
		}
	}
}
