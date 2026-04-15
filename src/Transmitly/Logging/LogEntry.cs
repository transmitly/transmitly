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

using System.Collections.ObjectModel;

namespace Transmitly.Logging;

/// <summary>
/// Represents a structured log entry.
/// </summary>
public sealed class LogEntry(
	DateTimeOffset timestamp,
	LogLevel level,
	string category,
	string eventName,
	string message,
	Exception? exception = null,
	IReadOnlyDictionary<string, object?>? properties = null,
	string? traceId = null,
	string? spanId = null,
	byte? traceFlags = null,
	string? traceState = null)
{
	private static readonly IReadOnlyDictionary<string, object?> EmptyProperties =
		new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());

	/// <summary>
	/// Gets the UTC timestamp when the log entry was created.
	/// </summary>
	public DateTimeOffset Timestamp { get; } = timestamp;

	/// <summary>
	/// Gets the severity level of the log entry.
	/// </summary>
	public LogLevel Level { get; } = level;

	/// <summary>
	/// Gets the logger category that produced the entry.
	/// </summary>
	public string Category { get; } = Guard.AgainstNullOrWhiteSpace(category);

	/// <summary>
	/// Gets the stable event name associated with the entry.
	/// </summary>
	public string EventName { get; } = Guard.AgainstNullOrWhiteSpace(eventName);

	/// <summary>
	/// Gets the rendered log message.
	/// </summary>
	public string Message { get; } = Guard.AgainstNullOrWhiteSpace(message);

	/// <summary>
	/// Gets the exception associated with the entry, if one was logged.
	/// </summary>
	public Exception? Exception { get; } = exception;

	/// <summary>
	/// Gets the structured properties associated with the entry.
	/// </summary>
	public IReadOnlyDictionary<string, object?> Properties { get; } = properties ?? EmptyProperties;

	/// <summary>
	/// Gets the trace identifier captured from the current activity, if available.
	/// </summary>
	public string? TraceId { get; } = traceId;

	/// <summary>
	/// Gets the span identifier captured from the current activity, if available.
	/// </summary>
	public string? SpanId { get; } = spanId;

	/// <summary>
	/// Gets the trace flags captured from the current activity, if available.
	/// </summary>
	public byte? TraceFlags { get; } = traceFlags;

	/// <summary>
	/// Gets the trace state captured from the current activity, if available.
	/// </summary>
	public string? TraceState { get; } = traceState;
}
