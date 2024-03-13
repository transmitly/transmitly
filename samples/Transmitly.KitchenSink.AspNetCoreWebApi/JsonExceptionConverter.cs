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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Transmitly.KitchenSink.AspNetCoreWebApi
{
	//Source = https://stackoverflow.com/a/76797018
	public class JsonExceptionConverter : JsonConverter<Exception>
	{
		public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}

		public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteString(nameof(Exception.Message), value.Message);

#if DEBUG
			if (value.InnerException is not null)
			{
				writer.WriteStartObject(nameof(Exception.InnerException));
				Write(writer, value.InnerException, options);
				writer.WriteEndObject();
			}

			if (value.TargetSite is not null)
			{
				writer.WriteStartObject(nameof(Exception.TargetSite));
				writer.WriteString(nameof(Exception.TargetSite.Name), value.TargetSite?.Name);
				writer.WriteString(nameof(Exception.TargetSite.DeclaringType), value.TargetSite?.DeclaringType?.FullName);
				writer.WriteEndObject();
			}

			if (value.StackTrace is not null)
			{
				writer.WriteString(nameof(Exception.StackTrace), value.StackTrace);
			}
#endif

			writer.WriteString(nameof(Type), value.GetType().ToString());
			writer.WriteEndObject();
		}
	}
}
