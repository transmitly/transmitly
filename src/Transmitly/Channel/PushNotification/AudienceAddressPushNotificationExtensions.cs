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

namespace Transmitly.Channel.Push
{
	/// <summary>
	/// Defines the Push Notification specific audience address types
	/// </summary>
	public static class AudienceAddressPushNotificationExtensions
	{
#pragma warning disable IDE0060 // Remove unused parameter
		/// <summary>
		/// General device token type
		/// </summary>
		/// <param name="audienceAddress"></param>
		/// <returns>Device Token Identifier</returns>
		public static string DeviceToken(this IAudienceAddressType? audienceAddress) => "device-token";

		/// <summary>
		/// Push notification topic type
		/// </summary>
		/// <param name="audienceAddress"></param>
		/// <returns>Push Notification Topic Identifier</returns>
		public static string Topic(this IAudienceAddressType? audienceAddress) => "push-topic";
#pragma warning restore IDE0060 // Remove unused parameter
	}
}
