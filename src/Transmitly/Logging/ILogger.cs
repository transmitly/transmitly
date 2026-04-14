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
/// Defines a logger that can be used to write log messages with various levels of severity and associated properties. 
/// </summary>
public interface ILogger
{
	/// <summary>
	/// Gets the name of the category associated with this instance.
	/// </summary>
	string CategoryName { get; }
	/// <summary>
	/// Checks if the given log level is enabled for this logger. This can be used to avoid unnecessary processing of log messages when the log level is not enabled. 
	/// </summary>
	/// <param name="level">The log level to check if the logger is enabled for.</param>
	/// <returns></returns>
	bool IsEnabled(LogLevel level);
	/// <summary>
	/// Writes a log entry with the specified properties.
	/// </summary>
	/// <param name="entry">The entry to log.</param>
	void Log(LogEntry entry);
}
