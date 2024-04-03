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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Filters;
using System.Text.Json;
using System.Text.Json.Serialization;
using Transmitly.ChannelProvider;
using Transmitly.KitchenSink.AspNetCoreWebApi.Configuration;

namespace Transmitly.KitchenSink.AspNetCoreWebApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			ILogger? logger = null;

			/// !!! See appSettings.json for examples of how to configure with your own settings !!!
			var tlyConfig = builder.Configuration.GetRequiredSection("Transmitly").Get<TransmitlyConfiguration>();

			// Add services to the container.
			builder.Services
				.AddControllers()
				.AddJsonOptions(opt =>
				{
					opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
					opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
					opt.JsonSerializerOptions.Converters.Add(new JsonExceptionConverter());

				});

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.ExampleFilters();
				c.SupportNonNullableReferenceTypes();
				c.SchemaFilter<SkipExceptionSchemaFilter>();
			});
			builder.Services.AddSwaggerExamplesFromAssemblyOf(typeof(Program));

			//todo: move to AspNetCore.Mvc package
			builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, ChannelProviderAdaptorModelBinder>();

			// The Transmitly.Microsoft.Extensions.DependencyInjection
			// package is used to wire everything up using Microsoft's Dependency injection.
			// Alternatively, you can use the CommunicationsClientBuilder directly
			// to configure Transmitly.
			builder.Services.AddTransmitly(tly =>
			{
				// Configure channel providers loaded from appsettings.json
				AddMailKitSupport(tly, tlyConfig);
				AddTwilioSupport(tly, tlyConfig);
				AddInfobipSupport(tly, tlyConfig);
				AddFirebaseSupport(tly, tlyConfig);

				//AddFluidTemplateEngine comes from the Transmitly.TemplateEngine.Fluid
				//This will give us the ability to replace and generate content in our templates.
				//In this case, we can use Fluid style templating.
				tly.AddFluidTemplateEngine(options => { })
				// We can use delivery report handlers for doing things like logging and storing
				// the generated communications even firing off webhooks to notify other systems.
				.AddDeliveryReportHandler((report) =>
				{
					logger?.LogInformation("[{channelId}:{channelProviderId}:Dispatched] Id={id}; Content={communication}", report.ChannelId, report.ChannelProviderId, report.CommunicationId, JsonSerializer.Serialize(report.ChannelCommunication));
					return Task.CompletedTask;

					// There's quite a few potential delivery events that can happen with Transmitly.
					// (see: DeliveryReportEvent.Name -- it's an extension method that any packages can extend!)
					// Here, we're filtering out all events except for 'Dispatched' events.
					// If we didn't add any filters, we'd see every report in this handler.
				}, [DeliveryReport.Event.Dispatched()])
				.AddDeliveryReportHandler((report) =>
				{
					logger?.LogInformation("[{channelId}:{channelProviderId}:StatusChanged] Id={id}; Status={status}", report.ChannelId, report.ChannelProviderId, report.CommunicationId, report.DispatchStatus);
					return Task.CompletedTask;
				}, [DeliveryReport.Event.StatusChanged()])
				// You can also filter out on other conditions like, channels and channel provider ids
				// for example you can have a handler specifically for email communications and another
				// that will handle SMS and push
				.AddDeliveryReportHandler((report) =>
				{
					logger?.LogError("[{channelId}:{channelProvider}:Error] Id={id}; Content={communication}", report.ChannelId, report.ChannelProviderId, report.CommunicationId, JsonSerializer.Serialize(report.ChannelCommunication));
					return Task.CompletedTask;
					//we're filtering out all events except for 'Error' events
				}, [DeliveryReport.Event.Error()])
				//Pipelines are the heart of Transmitly. Pipelines allow you to define your communications
				//as a domain action. This allows your domain code to stay agnostic to the details of how you
				//may send out a transactional communication.
				//For example, our first pipeline defines an Email and Sms. 
				//Transmitly will take care of the details of which channel to dispatch and which channel provider to use.
				//All you need to provide is the audience data and Transmitly takes care of the rest!
				//See: Dispatch route in Controllers.CommunicationsController.cs
				.AddPipeline(PipelineName.FirstPipeline, pipeline =>
				{
					// By default pipelines use a "First Match" dispatching strategy.
					// For example: your recipient has an email and sms address. This pipeline would
					// Send only the email channel, falling back to the sms if the email channel were to fail.
					// Out of the box there are multiple dispatch strategies. pipeline. "Any Match" would dispatch
					// using any channel that matched requirements (See: pipeline.UseAnyMatchPipelineDeliveryStrategy()).


					//AddEmail is a channel that is core to the Transmitly library.
					//AsAudienceAddress() is also a convenience method that helps us create an audience address
					//Audience addresses can be anything, email, phone, or even a device/app Id for push notifications!
					pipeline.AddEmail(tlyConfig.DefaultEmailFromAddress.AsAudienceAddress("The Transmit.ly group"), email =>
					{
						//Transmitly is a bit different. All of our content is supported by templates out of the box.
						//There are multiple types of templates to get you started. You can even create templates 
						//specific to certain cultures!
						email.Subject.AddStringTemplate("Hey {{firstName}}, Check out Transmit.ly! " + Emoji.Robot);
						email.HtmlBody.AddStringTemplate("Hey <strong>{{firstName}}</strong>, check out <a href=\"https://transmit.ly\" title=\"Transmit.ly\">this cool new library</a> for managing transactional communications in your app.");
						email.TextBody.AddStringTemplate("Hey, check out this cool new library. https://transmitly.ly");
					});

					//AddSms is a channel that is core to the Transmitly library.
					pipeline.AddSms(tlyConfig.DefaultSmsFromAddress.AsAudienceAddress(), sms =>
					{
						sms.Message.AddStringTemplate($"Check out Transmit.ly! {Emoji.Robot}");
						sms.DeliveryReportCallbackUrl = "https://scenes-babes-belgium-earned.trycloudflare.com/communications/channel/provider/update";

					});
				})
				//See: OTPCode route in Controllers.CommunicationsController.cs
				.AddPipeline(PipelineName.OtpCode, pipeline =>
				{
					//we want to notify any of our channel providers that support it, to prioritize this message
					//this is different than MessagePriority. Where MessagePriority indicates the importance to the recipient
					pipeline.TransportPriority = TransportPriority.High;

					pipeline.AddEmail(tlyConfig.DefaultEmailFromAddress.AsAudienceAddress(), email =>
					{
						email.Subject.AddStringTemplate("Your one time password code");
						email.HtmlBody.AddStringTemplate("Your code: <strong>{{code}}</strong>");
						email.TextBody.AddStringTemplate("Your code: {{code}}");

						//Already have emails defined with SendGrid? Great, we can handle those too!
						// We can use the extended properties provided by the SendGrid channel provider.
						// This way we ensure that if the SendGrid channel provider is used for this channel, we'll use the template id.
						// if we happen to fallback or even remove SendGrid, we can gracefully fallback to our content defined above. Neat!
						email.SendGrid().TemplateId = "d-89ae21e8ebed491380ed580f30e0b052";

						// While not required, we can specify channel providers that are allowed to 
						// handle this communication. In this case, we might want to use our secure
						// smtp server to send out our OTP codes. Another use-case would be using a schremsII
						// smtp server to comply with GDRP rules.
					});//}, Id.ChannelProvider.MailKit("secure-server"));

					pipeline.AddPushNotification(push =>
					{
						push.Title.AddStringTemplate("One time password code");
						push.Body.AddStringTemplate("Your OTP code: {{code}}");

					});
				})
				.AddPipeline(PipelineName.AppointmentReminder, pipeline =>
				{
					pipeline.AddVoice(tlyConfig.DefaultVoiceFromAddress.AsAudienceAddress(), voice =>
					{
						voice.Message.AddStringTemplate(
							"""
								Hello {{firstName}} <break strength="weak" /> this is a reminder about an upcoming doctors 
								appointment scheduled for Today <break strength="strong"/> at <say-as interpret-as="time" format="hms12">2:30pm</say-as>.
								Don't be late!
							"""
							);
						//voice.DeliveryReportCallbackUrl = "https://scenes-babes-belgium-earned.trycloudflare.com/communications/callback/status";
					});
				});
			});

			// Check out the Controllers/CommunicationsController.cs for an example of
			// how you can take advantage of everything we've configured here.
			var app = builder.Build();

			logger = app.Services.GetService<ILogger<Program>>();
			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();

			app.MapControllers();

			app.Run();
		}

		private static void AddFirebaseSupport(CommunicationsClientBuilder tly, TransmitlyConfiguration tlyConfig)
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
		}

		private static void AddInfobipSupport(CommunicationsClientBuilder tly, TransmitlyConfiguration tlyConfig)
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
		}

		private static void AddTwilioSupport(CommunicationsClientBuilder tly, TransmitlyConfiguration tlyConfig)
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
		}

		private static void AddMailKitSupport(CommunicationsClientBuilder tly, TransmitlyConfiguration tlyConfig)
		{
			foreach (var smtpSetting in tlyConfig.ChannelProviders.Smtp.Where(s => s.IsEnabled))
			{
				// Adding the Transmitly.ChannelProvider.MailKit package
				// allows us to add support to our app for Email via SMTP
				// through the MailKit library. 
				tly.AddMailKitSupport(mailkit =>
				{
					mailkit.Host = smtpSetting.Host;
					mailkit.UseSsl = smtpSetting.UseSsl;
					mailkit.Port = smtpSetting.Port;
					mailkit.UserName = smtpSetting.Username;
					mailkit.Password = smtpSetting.Password;
				}, smtpSetting.Id);
			}
		}
	}
}
