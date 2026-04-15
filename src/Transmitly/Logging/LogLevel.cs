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
/// Represents the severity level of a log entry.
/// </summary>
public enum LogLevel
{
	/// <summary>
	/// Logs that contain the most detailed diagnostic information.
	/// </summary>
	Trace = 0,

	/// <summary>
	/// Logs that are useful for debugging.
	/// </summary>
	Debug = 1,

	/// <summary>
	/// Logs that track the general flow of application execution.
	/// </summary>
	Information = 2,

	/// <summary>
	/// Logs that highlight an abnormal or unexpected event that did not stop execution.
	/// </summary>
	Warning = 3,

	/// <summary>
	/// Logs that indicate a failure in the current operation.
	/// </summary>
	Error = 4,

	/// <summary>
	/// Logs that indicate a critical or unrecoverable failure.
	/// </summary>
	Critical = 5,

	/// <summary>
	/// Disables logging.
	/// </summary>
	None = 6
}
