// Copyright (c) Code Impressions, LLC. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Transmitly.Channel.Configuration.WhatsApp;
using Transmitly.Channel.WhatsApp;
using Transmitly.Pipeline.Configuration;

namespace Transmitly;

/// <summary>
/// Extension methods related to the WhatsApp channel.
/// </summary>
public static class WhatsAppChannelConfigurationExtensions
{
	private const string WhatsAppId = "WhatsApp";

	/// <summary>
	/// Gets the 'WhatsApp' channel Id.
	/// </summary>
	/// <param name="channelId">The extension Id of the channel.</param>
	/// <param name="channel">Channel object.</param>
	/// <returns></returns>
	public static string WhatsApp(this Channels channel, string channelId = "")
	{
		return Guard.AgainstNull(channel).GetId(WhatsAppId, channelId);
	}

	/// <summary>
	/// Adds the 'WhatsApp' communication channel to provider pipeline.
	/// </summary>
	/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline.</param>
	/// <param name="fromAddressResolver">Service to resolve the from address for this channel.</param>
	/// <param name="whatsAppChannelConfiguration">WhatsApp channel configuration options.</param>
	/// <returns></returns>
	public static IPipelineConfiguration AddWhatsApp(this IPipelineConfiguration pipelineChannelConfiguration, Func<IDispatchCommunicationContext, IPlatformIdentityAddress?>? fromAddressResolver, Action<IWhatsAppChannelConfiguration> whatsAppChannelConfiguration)
	{
		var whatsAppOptions = new WhatsAppChannelConfiguration(fromAddressResolver);
		whatsAppChannelConfiguration(whatsAppOptions);
		pipelineChannelConfiguration.AddChannel(new WhatsAppChannel(whatsAppOptions));
		return pipelineChannelConfiguration;
	}

	/// <summary>
	/// Adds the 'WhatsApp' communication channel to provider pipeline.
	/// </summary>
	/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline.</param>
	/// <param name="fromAddress">Address used as the 'from' address.</param>
	/// <param name="whatsAppChannelConfiguration">WhatsApp channel configuration options.</param>
	/// <returns></returns>
	public static IPipelineConfiguration AddWhatsApp(this IPipelineConfiguration pipelineChannelConfiguration, IPlatformIdentityAddress fromAddress, Action<IWhatsAppChannelConfiguration> whatsAppChannelConfiguration)
	{
		return AddWhatsApp(pipelineChannelConfiguration, _ => fromAddress, whatsAppChannelConfiguration);
	}

	/// <summary>
	/// Adds the 'WhatsApp' communication channel to provider pipeline.
	/// </summary>
	/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline.</param>
	/// <param name="whatsAppChannelConfiguration">WhatsApp channel configuration options.</param>
	/// <returns></returns>
	public static IPipelineConfiguration AddWhatsApp(this IPipelineConfiguration pipelineChannelConfiguration, Action<IWhatsAppChannelConfiguration> whatsAppChannelConfiguration)
	{
		return AddWhatsApp(pipelineChannelConfiguration, fromAddressResolver: null, whatsAppChannelConfiguration);
	}
}
