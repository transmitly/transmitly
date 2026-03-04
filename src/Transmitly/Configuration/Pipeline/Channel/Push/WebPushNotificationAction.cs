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
/// Action entry shown by Web push notifications.
/// </summary>
public sealed class WebPushNotificationAction
{
	/// <summary>
	/// Initializes a new instance of the <see cref="WebPushNotificationAction"/> class.
	/// </summary>
	/// <param name="action">Action identifier.</param>
	/// <param name="title">Action label shown to the user.</param>
	/// <param name="icon">Optional action icon URL.</param>
	public WebPushNotificationAction(string action, string title, string? icon = null)
	{
		Action = Guard.AgainstNullOrWhiteSpace(action);
		Title = Guard.AgainstNullOrWhiteSpace(title);
		Icon = icon;
	}

	/// <summary>
	/// Action identifier.
	/// </summary>
	public string Action { get; }

	/// <summary>
	/// Action label shown to the user.
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// Optional action icon URL.
	/// </summary>
	public string? Icon { get; }
}