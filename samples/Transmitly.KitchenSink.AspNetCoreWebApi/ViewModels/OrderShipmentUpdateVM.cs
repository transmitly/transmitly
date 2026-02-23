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

using System.ComponentModel.DataAnnotations;

namespace Transmitly.KitchenSink.AspNetCoreWebApi.Controllers
{
	/// <summary>
	/// Dedicated request model for the order-shipment-update endpoint.
	/// This keeps the existing generic dispatch examples unchanged while providing
	/// a focused teaching surface for profile/content enricher concepts.
	/// </summary>
	public sealed class OrderShipmentUpdateVM
	{
		[Required]
		public string CustomerId { get; set; } = string.Empty;

		[Required]
		public string OrderId { get; set; } = string.Empty;

		/// <summary>
		/// Type is used by PlatformIdentityProfile enrichers for type filtering.
		/// </summary>
		public string CustomerType { get; set; } = "kitchensink-member";

		/// <summary>
		/// Optional seed addresses. The profile enricher can backfill these when omitted.
		/// </summary>
		public List<DispatchPlatformIdentityAddress> Addresses { get; set; } = [];

		/// <summary>
		/// Optional dispatch-level channel preference/filter behavior.
		/// </summary>
		public List<string> AllowedChannelIds { get; set; } = [];

		public string? Culture { get; set; }
	}
}
