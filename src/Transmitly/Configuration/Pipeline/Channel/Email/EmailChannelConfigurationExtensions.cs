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

using Transmitly.Channel.Configuration.Email;
using Transmitly.Pipeline.Configuration;

namespace Transmitly;

/// <summary>
/// Extension methods related to the email channel
/// </summary>
public static class EmailChannelConfigurationExtensions
{
	private const string EmailId = "Email";
	/// <summary>
	/// Gets the 'Email' channel Id
	/// </summary>
	/// <param name="channelId">The extension Id of the channel</param>
	/// <param name="channel">Channel object.</param>
	/// <returns></returns>
	public static string Email(this Channels channel, string channelId = "")
	{
		return Guard.AgainstNull(channel).GetId(EmailId, channelId);
	}

	/// <summary>
	/// Adds the 'Email' communication channel to provider pipeline
	/// </summary>
	/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline</param>
	/// <param name="fromAddress">Address used as the 'from' address</param>
	/// <param name="emailChannelConfiguration">Email Channel configuration options</param>
	/// <returns></returns>
	public static IPipelineConfiguration AddEmail(this IPipelineConfiguration pipelineChannelConfiguration, string fromAddress, Action<IEmailChannelConfiguration> emailChannelConfiguration)
	{
		return AddEmail(pipelineChannelConfiguration, fromAddress.AsIdentityAddress(), emailChannelConfiguration);
	}


	/// <summary>
	/// Adds the 'Email' communication channel to provider pipeline
	/// </summary>
	/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline</param>
	/// <param name="fromAddress">Address used as the 'from' address</param>
	/// <param name="emailChannelConfiguration">Email Channel configuration options</param>
	/// <returns></returns>
	public static IPipelineConfiguration AddEmail(this IPipelineConfiguration pipelineChannelConfiguration, IPlatformIdentityAddress fromAddress, Action<IEmailChannelConfiguration> emailChannelConfiguration)
	{
		return AddEmail(pipelineChannelConfiguration, (ctx) => fromAddress, emailChannelConfiguration);
	}

	/// <summary>
	/// Adds the 'Email' communication channel to provider pipeline
	/// </summary>
	/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline</param>
	/// <param name="fromAddressResolver">Resolver that will return a <see cref="IPlatformIdentityAddress"/></param>
	/// <param name="emailChannelConfiguration">Email Channel configuration options</param>
	/// <returns></returns>
	public static IPipelineConfiguration AddEmail(this IPipelineConfiguration pipelineChannelConfiguration, Func<IDispatchCommunicationContext, IPlatformIdentityAddress> fromAddressResolver, Action<IEmailChannelConfiguration> emailChannelConfiguration)
	{
		var emailOptions = new EmailChannelConfiguration(fromAddressResolver);
		emailChannelConfiguration(emailOptions);
		pipelineChannelConfiguration.AddChannel(new EmailChannel(emailOptions));
		return pipelineChannelConfiguration;
	}

	public static string ToEmailAddress(this IPlatformIdentityAddress identityAddress)
	{
		Guard.AgainstNull(identityAddress);
		Guard.AgainstNullOrWhiteSpace(identityAddress.Value);
		return string.IsNullOrEmpty(identityAddress.Display)
			? identityAddress.Value
			: $"{identityAddress.Display} <{identityAddress.Display}>";
	}
}