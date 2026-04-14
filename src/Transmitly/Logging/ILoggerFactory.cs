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
/// Defines a factory for creating loggers.
/// </summary>
public interface ILoggerFactory
{
	/// <summary>
	/// Creates a new logger instance for the specified category name.
	/// </summary>
	/// <remarks>The returned logger can be used to write log messages that are associated with the given category.
	/// Category names are used to organize and filter log output.</remarks>
	/// <param name="categoryName">The category name for messages produced by the logger. This is typically the fully qualified class name or another
	/// logical grouping. Cannot be null.</param>
	/// <returns>An ILogger instance that logs messages for the specified category.</returns>
	ILogger CreateLogger(string categoryName);
}
