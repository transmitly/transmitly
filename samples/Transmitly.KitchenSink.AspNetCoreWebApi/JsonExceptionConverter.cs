﻿// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
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
	public sealed class JsonExceptionConverter : JsonConverter<Exception>
	{
		public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}

		public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
		{
			writer.WriteRawValue(JsonSerializer.Serialize(new LimitedExceptionDetail(value)));
		}
	}

	//Source = https://stackoverflow.com/a/72968664
	sealed class LimitedExceptionDetail
	{
		public LimitedExceptionDetail() { }

		internal LimitedExceptionDetail(Exception exception)
		{
			Type = exception.GetType().FullName;
			Message = exception.Message;
			Source = exception.Source;
#if DEBUG
			StackTrace = exception.StackTrace;
			if (exception.InnerException is not null)
			{
				InnerException = new LimitedExceptionDetail(exception.InnerException);
			}
#endif
		}

		public string? Type { get; set; }
		public string? Message { get; set; }
		public string? Source { get; set; }
		public string? StackTrace { get; set; }
		public LimitedExceptionDetail? InnerException { get; set; }
	}
}