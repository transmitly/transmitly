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

using System.Text;

namespace Transmitly.Logging;

internal sealed class ConsoleLoggerFactory : ILoggerFactory
{
	public ILogger CreateLogger(string categoryName)
	{
		return new ConsoleLogger(categoryName);
	}

	private sealed class ConsoleLogger(string categoryName) : ILogger
	{
		public string CategoryName { get; } = Guard.AgainstNullOrWhiteSpace(categoryName);

		public bool IsEnabled(LogLevel level) => level != LogLevel.None;

		public void Log(LogEntry entry)
		{
			Guard.AgainstNull(entry);

			var builder = new StringBuilder(256);
			builder.Append(entry.Timestamp.ToUniversalTime().ToString("O"));
			builder.Append(" [");
			builder.Append(entry.Level);
			builder.Append("] ");
			builder.Append(entry.Category);
			builder.Append(' ');
			builder.Append(entry.EventName);
			builder.Append(": ");
			builder.Append(entry.Message);

			foreach (var property in entry.Properties)
			{
				builder.Append(" | ");
				builder.Append(property.Key);
				builder.Append('=');
				builder.Append(property.Value);
			}

			if (entry.Exception != null)
			{
				builder.Append(" | exception=");
				builder.Append(entry.Exception.GetType().FullName);
				builder.Append(": ");
				builder.Append(entry.Exception.Message);
			}

			var message = builder.ToString();
			if (entry.Level >= LogLevel.Warning)
				Console.Error.WriteLine(message);
			else
				Console.WriteLine(message);
		}
	}
}
