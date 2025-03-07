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
using Swashbuckle.AspNetCore.Filters;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tandely.Notifications.Client;
using Transmitly;
using Transmitly.Samples.Shared;

namespace Tandely.Orders.Service
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
				options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
			});

			builder.Services.Configure<JsonOptions>(options =>
			{
				options.SerializerOptions.PropertyNameCaseInsensitive = true;
				options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				options.SerializerOptions.Converters.Add(new JsonExceptionConverter());
				options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
			});

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var notificationsServiceUrl = Guard.AgainstNullOrWhiteSpace(builder.Configuration.GetValue<string>("NotificationsService:Url"));
			var apiKey = builder.Configuration.GetValue<string?>("NotificationsService:ApiKey");

			var clientBuilder = new CommunicationsClientBuilder();

			clientBuilder.UseTandelyNotifications(options =>
			{
				options.BasePath = new Uri(notificationsServiceUrl);
				options.ApiKey = apiKey;
			});

			var client = clientBuilder.BuildClient();

			builder.Services.AddSingleton(client);

			builder.Services.AddSwaggerGen(c =>
			{
				c.ExampleFilters();
				c.SupportNonNullableReferenceTypes();
				c.SchemaFilter<SkipExceptionSchemaFilter>();
			});
			builder.Services.AddSwaggerExamplesFromAssemblyOf(typeof(Program));

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
