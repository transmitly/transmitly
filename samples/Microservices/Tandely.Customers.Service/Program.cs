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
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Transmitly.Samples.Shared;

namespace Tandely.Customers.Service
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllers(options =>
			{
				var noContentFormatter = options.OutputFormatters.OfType<HttpNoContentOutputFormatter>().FirstOrDefault();
				if (noContentFormatter != null)
				{
					noContentFormatter.TreatNullValueAsNoContent = false;
				}
			}).AddJsonOptions(options =>
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

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options =>
			{
				var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

				// Enable annotations (optional)
				options.EnableAnnotations();
			});

			builder.Services.AddSingleton<CustomerRepository>();
			builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
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
