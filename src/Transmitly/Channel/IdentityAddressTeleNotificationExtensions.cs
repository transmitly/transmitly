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

namespace Transmitly.Channel
{
	/// <summary>
	/// Defines the Push Notification specific identity address types
	/// </summary>
	public static class IdentityAddressTeleNotificationExtensions
	{
#pragma warning disable IDE0060 // Remove unused parameter

		public static string Phone(this IIdentityAddressType? identityAddress) => "phone";

		public static string Mobile(this IIdentityAddressType? identityAddress) => "mobile-phone";

		public static string HomePhone(this IIdentityAddressType? identityAddress) => "home-phone";

		public static string Cell(this IIdentityAddressType? identityAddress) => "cell-phone";

#pragma warning restore IDE0060 // Remove unused parameter

	}
}
