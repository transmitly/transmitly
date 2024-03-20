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

using Transmitly.Channel.Voice;
using Transmitly.Pipeline.Configuration;

namespace Transmitly
{
	/// <summary>
	/// Extension methods related to the voice channel
	/// </summary>
	public static class VoiceChannelConfigurationExtensions
	{

		private const string VoiceId = "Voice";
		/// <summary>
		/// Gets the 'Voice' channel Id
		/// </summary>
		/// <param name="channelId"></param>
		/// <returns></returns>
		public static string Voice(this Channels channel, string channelId = "Default")
		{
			Guard.AgainstNull(channel);
			Guard.AgainstNullOrWhiteSpace(channelId);
			return $"{VoiceId}.{channelId}";
		}

		/// <summary>
		/// Adds the 'Voice' communication channel to provider pipeline.
		/// </summary>
		/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline.</param>
		/// <param name="voiceChannelConfiguration">Voice Channel configuration options.</param>
		/// <param name="allowedChannelProviders">List of channel providers that will be allowed to handle this channel.</param>
		/// <param name="fromAddress">Address the communication will appear to be sent from.</param>
		/// <returns></returns>
		public static IPipelineChannelConfiguration AddVoice(this IPipelineChannelConfiguration pipelineChannelConfiguration, IAudienceAddress? fromAddress, Action<IVoiceChannel> voiceChannelConfiguration, params string[]? allowedChannelProviders)
		{
			Guard.AgainstNull(voiceChannelConfiguration);
			Guard.AgainstNull(pipelineChannelConfiguration);

			var voiceOptions = new VoiceChannel(fromAddress, allowedChannelProviders);
			voiceChannelConfiguration(voiceOptions);
			pipelineChannelConfiguration.AddChannel(voiceOptions);
			return pipelineChannelConfiguration;
		}

		/// <summary>
		/// Adds the 'Voice' communication channel to provider pipeline.
		/// </summary>
		/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline.</param>
		/// <param name="voiceChannelConfiguration">Voice Channel configuration options.</param>
		/// <param name="allowedChannelProviders">List of channel providers that will be allowed to handle this channel.</param>
		/// <param name="fromAddress">Address the communication will appear to be sent from.</param>
		/// <returns></returns>
		public static IPipelineChannelConfiguration AddVoice(this IPipelineChannelConfiguration pipelineChannelConfiguration, Func<IDispatchCommunicationContext, IAudienceAddress> fromAddressResolver, Action<IVoiceChannel> voiceChannelConfiguration, params string[]? allowedChannelProviders)
		{
			Guard.AgainstNull(voiceChannelConfiguration);
			Guard.AgainstNull(pipelineChannelConfiguration);

			var voiceOptions = new VoiceChannel(fromAddressResolver, allowedChannelProviders);
			voiceChannelConfiguration(voiceOptions);
			pipelineChannelConfiguration.AddChannel(voiceOptions);
			return pipelineChannelConfiguration;
		}


		/// <summary>
		/// Adds the 'Voice' communication channel to provider pipeline.
		/// </summary>
		/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline.</param>
		/// <param name="voiceChannelConfiguration">Voice Channel configuration options.</param>
		/// <param name="allowedChannelProviders">List of channel providers that will be allowed to handle this channel.</param>
		/// <returns></returns>
		public static IPipelineChannelConfiguration AddVoice(this IPipelineChannelConfiguration pipelineChannelConfiguration, Action<IVoiceChannel> voiceChannelConfiguration, params string[]? allowedChannelProviders)
		{
			return AddVoice(pipelineChannelConfiguration, fromAddress: null, voiceChannelConfiguration, allowedChannelProviders);
		}
	}
}
