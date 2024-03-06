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
using Swashbuckle.AspNetCore.Filters;
using System.Text.Json;
using System.Text.Json.Serialization;
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
			const string defaultFromAddress = "demo@example.com";
			// Add services to the container.
			builder.Services.AddControllers().AddJsonOptions(opt =>
			{
				opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
				opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
			});

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.ExampleFilters();
				c.SupportNonNullableReferenceTypes();
			});
			builder.Services.AddSwaggerExamplesFromAssemblyOf(typeof(Program));

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
				// We can use delivery report handlers for doing things like loggging and storing
				// the generated communications even firing off webhooks to notify other systems.
				.AddDeliveryReportHandler((report) =>
				{
					logger?.LogInformation($"[{report.ChannelId}:{report.ChannelProviderId}:Dispatched] {JsonSerializer.Serialize(report.ChannelCommunication)}");
					return Task.CompletedTask;

					// There's quite a few potential delivery events that can happen with Transmitly.
					// (see: DeliveryReportEvent.Name -- it's an extension method that any packages can extend!)
					// Here, we're filtering out all events except for 'Dispatched' events.
					// If we didn't add any filters, we'd see every report in this handler.
				}, [DeliveryReportEvent.Name.Dispatched()])
				// You can also filter out on other conditions like, channels and channel provider ids
				// for example you can have a handler specifically for email communications and another
				// that will handle SMS and push
				.AddDeliveryReportHandler((report) =>
				{
					logger?.LogError($"[{report.ChannelId}:{report.ChannelProviderId}:Error] {JsonSerializer.Serialize(report.ChannelCommunication)}");
					return Task.CompletedTask;
					//we're filtering out all events except for 'Error' events
				}, [DeliveryReportEvent.Name.Error()])
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
					pipeline.AddEmail(defaultFromAddress.AsAudienceAddress("The Transmit.ly group"), email =>
					{
						//Transmitly is a bit different. All of our content is supported by templates out of the box.
						//There are multiple types of templates to get you started. You can even create templates 
						//specific to certain cultures!
						email.Subject.AddStringTemplate("Hey {{firstName}}, Check out Transmit.ly! " + Emoji.Robot);
						email.HtmlBody.AddStringTemplate("Hey <strong>{{firstName}}</strong>, check out <a href=\"https://transmit.ly\" title=\"Transmit.ly\">this cool new library</a> for managing transactional communications in your app.");
						email.TextBody.AddStringTemplate("Hey, check out this cool new library. https://transmitly.ly");
					});

					//AddSms is a channel that is core to the Transmitly library.
					pipeline.AddSms(sms =>
					{
						sms.Body.AddStringTemplate("Check out Transmit.ly!");
					});
				})
				//See: OTPCode route in Controllers.CommunicationsController.cs
				.AddPipeline(PipelineName.OtpCode, pipeline =>
				{
					//we want to notify any of our channel providers that support it, to prioritize this message
					//this is different than MessagePriority. Where MessagePriority indicates the importance to the recipient
					pipeline.TransportPriority = TransportPriority.High;

					pipeline.AddEmail(defaultFromAddress.AsAudienceAddress(), email =>
					{
						email.Subject.AddStringTemplate("Your one time password code");
						email.HtmlBody.AddStringTemplate("Your code: <strong>{{code}}");
						email.TextBody.AddStringTemplate("Your code: {{code}}");

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
				//Already have emails defined with SendGrid? Great, we can handle those too!
				.AddPipeline(PipelineName.SendGridTemplate, pipeline =>
				{
					// by default, this channel will only be used if there is a SendGrid channel provider defined.
					//Don't forget to replace the templateId!
					pipeline.AddSendGridTemplateEmail(defaultFromAddress.AsAudienceAddress(), "<templateId>", sendGrid => { });
					// a nice byproduct of the above line, is that we can seamlessly use another channel provider if we decide to move away from SendGrid (or SendGrid is down)
					// we're restricting this to only use the default mailkit or infobip channel providers
					pipeline.AddEmail(defaultFromAddress.AsAudienceAddress(), email =>
					{
						email.Subject.AddStringTemplate("A subject that matches the SendGrid Template");
						email.HtmlBody.AddStringTemplate("A body that matches the SendGrid Template");
					}, Id.ChannelProvider.MailKit(), Id.ChannelProvider.Infobip());
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
				// allows us to add support to our app for Email & SMS through
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
