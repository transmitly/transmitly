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

using Transmitly.Channel.Email;
using Transmitly.Pipeline.Configuration;
namespace Transmitly
{
	/// <summary>
	/// Extension methods related to the email channel
	/// </summary>
	public static class EmailChannelConfigurationExtensions
	{
		private const string EmailId = "Email";
		/// <summary>
		/// Gets the 'Email' channel Id
		/// </summary>
		/// <param name="channelId"></param>
		/// <returns></returns>
		public static string Email(this Channels channel, string channelId = "Default")
		{
			Guard.AgainstNull(channel);
			Guard.AgainstNullOrWhiteSpace(channelId);
			return $"{EmailId}.{channelId}";
		}

		/// <summary>
		/// Adds the 'Email' communication channel to the provided pipeline
		/// </summary>
		/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline</param>
		/// <param name="fromAddress">Address used as the 'from' address</param>
		/// <param name="emailChannelConfiguration">Email Channel configuration options</param>
		/// <returns></returns>
		public static IPipelineChannelConfiguration AddEmail(this IPipelineChannelConfiguration pipelineChannelConfiguration, IAudienceAddress fromAddress, Action<IEmailChannel> emailChannelConfiguration)
		{
			return AddEmail(pipelineChannelConfiguration, fromAddress, emailChannelConfiguration, null);
		}

		/// <summary>
		/// Adds the 'Email' communication channel to provider pipeline
		/// </summary>
		/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline</param>
		/// <param name="fromAddress">Address used as the 'from' address</param>
		/// <param name="emailChannelConfiguration">Email Channel configuration options</param>
		/// <param name="allowedChannelProviders">List of channel providers that will be allowed to handle this channel</param>
		/// <returns></returns>
		public static IPipelineChannelConfiguration AddEmail(this IPipelineChannelConfiguration pipelineChannelConfiguration, IAudienceAddress fromAddress, Action<IEmailChannel> emailChannelConfiguration, params string[]? allowedChannelProviders)
		{
			var emailOptions = new EmailChannel(fromAddress, allowedChannelProviders);
			emailChannelConfiguration(emailOptions);
			pipelineChannelConfiguration.AddChannel(emailOptions);
			return pipelineChannelConfiguration;
		}

		/// <summary>
		/// Adds the 'Email' communication channel to provider pipeline
		/// </summary>
		/// <param name="pipelineChannelConfiguration">Channel configuration for the pipeline</param>
		/// <param name="fromAddressResolver">Resolver that will return a <see cref="IAudienceAddress"/></param>
		/// <param name="emailChannelConfiguration">Email Channel configuration options</param>
		/// <param name="allowedChannelProviders">List of channel providers that will be allowed to handle this channel</param>
		/// <returns></returns>
		public static IPipelineChannelConfiguration AddEmail(this IPipelineChannelConfiguration pipelineChannelConfiguration, Func<IDispatchCommunicationContext, IAudienceAddress> fromAddressResolver, Action<IEmailChannel> emailChannelConfiguration, params string[]? allowedChannelProviders)
		{
			var emailOptions = new EmailChannel(fromAddressResolver, allowedChannelProviders);
			emailChannelConfiguration(emailOptions);
			pipelineChannelConfiguration.AddChannel(emailOptions);
			return pipelineChannelConfiguration;
		}

		/// <summary>
		/// Adds an email message pipeline to the communications configuration builder
		/// </summary>
		/// <param name="communicationsConfigurationBuilder">The communications configuration builder</param>
		/// <param name="messageId">The ID of the message</param>
		/// <param name="fromAddress">The 'from' address</param>
		/// <param name="subject">The subject of the email</param>
		/// <param name="htmlBody">The HTML body of the email</param>
		/// <param name="textBody">The text body of the email (optional)</param>
		/// <returns>The updated communications configuration builder</returns>
		public static CommunicationsClientBuilder AddEmailMessage(this CommunicationsClientBuilder communicationsConfigurationBuilder, string messageId, string fromAddress, string subject, string htmlBody, string? textBody = null)
		{
			return communicationsConfigurationBuilder.AddPipeline(Guard.AgainstNullOrWhiteSpace(messageId), options =>
			{
				options.AddEmail(Guard.AgainstNullOrWhiteSpace(fromAddress).AsAudienceAddress(), email =>
				{
					email.Subject.AddStringTemplate(Guard.AgainstNullOrWhiteSpace(subject));
					if (!string.IsNullOrWhiteSpace(htmlBody))
					{
						email.HtmlBody.AddStringTemplate(htmlBody);
					}

					if (!string.IsNullOrWhiteSpace(textBody))
					{
						email.TextBody.AddStringTemplate(textBody!);
					}
				});
			});
		}

		public static string ToEmailAddress(this IAudienceAddress audienceAddress)
		{
			Guard.AgainstNull(audienceAddress);
			Guard.AgainstNullOrWhiteSpace(audienceAddress.Value);
			if (string.IsNullOrEmpty(audienceAddress.Display))
				return audienceAddress.Value;
			return $"{audienceAddress.Display} <{audienceAddress.Display}>";
		}
	}
}