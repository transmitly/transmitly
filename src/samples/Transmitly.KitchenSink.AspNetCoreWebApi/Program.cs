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
using Transmitly;
using Transmitly.Channel.Push;
using Transmitly.ChannelProvider.Twilio;
using static Transmitly.KitchenSink.AspNetCoreWebApi.Controllers.CommunicationsController;
using System.Text.Json;
using Newtonsoft.Json;
namespace Transmitly.KitchenSink.AspNetCoreWebApi
{
	public class DispatchVMExample : IExamplesProvider<DispatchVM>
	{
		public DispatchVM GetExamples()
		{
			return new DispatchVM
			{
				PipelineName = "first-pipeline",
				AllowedChannelIds = null,
				ContentModel = new DispatchContentModel
				{
					Model = new
					{
						firstName = "Mit",
						lastName = "Ly",
						date = DateTime.UtcNow,
						amount = 100.11,
						currency = "$"
					}
				},
				Culture = null,
				Recipients = new List<DispatchAudience>{
					 new DispatchAudience(){
						 Addresses = new List<DispatchAudienceAddress>{
							new DispatchAudienceAddress{ Value = "example@domain.com", Display="Example Display" },
							new DispatchAudienceAddress{ Value = "+18885551234" },
							new DispatchAudienceAddress{ Value = "fe595523a0c2965f9eabff921555df48-80df133c-5aab-4db4-bd03-b04331181664", Type=AudienceAddress.Types.DeviceToken() }
						 }
					 }
				}

			};
		}
	}
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

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
			// package to wire everything up using Microsoft Dependency injection
			// Alternatively, you can use the CommunicationsClientBuilder directly
			// to configure Transmitly.
			// We can use it in our app by adding a ICommunicationsClient in our constructors
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
					mailkit.UserName = "uname";
					mailkit.Password = "pw";
				})
				//.AddTwilioSupport(twilio =>
				//{
				//	twilio.AuthToken = "<authToken>";
				//	twilio.AccountSid = "<Sid>";
				//})
				//.AddInfobipSupport(infobip =>
				//{
				//	infobip.BasePath = "https://base.infobip.com";
				//	infobip.ApiKey = "key";
				//	infobip.ApiKeyPrefix = "App";
				//})
				//Pipelines are the heart of Transmitly. Pipelines allow you to define your communications
				//as a domain action. This allows your domain code to stay agnostic to the details of how you
				//may send out a transactional communication.
				//For example, our first pipeline defines an Email and Sms. 
				//Transmitly will take care of the details of which channel to dispatch and which channel provider to use.
				//All you need to provide is the audience data and Transmitly takes care of the rest!
				.AddFluidTemplateEngine(options => { options.AllowFunctions = true; })
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
						email.Subject.AddStringTemplate("Hey {{firstName}}, Check out Transmit.ly!");
						email.HtmlBody.AddStringTemplate("Hey {{firstName}}, check out this cool new library for managing app communications. <a href=\"https://transmit.ly\">");
						email.TextBody.AddStringTemplate("Hey, check out this cool new library. https://transmitly.ly");
					});

					//AddSms is a channel that is core to the Transmitly library.
					pipeline.AddSms(sms =>
					{
						sms.Body.AddStringTemplate("Check out Transmit.ly!");
					});
				});
			});

			var app = builder.Build();

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
