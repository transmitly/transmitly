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

namespace Transmitly;

/// <summary>
/// Base push notification content payload.
/// </summary>
public interface IPushNotificationContent
{
	/// <summary>
	/// Push notification title.
	/// </summary>
	string? Title { get; }
	/// <summary>
	/// Push notification body.
	/// </summary>
	string? Body { get; }
	/// <summary>
	/// Push notification image URL.
	/// </summary>
	string? ImageUrl { get; }
	/// <summary>
	/// Push notification custom data values.
	/// </summary>
	IReadOnlyDictionary<string, string>? Data { get; }
	/// <summary>
	/// Push notification header values.
	/// </summary>
	IReadOnlyDictionary<string, string>? Headers { get; }
}
