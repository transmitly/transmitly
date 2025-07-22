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

namespace Transmitly.Samples.Shared
{
	//Source=https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-9-0#deserialize-inferred-types-to-object-properties
	public sealed class ObjectToInferredTypesConverter : JsonConverter<object>
	{
		public override object Read(
			ref Utf8JsonReader reader,
			Type typeToConvert,
			JsonSerializerOptions options) => reader.TokenType switch
			{
				JsonTokenType.True => true,
				JsonTokenType.False => false,
				JsonTokenType.Number when reader.TryGetInt64(out long l) => l,
				JsonTokenType.Number => reader.GetDouble(),
				JsonTokenType.String when reader.TryGetDateTime(out DateTime datetime) => datetime,
				JsonTokenType.String => reader.GetString()!,
				_ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
			};

		public override void Write(
			Utf8JsonWriter writer,
			object objectToWrite,
			JsonSerializerOptions options)
		{
			var runtimeType = objectToWrite.GetType();
			if (runtimeType == typeof(object))
			{
				writer.WriteStartObject();
				writer.WriteEndObject();
				return;
			}

			JsonSerializer.Serialize(writer, objectToWrite, runtimeType, options);
		}
	}
}