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

using Transmitly.Template.Configuration;

namespace Transmitly.Channel.Configuration.Push;

/// <summary>
/// Web specific push notification configuration.
/// </summary>
public interface IWebPushNotification : IPushContent
{
	/// <summary>
	/// Web notification icon templates.
	/// </summary>
	IContentTemplateConfiguration Icon { get; }
	/// <summary>
	/// Web notification badge templates.
	/// </summary>
	IContentTemplateConfiguration Badge { get; }
	/// <summary>
	/// Web notification language templates.
	/// </summary>
	IContentTemplateConfiguration Language { get; }
	/// <summary>
	/// Whether existing notifications should be renotified.
	/// </summary>
	bool? Renotify { get; }
	/// <summary>
	/// Whether the notification should remain active until user interaction.
	/// </summary>
	bool? RequireInteraction { get; }
	/// <summary>
	/// Whether the notification should be silent.
	/// </summary>
	bool? IsSilent { get; }
	/// <summary>
	/// Web notification tag.
	/// </summary>
	string? Tag { get; }
	/// <summary>
	/// Web notification timestamp.
	/// </summary>
	DateTimeOffset? Timestamp { get; }
	/// <summary>
	/// Web notification vibration pattern.
	/// </summary>
	IReadOnlyCollection<int>? VibratePattern { get; }
	/// <summary>
	/// Web notification text direction.
	/// </summary>
	WebPushDisplayDirection? Direction { get; }
	/// <summary>
	/// Actions shown by the Web notification.
	/// </summary>
	IReadOnlyCollection<WebPushNotificationAction>? Actions { get; }
}