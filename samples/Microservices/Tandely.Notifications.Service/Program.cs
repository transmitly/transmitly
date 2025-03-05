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
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Tandely.IntegrationEvents;
using Transmitly;

namespace Tandely.Notifications.Service
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			// Add services to the container.
			builder.Services.AddControllers().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
				options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				options.JsonSerializerOptions.Converters.Add(new JsonExceptionConverter());
			});
			builder.Services.Configure<JsonOptions>(options =>
			{
				options.SerializerOptions.PropertyNameCaseInsensitive = true;
				options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				options.SerializerOptions.Converters.Add(new JsonExceptionConverter());
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
			builder.Services.AddTransmitly(tly =>
			{
				tly
				.AddPlatformIdentityResolver<CustomerRepository>("Customer")
				.AddDeliveryReportHandler((report) =>
				{
					logger.CreateLogger<Program>().LogInformation("[DeliveryReport] Dispatched to {ChannelId} with result {DispatchStatus}", report.ChannelId, report.DispatchStatus);
					return Task.CompletedTask;
				})
				.AddFluidTemplateEngine()
				.AddDispatchLoggingSupport(options =>
				{
					options.SimulateDispatchResult = true;
				})
				.AddPipeline(ShippingIntegrationEvent.OrderShipped, pipeline =>
				{
					pipeline
					.AddSms("+18881234567".AsIdentityAddress(), sms =>
					{
						sms.Message.AddStringTemplate("{{aud.FirstName}}, #{{OrderId}} has shipped with {{Carrier}}! You can track it <a href=\"https://shipping.example.com/?track={{TrackingNumber}}\">here</a>");
					})
					.AddPushNotification(push =>
					{
						push.Title.AddStringTemplate("{{aud.FirstName}}, your order shipped!");
						push.Body.AddStringTemplate("Your Tandely order, #{{OrderId}} has shipped with {{Carrier}}! ");
					})
					.AddEmail("from@example.com".AsIdentityAddress(), email =>
					{
						email.Subject.AddStringTemplate("Tandely order, {{OrderId}}, shipped!");
						email.HtmlBody.AddStringTemplate("{{aud.FirstName}}, your Tandely order, #{{OrderId}} has shipped with {{Carrier}}! " +
							"You can track it <a href=\"https://shipping.example.com/?track={{TrackingNumber}}\">here</a>" +
							"<p>You now have a total of {{aud.LoyaltyPoints}} loyalty points!</p>");
						email.TextBody.AddStringTemplate("Your Tandely order, #{{OrderId}} has shipped with {{Carrier}}! You can track it at https://shipping.example.com/?track={{TrackingNumber}}");
					})
					.UseFirstMatchPipelineDeliveryStrategy();
				})
				.AddPipeline(OrdersIntegrationEvent.OrderConfirmation, pipeline =>
				{
					pipeline
					.AddEmail("from@example.com".AsIdentityAddress(), email =>
					{
						email.Subject.AddStringTemplate("Thank you for your order, {{aud.FirstName}}");
						email.HtmlBody.AddTemplateResolver(async ctx=>{
							
							var result = await new HttpClient().GetStringAsync("https://raw.githubusercontent.com/transmitly/transmitly/0289de19f3f97cfb85fe6e18b5e8c4d54c8ed4a9/samples/templates/liquid/invoice.html");
							if(string.IsNullOrWhiteSpace(result))
								return "Your total for your order, #{{Order.Id}} is <strong>${{Order.Total}}</strong> which brings you to <strong>{{aud.LoyaltyPoints}}</strong> loyalty points!";
							return result;



						});
						//email.HtmlBody.AddStringTemplate("Your total for your order, #{{Order.Id}} is <strong>${{Order.Total}}</strong> which brings you to <strong>{{aud.LoyaltyPoints}}</strong> loyalty points!");
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
