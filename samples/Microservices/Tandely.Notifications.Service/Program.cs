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
using Tandely.IntegrationEvents;
using Transmitly;
using Transmitly.ChannelProvider.Debugging;

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

            builder.Services.AddTransmitly(tly =>
            {
                tly
                .AddPlatformIdentityResolver<CustomerRepository>()
                .AddScribanTemplateEngine()
                .AddDispatchLoggingSupport()
                .AddSmtpSupport(smtp =>
                {
                    smtp.Host = "smtp.example.com";
                    smtp.UserName = "test";
                    smtp.Password = "test";
                })

                .AddPipeline(ShippingIntegrationEvent.OrderShipped, pipeline =>
                {
                    pipeline.AddEmail("from@domain.com".AsIdentityAddress(), email =>
                    {
                        email.Subject.AddStringTemplate("Order #{trx.orderNumber} has shipped!");
                        email.HtmlBody.AddStringTemplate("Hey {aud.DisplayName}, good news! Your order has shipped!");
                    });
                })
                .AddPipeline(OrdersIntegrationEvent.OrderConfirmation, pipeline =>
                {
                    pipeline.AddEmail("from@domain.com".AsIdentityAddress(), email =>
                    {
                        email.Subject.AddStringTemplate("Thank you for your order, #{Order.Id}!");
                        email.HtmlBody.AddStringTemplate("Testing body!");
                    });

                    pipeline.AddSms("88812345678".AsIdentityAddress(), sms =>
                    {
                        sms.Message.AddStringTemplate("Hey VIP! Testing message!");
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
