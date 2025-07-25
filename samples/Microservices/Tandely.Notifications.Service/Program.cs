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

using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tandely.IntegrationEvents;
using Transmitly;
using Transmitly.Exceptions;
using Transmitly.Samples.Shared;
using Transmitly.Util;

namespace Tandely.Notifications.Service
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			/// !!! See appSettings.json for examples of how to configure with your own settings !!!
			var tlyConfig = builder.Configuration.GetRequiredSection("Transmitly").Get<TransmitlyConfiguration>();
			if (tlyConfig == null)
				throw new CommunicationsException("Transmitly configuration section is missing from app configuration.");

			// Add services to the container.
			builder.Services.AddControllers().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
				options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				options.JsonSerializerOptions.Converters.Add(new JsonExceptionConverter());
				options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
				options.JsonSerializerOptions.Converters.Add(new ObjectToInferredTypesConverter());
			});

			builder.Services.Configure<JsonOptions>(options =>
			{
				options.SerializerOptions.PropertyNameCaseInsensitive = true;
				options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				options.SerializerOptions.Converters.Add(new JsonExceptionConverter());
				options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
				options.SerializerOptions.Converters.Add(new ObjectToInferredTypesConverter());
			});

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var customerServiceUrl = Guard.AgainstNullOrWhiteSpace(builder.Configuration.GetValue<string>("CustomerService:Url"));
			var apiKey = builder.Configuration.GetValue<string?>("CustomerService:ApiKey");

			builder.Services.AddHttpClient<CustomerRepository>(c =>
			{
				c.BaseAddress = new Uri(customerServiceUrl);
				c.DefaultRequestHeaders.Add("x-tandely-api-key", apiKey);
			});

			var logger = LoggerFactory.Create((c) => c.AddConsole().AddDebug());
			var defaultLogger = logger.CreateLogger<Program>();
			builder.Services.AddTransmitly(tly =>
			{
				// Configure channel providers loaded from appsettings.json
				tly
				.AddDispatchLoggingSupport(tlyConfig)//Console logging
				.AddSmtpSupport(tlyConfig)//Email
				.AddTwilioSupport(tlyConfig)//Email/SMS
				.AddInfobipSupport(tlyConfig)//Email/Sms/Voice
				.AddFirebaseSupport(tlyConfig)//Push
				.AddSendGridSupport(tlyConfig)//Email
				.AddPlatformIdentityResolver<CustomerRepository>("Customer")
				.AddDeliveryReportHandler((report) =>
				{
					defaultLogger.LogInformation("SendGrid MessageId: {SendGridMessageId}", report.SendGrid().Email.MessageId);
					defaultLogger.LogInformation("[DeliveryReport] Dispatched to {ChannelId} using {ChannelProvider} with result {DispatchStatus}", report.ChannelId, report.ChannelProviderId, report.Status);
					return Task.CompletedTask;
				})
				.AddFluidTemplateEngine()
				.AddPersona<Customer>("VIP", nameof(Customer), (c) => c.LoyaltyPoints > 500)
				.AddPipeline(ShippingIntegrationEvent.OrderShipped, pipeline =>
				{
					pipeline
					.AddSms(tlyConfig.DefaultSmsFromAddress.AsIdentityAddress(), sms =>
					{
						sms.Message.AddStringTemplate("{{pid.FirstName}}, #{{OrderId}} has shipped with {{Carrier}}! You can track it @ https://shipping.example.com/?track={{TrackingNumber}}");
					})
					.AddEmail((ctx) =>
					{
						var contentModel = (IDictionary<string, object>?)ctx.ContentModel?.Model;
						return (contentModel?.TryGetValue("TenantId", out var tenantValue) ?? false && tenantValue?.ToString() == "tenant-1" ? "from@tenant-1.com" : tlyConfig.DefaultEmailFromAddress).AsIdentityAddress();
					},
					email =>
					{
						email.Subject.AddStringTemplate("Tandely order, {{OrderId}}, shipped!");
						email.HtmlBody.AddEmbeddedResourceTemplate("Tandely.Notifications.Service.templates.order_shipped.email.default.html");
						email.TextBody.AddStringTemplate("Your Tandely order, #{{OrderId}} has shipped with {{Carrier}}! You can track it at https://shipping.example.com/?track={{TrackingNumber}}");
					})
					.UseFirstMatchPipelineDeliveryStrategy();
				})
				.AddPipeline(ShippingIntegrationEvent.OrderShipped, pipeline =>
				{
					pipeline.AddPersonaFilter("VIP");
					pipeline
					.AddPushNotification(push =>
					 {
						 push.Title.AddStringTemplate("{{pid.FirstName}}, your VIP order shipped!");
						 push.Body.AddStringTemplate("Your VIP Tandely order, #{{OrderId}} has shipped with {{Carrier}}! ");
					 }).
					 AddVoice(tlyConfig.DefaultVoiceFromAddress.AsIdentityAddress(), voice =>
					 {
						 voice.Message.AddStringTemplate("{{pid.FirstName}}, your personal shopper will be contacting you shortly. In the meantime, we're happy to announce your order #{{OrderId}} has shipped with {{Carrier}}! ");
					 });

				})
				.AddPipeline(OrdersIntegrationEvent.OrderConfirmation, pipeline =>
				{
					pipeline
					.AddEmail(tlyConfig.DefaultEmailFromAddress.AsIdentityAddress(), email =>
					{
						email.Subject.AddStringTemplate("Thank you for your order, {{pid.FirstName}}");
						email.HtmlBody.AddTemplateResolver(async ctx =>
						{
							var result = await new HttpClient()
							.GetStringAsync("https://raw.githubusercontent.com/transmitly/transmitly/0289de19f3f97cfb85fe6e18b5e8c4d54c8ed4a9/samples/templates/liquid/invoice.html");

							if (string.IsNullOrWhiteSpace(result))
								return "Your total for your order, #{{Order.Id}} is <strong>${{Order.Total}}</strong> which brings you to <strong>{{pid.LoyaltyPoints}}</strong> loyalty points!";
							return result;
						});
					});
				});
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();

			app.MapControllers();

			app.Run();
		}
	}
}
