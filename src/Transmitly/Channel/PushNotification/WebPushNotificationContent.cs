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

namespace Transmitly.Channel.Push;

internal sealed class WebPushNotificationContent : PushNotificationContent, IWebPushNotificationContent
{
	public string? Icon { get; internal set; }

	public string? Badge { get; internal set; }

	public string? Language { get; internal set; }

	public bool? Renotify { get; internal set; }

	public bool? RequireInteraction { get; internal set; }

	public bool? IsSilent { get; internal set; }

	public string? Tag { get; internal set; }

	public DateTimeOffset? Timestamp { get; internal set; }

	public IReadOnlyCollection<int>? VibratePattern { get; internal set; }

	public WebPushDisplayDirection? Direction { get; internal set; }

	public IReadOnlyCollection<WebPushNotificationAction>? Actions { get; internal set; }
}