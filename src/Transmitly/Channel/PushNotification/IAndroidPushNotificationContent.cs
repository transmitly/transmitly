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

using Transmitly.Channel.Configuration.Push;

namespace Transmitly;

/// <summary>
/// Android specific push notification content payload.
/// </summary>
public interface IAndroidPushNotificationContent : IPushNotificationContent
{
	/// <summary>
	/// Android collapse identifier.
	/// </summary>
	string? CollapseId { get; }
	/// <summary>
	/// Android delivery priority.
	/// </summary>
	AndroidNotificationPriority? Priority { get; }
	/// <summary>
	/// Android time to live.
	/// </summary>
	TimeSpan? TimeToLive { get; }
	/// <summary>
	/// Android target application package identifier.
	/// </summary>
	string? TargetApplicationId { get; }
	/// <summary>
	/// Whether delivery is allowed before first device unlock.
	/// </summary>
	bool? AllowDeliveryBeforeFirstUnlock { get; }
}
