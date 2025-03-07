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

using Transmitly.Samples.Shared;

namespace Transmitly
{
	public static class TransmitlyConfigurationExtensions
	{
		public static CommunicationsClientBuilder AddFirebaseSupport(this CommunicationsClientBuilder tly, TransmitlyConfiguration tlyConfig)
		{
			foreach (var firebaseSetting in tlyConfig.ChannelProviders.Firebase.Where(s => s.IsEnabled))
			{
				tly.AddFirebaseSupport(firebase =>
				{
					firebase.ProjectId = firebaseSetting.Options.ProjectId;
					firebase.Credential = firebaseSetting.Options.Credential;
					firebase.ServiceAccountId = firebaseSetting.Options.ServiceAccountId;
				}, firebaseSetting.Id);
			}
			return tly;
		}

		public static CommunicationsClientBuilder AddInfobipSupport(this CommunicationsClientBuilder tly, TransmitlyConfiguration tlyConfig)
		{
			foreach (var infobipSetting in tlyConfig.ChannelProviders.Infobip.Where(s => s.IsEnabled))
			{
				// Adding the Transmitly.ChannelProvider.Infobip package
				// allows us to add support to our app for Email, SMS & Voice through
				// an account with Infobip.
				tly.AddInfobipSupport(infobip =>
				{
					infobip.BasePath = infobipSetting.BasePath;
					infobip.ApiKey = infobipSetting.ApiKey;
					infobip.ApiKeyPrefix = infobipSetting.ApiKeyPrefix;
				}, infobipSetting.Id);
			}
			return tly;
		}

		public static CommunicationsClientBuilder AddTwilioSupport(this CommunicationsClientBuilder tly, TransmitlyConfiguration tlyConfig)
		{
			foreach (var twilioSetting in tlyConfig.ChannelProviders.Twilio.Where(s => s.IsEnabled))
			{
				// Adding the Transmitly.ChannelProvider.Twilio package
				// allows us to add support to our app for SMS through
				// an account with Twilio.
				tly.AddTwilioSupport(twilio =>
				{
					twilio.AuthToken = twilioSetting.AuthToken;
					twilio.AccountSid = twilioSetting.AccountSid;
				}, twilioSetting.Id);
			}
			return tly;
		}

		public static CommunicationsClientBuilder AddSmtpSupport(this CommunicationsClientBuilder tly, TransmitlyConfiguration tlyConfig)
		{
			foreach (var smtpSetting in tlyConfig.ChannelProviders.Smtp.Where(s => s.IsEnabled))
			{
				// Adding the Transmitly.ChannelProvider.Smtp package
				// allows us to add support to our app for Email via SMTP
				// with the MailKit library. 
				tly.AddSmtpSupport(smtp =>
				{
					smtp.Host = smtpSetting.Host;
					smtp.SocketOptions = ChannelProvider.Smtp.Configuration.SecureSocketOptions.Auto;
					smtp.Port = smtpSetting.Port;
					smtp.UserName = smtpSetting.Username;
					smtp.Password = smtpSetting.Password;
				}, smtpSetting.Id);
			}
			return tly;
		}

		public static CommunicationsClientBuilder AddSendGridSupport(this CommunicationsClientBuilder tly, TransmitlyConfiguration tlyConfig)
		{
			// Adding the Transmitly.ChannelProvider.SendGrid package
			// allows us to add support to our app for Email
			// through an account with SendGrid. 
			foreach (var sendGridSetting in tlyConfig.ChannelProviders.SendGrid.Where(s => s.IsEnabled))
			{
				tly.AddSendGridSupport(sendgrid =>
				{
					sendgrid.ApiKey = sendGridSetting.ApiKey;
				});
			}
			return tly;
		}

		public static CommunicationsClientBuilder AddDispatchLoggingSupport(this CommunicationsClientBuilder tly, TransmitlyConfiguration tlyConfig)
		{
			// Adding the local Transmitly.ChannelProvider.Logger package
			// allows us to add logging to the console for any channel type
			foreach (var debuggerSetting in tlyConfig.ChannelProviders.Debugger.Where(s => s.IsEnabled))
			{
				tly.AddDispatchLoggingSupport(logger =>
				{
					logger.SimulateDispatchResult = debuggerSetting.SimulateDispatchResult;
					logger.LogLevel = debuggerSetting.LogLevel;
				});
			}
			return tly;
		}
	}
}
