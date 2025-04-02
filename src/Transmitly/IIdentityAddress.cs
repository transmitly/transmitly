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

namespace Transmitly
{
	/// <summary>
	/// An address for an identity.
	/// </summary>
	public interface IIdentityAddress
	{		
		/// <summary>
		/// Value of the address (test@example.com, +1234567890)
		/// </summary>
		/// <example>test@example.com; +1234567890; 123 Main St, Example, EX 01234</example>
		string Value { get; set; }
		/// <summary>
		/// Display name of the address.
		/// </summary>
		/// <example>John Doe, Work Group</example>
		string? Display { get; set; }
		/// <summary>
		/// The <see cref="Id.Channel">channel id</see> the address intended for.
		/// </summary>
		/// <example>Sms, Email, Push</example>
		string? ChannelId { get; set; }
		/// <summary>
		/// <see cref="IIdentityAddressType">Purpose</see> of the address.
		/// </summary>
		/// <example>Primary, Secondary, Home, Work, Mobile</example>
		string? Purpose { get; set; }
		/// <summary>
		/// Defines the parts of an identity address.
		/// </summary>
		/// <remarks>Primarily for addresses that are not well represented by a single line. For example: Mailing addresses</remarks>
		IDictionary<string, string?> AddressParts { get; set; }
		/// <summary>
		/// Additional attributes for the address.
		/// </summary>
		/// <remarks>Primarily used to define additional context for an address that might be used by a channel provider.</remarks>
		IDictionary<string, string?> Attributes { get; set; }
	}
}
