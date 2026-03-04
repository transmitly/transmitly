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

namespace Transmitly.Channel.Configuration.Push;

/// <summary>
/// Android LED light behavior for a notification.
/// </summary>
public sealed class AndroidNotificationLightSettings
{
	/// <summary>
	/// Light color in hexadecimal format (for example, <c>#RRGGBB</c>).
	/// </summary>
	public string? Color { get; init; }

	/// <summary>
	/// Duration that the LED should remain on.
	/// </summary>
	public TimeSpan? OnDuration { get; init; }

	/// <summary>
	/// Duration that the LED should remain off.
	/// </summary>
	public TimeSpan? OffDuration { get; init; }
}