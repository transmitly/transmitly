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

using Transmitly.Channel.Sms;
using Transmitly.Pipeline.Configuration;

namespace Transmitly
{
	/// <summary>
	/// Extension methods related to the sms channel.
	/// </summary>
	public static class SmsChannelConfigurationExtensions
	{
		private const string SmsId = "Sms";
		/// <summary>
		/// Gets the 'Sms' channel Id.
		/// </summary>
		/// <param name="channelId"></param>
		/// <returns></returns>
		public static string Sms(this Channels channel, string channelId = "Default")
		{
			Guard.AgainstNull(channel);
			Guard.AgainstNullOrWhiteSpace(channelId);
			return $"{SmsId}.{channelId}";
		}

		/// <summary>
		/// Adds the 'Sms' communication channel to provider pipeline.
		/// </summary>
		/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline.</param>
		/// <param name="fromAddressResolver">Service to resolve the from address for this channel.</param>
		/// <param name="smsChannelConfiguration">Sms Channel configuration options.</param>
		/// <param name="allowedChannelProviders">List of channel providers that will be allowed to handle this channel.</param>
		/// <returns></returns>
		public static IPipelineChannelConfiguration AddSms(this IPipelineChannelConfiguration pipelineChannelConfiguration, Func<IDispatchCommunicationContext, IAudienceAddress> fromAddressResolver, Action<ISmsChannel> smsChannelConfiguration, params string[]? allowedChannelProviders)
		{
			var emailOptions = new SmsChannel(fromAddressResolver, allowedChannelProviders);
			smsChannelConfiguration(emailOptions);
			pipelineChannelConfiguration.AddChannel(emailOptions);
			return pipelineChannelConfiguration;
		}

		/// <summary>
		/// Adds the 'Sms' communication channel to provider pipeline.
		/// </summary>
		/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline.</param>
		/// <param name="fromAddress">Address used as the 'from' address.</param>
		/// <param name="smsChannelConfiguration">Sms Channel configuration options.</param>
		/// <param name="allowedChannelProviders">List of channel providers that will be allowed to handle this channel.</param>
		/// <returns></returns>
		public static IPipelineChannelConfiguration AddSms(this IPipelineChannelConfiguration pipelineChannelConfiguration, IAudienceAddress fromAddress, Action<ISmsChannel> smsChannelConfiguration, params string[]? allowedChannelProviders)
		{
			var emailOptions = new SmsChannel(fromAddress, allowedChannelProviders);
			smsChannelConfiguration(emailOptions);
			pipelineChannelConfiguration.AddChannel(emailOptions);
			return pipelineChannelConfiguration;
		}

		/// <summary>
		/// Adds the 'Sms' communication channel to provider pipeline.
		/// </summary>
		/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline.</param>
		/// <param name="smsChannelConfiguration">Sms Channel configuration options.</param>
		/// <param name="allowedChannelProviders">List of channel providers that will be allowed to handle this channel.</param>
		/// <returns></returns>
		public static IPipelineChannelConfiguration AddSms(this IPipelineChannelConfiguration pipelineChannelConfiguration, Action<ISmsChannel> smsChannelConfiguration, params string[]? allowedChannelProviders)
		{
			var emailOptions = new SmsChannel(allowedChannelProviders);
			smsChannelConfiguration(emailOptions);
			pipelineChannelConfiguration.AddChannel(emailOptions);
			return pipelineChannelConfiguration;
		}
	}
}