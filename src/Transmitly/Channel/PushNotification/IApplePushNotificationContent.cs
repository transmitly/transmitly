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
/// Apple specific push notification content payload.
/// </summary>
public interface IApplePushNotificationContent : IPushNotificationContent
{
	/// <summary>
	/// Apple notification subtitle.
	/// </summary>
	string? Subtitle { get; }
	/// <summary>
	/// Apple localized subtitle key.
	/// </summary>
	string? SubtitleLocalizationKey { get; }
	/// <summary>
	/// Apple localized subtitle arguments.
	/// </summary>
	IReadOnlyCollection<string> SubtitleLocalizationArguments { get; }
	/// <summary>
	/// Apple action localization key.
	/// </summary>
	string? ActionLocalizationKey { get; }
	/// <summary>
	/// Apple body localization key.
	/// </summary>
	string? BodyLocalizationKey { get; }
	/// <summary>
	/// Apple body localization arguments.
	/// </summary>
	IReadOnlyCollection<string>? BodyLocalizationArguments { get; }
	/// <summary>
	/// Apple title localization key.
	/// </summary>
	string? TitleLocalizationKey { get; }
	/// <summary>
	/// Apple title localization arguments.
	/// </summary>
	IReadOnlyCollection<string>? TitleLocalizationArguments { get; }
	/// <summary>
	/// Apple badge count.
	/// </summary>
	int? Badge { get; }
	/// <summary>
	/// Apple sound name.
	/// </summary>
	string? Sound { get; }
	/// <summary>
	/// Apple critical sound configuration.
	/// </summary>
	AppleCriticalSound? CriticalSound { get; }
	/// <summary>
	/// Whether this is a background update notification.
	/// </summary>
	bool? IsBackgroundUpdate { get; }
	/// <summary>
	/// Whether content can be modified by a notification service extension.
	/// </summary>
	bool? IsContentMutable { get; }
	/// <summary>
	/// Apple notification category identifier.
	/// </summary>
	string? Category { get; }
	/// <summary>
	/// Apple notification thread identifier.
	/// </summary>
	string? ThreadId { get; }
	/// <summary>
	/// Apple notification launch image.
	/// </summary>
	string? LaunchImage { get; }
	/// <summary>
	/// Apple live activity token.
	/// </summary>
	string? LiveActivityToken { get; }
	/// <summary>
	/// Apple notification interruption level.
	/// </summary>
	AppleNotificationInterruptionLevel? InterruptionLevel { get; }
	/// <summary>
	/// Apple notification relevance score.
	/// </summary>
	double? RelevanceScore { get; }
	/// <summary>
	/// Apple target content identifier.
	/// </summary>
	string? TargetContentId { get; }
}
