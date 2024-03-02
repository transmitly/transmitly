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

namespace Transmitly.KitchenSink.AspNetCoreWebApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			ILogger? logger = null;
			// Add services to the container.

			builder.Services.AddControllers().AddJsonOptions(opt => opt.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull);
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.ExampleFilters();
				c.SupportNonNullableReferenceTypes();
			});
			builder.Services.AddSwaggerExamplesFromAssemblyOf(typeof(Program));
			// We're using the Transmitly.Microsoft.Extensions.DependencyInjection
			// package to wire everything up using Microsoft's Dependency injection.
			// Alternatively, you can use the CommunicationsClientBuilder directly
			// to configure Transmitly.
			builder.Services.AddTransmitly(tly =>
			{
				// Adding the Transmitly.ChannelProvider.MailKit package
				// allows us to add support to our app for Email via SMTP
				// through the MailKit library. 
				tly.AddMailKitSupport(mailkit =>
				{
					mailkit.Host = "smtp.domain.com";
					mailkit.UseSsl = true;
					mailkit.Port = 587;
					mailkit.UserName = "<username>";
					mailkit.Password = "<password>";
				})
				// Adding the Transmitly.ChannelProvider.Twilio package
				// allows us to add support to our app for SMS through
				// an account with Twilio.
				.AddTwilioSupport(twilio =>
				{
					twilio.AuthToken = "<authToken>";
					twilio.AccountSid = "<Sid>";
				})
				// Adding the Transmitly.ChannelProvider.Infobip package
				// allows us to add support to our app for Email & SMS through
				// an account with Infobip.
				.AddInfobipSupport(infobip =>
				{
					infobip.BasePath = "https://base.infobip.com";
					infobip.ApiKey = "key";
					infobip.ApiKeyPrefix = "App";
				})
				//AddFluidTemplateEngine comes from the Transmitly.TemplateEngine.Fluid
				//This will give us the ability to replace and generate content in our templates.
				//In this case, we can use Fluid style templating.
				.AddFluidTemplateEngine(options => { })
				// We can use delivery report handlers for doing things like loggging and storing
				// the generated communications even firing off webhooks to notify other systems.
				.AddDeliveryReportHandler((report) =>
				{
					logger?.LogInformation($"[{report.ChannelId} - {report.ChannelProviderId}] Content={JsonSerializer.Serialize(report.ChannelCommunication)}");
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
					logger?.LogError($"[{report.ChannelId} - {report.ChannelProviderId}] {JsonSerializer.Serialize(report.ChannelCommunication)}");
					return Task.CompletedTask;
					//we're filtering out all events except for 'Error' events
				}, [DeliveryReportEvent.Name.Error()])
				//Pipelines are the heart of Transmitly. Pipelines allow you to define your communications
				//as a domain action. This allows your domain code to stay agnostic to the details of how you
				//may send out a transactional communication.
				//For example, our first pipeline defines an Email and Sms. 
				//Transmitly will take care of the details of which channel to dispatch and which channel provider to use.
				//All you need to provide is the audience data and Transmitly takes care of the rest!
				.AddPipeline("first-pipeline", pipeline =>
				{
					//AddEmail is a channel that is core to the Transmitly library.
					//AsAudienceAddress() is also a convenience method that helps us create an audience address
					//Audience addresses can be anything, email, phone, or even a device/app Id for push notifications!
					pipeline.AddEmail("demo@domain.com".AsAudienceAddress("The Transmit.ly guy"), email =>
					{
						//Transmitly is a bit different. All of our content is supported by templates out of the box.
						//There are multiple types of templates to get you started. You can even create templates 
						//specific to certain cultures!
						email.Subject.AddStringTemplate("Hey {{firstName}}, Check out Transmit.ly! " + Emoji.Robot);
						email.HtmlBody.AddStringTemplate("Hey <strong>{{firstName}}</strong>, check out this cool new library for managing app communications. https://transmit.ly");
						email.TextBody.AddStringTemplate("Hey, check out this cool new library. https://transmitly.ly");
					});

					//AddSms is a channel that is core to the Transmitly library.
					pipeline.AddSms(sms =>
					{
						sms.Body.AddStringTemplate("Check out Transmit.ly!");
					});
				});
			});

			// Since we're using DI, we can now add a ICommunicationsClient to the 
			// constructor of our objects. 
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
	}
}
