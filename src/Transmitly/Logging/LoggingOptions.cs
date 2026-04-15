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

namespace Transmitly.Logging;

/// <summary>
/// Provides options for configuring Transmitly logging.
/// </summary>
public sealed class LoggingOptions
{
	/// <summary>
	/// Gets or sets the minimum log level that will be emitted.
	/// </summary>
	public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

	/// <summary>
	/// Gets or sets the logger factory used to create loggers for Transmitly components.
	/// </summary>
	/// <remarks>If not set, a default <see cref="ConsoleLoggerFactory"/> will be used.</remarks>
	public ILoggerFactory? LoggerFactory { get; set; }
}
