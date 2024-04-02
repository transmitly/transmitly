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

using Transmitly.Channel.Push;
using Transmitly.Pipeline.Configuration;

namespace Transmitly
{
	/// <summary>
	/// Extension methods related to the Push channel
	/// </summary>
	public static class PushChannelConfigurationExtensions
	{
		private const string PushNotificationId = "Push";
		/// <summary>
		/// Gets the 'PushNotification' channel Id
		/// </summary>
		/// <param name="channelId">The extension Id of the channel</param>
		/// <param name="channel">Channel object.</param>
		/// <returns></returns>
		public static string PushNotification(this Channels channel, string channelId = "")
		{
			return Guard.AgainstNull(channel).GetId(PushNotificationId, channelId);
		}

		/// <summary>
		/// Adds the 'Push Notification' communication channel to provider pipeline
		/// </summary>
		/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline</param>
		/// <param name="pushChannelConfiguration">Push Notification Channel configuration options</param>
		/// <param name="allowedChannelProviders">List of channel providers that will be allowed to handle this channel</param>
		/// <returns></returns>
		public static IPipelineChannelConfiguration AddPushNotification(
			this IPipelineChannelConfiguration pipelineChannelConfiguration,
			Action<IPushNotificationChannel> pushChannelConfiguration,
			params string[]? allowedChannelProviders
		)
		{
			var pushOptions = new PushNotificationChannel(allowedChannelProviders);
			pushChannelConfiguration(pushOptions);
			pipelineChannelConfiguration.AddChannel(pushOptions);
			return pipelineChannelConfiguration;
		}

		public static IPipelineChannelConfiguration AddPushNotification(
			this IPipelineChannelConfiguration pipelineChannelConfiguration,
			params string[]? allowedChannelProviders
		)
		{
			return AddPushNotification(pipelineChannelConfiguration, (opts) => { }, allowedChannelProviders);
		}
	}
}