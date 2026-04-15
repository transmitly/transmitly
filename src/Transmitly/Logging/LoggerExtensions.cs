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
/// Provides helper extension methods for creating and writing log entries.
/// </summary>
public static class LoggerExtensions
{
	/// <summary>
	/// Creates a logger using the full name of <typeparamref name="T"/> as the category when available.
	/// </summary>
	/// <typeparam name="T">The type used to derive the logger category name.</typeparam>
	/// <param name="loggerFactory">The logger factory.</param>
	/// <returns>A logger for the specified type category.</returns>
	public static ILogger CreateLogger<T>(this ILoggerFactory loggerFactory) =>
		Guard.AgainstNull(loggerFactory).CreateLogger(typeof(T).FullName ?? typeof(T).Name);

	/// <summary>
	/// Writes a log entry for the provided logger when the specified level is enabled.
	/// </summary>
	/// <param name="logger">The logger to write to.</param>
	/// <param name="level">The severity level for the entry.</param>
	/// <param name="eventName">The stable event name for the entry.</param>
	/// <param name="message">The rendered log message.</param>
	/// <param name="exception">An optional exception associated with the entry.</param>
	/// <param name="properties">Optional structured properties to attach to the entry.</param>
	public static void Log(
		this ILogger logger,
		LogLevel level,
		string eventName,
		string message,
		Exception? exception = null,
		IReadOnlyDictionary<string, object?>? properties = null)
	{
		Guard.AgainstNull(logger);

		if (!logger.IsEnabled(level))
			return;

		var entry = new LogEntry(
			DateTimeOffset.UtcNow,
			level,
			logger.CategoryName,
			eventName,
			message,
			exception,
			properties,
			GetTraceId(),
			GetSpanId(),
			GetTraceFlags(),
			GetTraceState());

		logger.Log(entry);
	}

	/// <summary>
	/// Writes a log entry using deferred structured property creation when the specified level is enabled.
	/// </summary>
	/// <typeparam name="TState">The state used to build structured properties.</typeparam>
	/// <param name="logger">The logger to write to.</param>
	/// <param name="level">The severity level for the entry.</param>
	/// <param name="eventName">The stable event name for the entry.</param>
	/// <param name="message">The rendered log message.</param>
	/// <param name="exception">An optional exception associated with the entry.</param>
	/// <param name="state">The state passed to <paramref name="propertiesFactory"/>.</param>
	/// <param name="propertiesFactory">Creates structured properties when logging is enabled.</param>
	public static void Log<TState>(
		this ILogger logger,
		LogLevel level,
		string eventName,
		string message,
		Exception? exception,
		TState state,
		Func<TState, IReadOnlyDictionary<string, object?>?> propertiesFactory)
	{
		Guard.AgainstNull(logger);
		Guard.AgainstNull(propertiesFactory);

		if (!logger.IsEnabled(level))
			return;

		logger.Log(level, eventName, message, exception, propertiesFactory(state));
	}

	/// <summary>
	/// Writes an informational log entry.
	/// </summary>
	/// <param name="logger">The logger to write to.</param>
	/// <param name="eventName">The stable event name for the entry.</param>
	/// <param name="message">The rendered log message.</param>
	/// <param name="properties">Optional structured properties to attach to the entry.</param>
	public static void LogInformation(this ILogger logger, string eventName, string message, IReadOnlyDictionary<string, object?>? properties = null) =>
		logger.Log(LogLevel.Information, eventName, message, null, properties);

	/// <summary>
	/// Writes an informational log entry using deferred structured property creation.
	/// </summary>
	/// <typeparam name="TState">The state used to build structured properties.</typeparam>
	/// <param name="logger">The logger to write to.</param>
	/// <param name="eventName">The stable event name for the entry.</param>
	/// <param name="message">The rendered log message.</param>
	/// <param name="state">The state passed to <paramref name="propertiesFactory"/>.</param>
	/// <param name="propertiesFactory">Creates structured properties when logging is enabled.</param>
	public static void LogInformation<TState>(this ILogger logger, string eventName, string message, TState state, Func<TState, IReadOnlyDictionary<string, object?>?> propertiesFactory) =>
		logger.Log(LogLevel.Information, eventName, message, null, state, propertiesFactory);

	/// <summary>
	/// Writes a debug log entry.
	/// </summary>
	/// <param name="logger">The logger to write to.</param>
	/// <param name="eventName">The stable event name for the entry.</param>
	/// <param name="message">The rendered log message.</param>
	/// <param name="properties">Optional structured properties to attach to the entry.</param>
	public static void LogDebug(this ILogger logger, string eventName, string message, IReadOnlyDictionary<string, object?>? properties = null) =>
		logger.Log(LogLevel.Debug, eventName, message, null, properties);

	/// <summary>
	/// Writes a debug log entry using deferred structured property creation.
	/// </summary>
	/// <typeparam name="TState">The state used to build structured properties.</typeparam>
	/// <param name="logger">The logger to write to.</param>
	/// <param name="eventName">The stable event name for the entry.</param>
	/// <param name="message">The rendered log message.</param>
	/// <param name="state">The state passed to <paramref name="propertiesFactory"/>.</param>
	/// <param name="propertiesFactory">Creates structured properties when logging is enabled.</param>
	public static void LogDebug<TState>(this ILogger logger, string eventName, string message, TState state, Func<TState, IReadOnlyDictionary<string, object?>?> propertiesFactory) =>
		logger.Log(LogLevel.Debug, eventName, message, null, state, propertiesFactory);

	/// <summary>
	/// Writes a warning log entry.
	/// </summary>
	/// <param name="logger">The logger to write to.</param>
	/// <param name="eventName">The stable event name for the entry.</param>
	/// <param name="message">The rendered log message.</param>
	/// <param name="exception">An optional exception associated with the entry.</param>
	/// <param name="properties">Optional structured properties to attach to the entry.</param>
	public static void LogWarning(this ILogger logger, string eventName, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null) =>
		logger.Log(LogLevel.Warning, eventName, message, exception, properties);

	/// <summary>
	/// Writes a warning log entry using deferred structured property creation.
	/// </summary>
	/// <typeparam name="TState">The state used to build structured properties.</typeparam>
	/// <param name="logger">The logger to write to.</param>
	/// <param name="eventName">The stable event name for the entry.</param>
	/// <param name="message">The rendered log message.</param>
	/// <param name="state">The state passed to <paramref name="propertiesFactory"/>.</param>
	/// <param name="propertiesFactory">Creates structured properties when logging is enabled.</param>
	public static void LogWarning<TState>(this ILogger logger, string eventName, string message, TState state, Func<TState, IReadOnlyDictionary<string, object?>?> propertiesFactory) =>
		logger.Log(LogLevel.Warning, eventName, message, null, state, propertiesFactory);

	/// <summary>
	/// Writes a warning log entry with an exception using deferred structured property creation.
	/// </summary>
	/// <typeparam name="TState">The state used to build structured properties.</typeparam>
	/// <param name="logger">The logger to write to.</param>
	/// <param name="eventName">The stable event name for the entry.</param>
	/// <param name="message">The rendered log message.</param>
	/// <param name="exception">An optional exception associated with the entry.</param>
	/// <param name="state">The state passed to <paramref name="propertiesFactory"/>.</param>
	/// <param name="propertiesFactory">Creates structured properties when logging is enabled.</param>
	public static void LogWarning<TState>(this ILogger logger, string eventName, string message, Exception? exception, TState state, Func<TState, IReadOnlyDictionary<string, object?>?> propertiesFactory) =>
		logger.Log(LogLevel.Warning, eventName, message, exception, state, propertiesFactory);

	/// <summary>
	/// Writes an error log entry.
	/// </summary>
	/// <param name="logger">The logger to write to.</param>
	/// <param name="eventName">The stable event name for the entry.</param>
	/// <param name="message">The rendered log message.</param>
	/// <param name="exception">An optional exception associated with the entry.</param>
	/// <param name="properties">Optional structured properties to attach to the entry.</param>
	public static void LogError(this ILogger logger, string eventName, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null) =>
		logger.Log(LogLevel.Error, eventName, message, exception, properties);

	/// <summary>
	/// Writes an error log entry using deferred structured property creation.
	/// </summary>
	/// <typeparam name="TState">The state used to build structured properties.</typeparam>
	/// <param name="logger">The logger to write to.</param>
	/// <param name="eventName">The stable event name for the entry.</param>
	/// <param name="message">The rendered log message.</param>
	/// <param name="exception">An optional exception associated with the entry.</param>
	/// <param name="state">The state passed to <paramref name="propertiesFactory"/>.</param>
	/// <param name="propertiesFactory">Creates structured properties when logging is enabled.</param>
	public static void LogError<TState>(this ILogger logger, string eventName, string message, Exception? exception, TState state, Func<TState, IReadOnlyDictionary<string, object?>?> propertiesFactory) =>
		logger.Log(LogLevel.Error, eventName, message, exception, state, propertiesFactory);

#if FEATURE_ACTIVITY
	private static string? GetTraceId() => System.Diagnostics.Activity.Current?.TraceId.ToString();

	private static string? GetSpanId() => System.Diagnostics.Activity.Current?.SpanId.ToString();

	private static byte? GetTraceFlags() => System.Diagnostics.Activity.Current is { } activity ? (byte)activity.ActivityTraceFlags : null;

	private static string? GetTraceState() => System.Diagnostics.Activity.Current?.TraceStateString;
#else
	private static string? GetTraceId() => null;

	private static string? GetSpanId() => null;

	private static byte? GetTraceFlags() => null;

	private static string? GetTraceState() => null;
#endif
}
