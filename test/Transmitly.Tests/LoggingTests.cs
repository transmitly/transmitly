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

using System.Collections.Concurrent;
using System.Diagnostics;
using Transmitly.ChannelProvider;

namespace Transmitly.Tests;

[TestClass]
public class LoggingTests
{
	[TestMethod]
	public void AddLoggingShouldEnableNonNullLoggerFactory()
	{
		var capture = new CaptureClientMiddleware();

		new CommunicationsClientBuilder()
			.AddLogging()
			.AddClientMiddleware(capture)
			.BuildClient();

		Assert.IsNotNull(capture.LoggerFactory);
		Assert.IsTrue(capture.LoggerFactory!.CreateLogger("test").IsEnabled(LogLevel.Information));
	}

	[TestMethod]
	public void WithoutAddLoggingShouldKeepNullLoggerFactory()
	{
		var capture = new CaptureClientMiddleware();

		new CommunicationsClientBuilder()
			.AddClientMiddleware(capture)
			.BuildClient();

		Assert.IsNotNull(capture.LoggerFactory);
		Assert.IsFalse(capture.LoggerFactory!.CreateLogger("test").IsEnabled(LogLevel.Information));
	}

	[TestMethod]
	public async Task DispatchMiddlewareAndCoreShouldEmitLogsThroughConfiguredFactory()
	{
		var factory = new RecordingLoggerFactory();
		var middleware = new LoggingDispatchMiddleware();

		var client = new CommunicationsClientBuilder()
			.AddLogging(options =>
			{
				options.LoggerFactory = factory;
				options.MinimumLevel = LogLevel.Debug;
			})
			.AddDispatchMiddleware(middleware)
			.ChannelProvider.Add<Mocks.SuccessChannelProviderDispatcher, object>("test")
			.AddPipeline("test", options =>
			{
				options.AddChannel(new UnitTestChannel("unit-test-address").HandlesAddressStartsWith("unit-test"));
			})
			.BuildClient();

		await client.DispatchAsync("test", "unit-test-address-to", new { });

		Assert.IsTrue(factory.Entries.Any(e => e.EventName == "test.middleware"));
		Assert.IsTrue(factory.Entries.Any(e => e.EventName == LogEvents.DispatchStarted));
		Assert.IsTrue(factory.Entries.Any(e => e.EventName == LogEvents.ProviderDispatchCompleted));
	}

	[TestMethod]
	public async Task NonDiDispatcherShouldReceiveLoggerFactoryViaConstructor()
	{
		LoggerAwareDispatcher.CapturedLoggerFactory = null;

		var client = new CommunicationsClientBuilder()
			.AddLogging(options =>
			{
				options.LoggerFactory = new RecordingLoggerFactory();
				options.MinimumLevel = LogLevel.Debug;
			})
			.ChannelProvider.Add<LoggerAwareDispatcher, object>("logger-aware")
			.AddPipeline("test", options =>
			{
				options.AddChannel(new UnitTestChannel("unit-test-address").HandlesAddressStartsWith("unit-test"));
			})
			.BuildClient();

		await client.DispatchAsync("test", "unit-test-address-to", new { });

		Assert.IsNotNull(LoggerAwareDispatcher.CapturedLoggerFactory);
		Assert.IsTrue(LoggerAwareDispatcher.CapturedLoggerFactory!.CreateLogger("test").IsEnabled(LogLevel.Debug));
	}

	[TestMethod]
	public void DeferredPropertiesFactoryShouldNotRunWhenLevelDisabled()
	{
		var factory = new MinimumLevelLoggerFactory(new RecordingLoggerFactory(), LogLevel.Warning);
		var logger = factory.CreateLogger<LoggingTests>();
		var factoryInvoked = false;

		logger.LogDebug(
			"test.debug",
			"Debug should be suppressed.",
			42,
			_ =>
			{
				factoryInvoked = true;
				return new Dictionary<string, object?> { ["value"] = 42 };
			});

		Assert.IsFalse(factoryInvoked);
	}

	[TestMethod]
	public void DeferredPropertiesFactoryShouldRunWhenLevelEnabled()
	{
		var factory = new RecordingLoggerFactory();
		var logger = factory.CreateLogger<LoggingTests>();
		var factoryInvocationCount = 0;

		logger.LogInformation(
			"test.info",
			"Properties should be created.",
			42,
			value =>
			{
				factoryInvocationCount++;
				return new Dictionary<string, object?> { ["value"] = value };
			});

		Assert.AreEqual(1, factoryInvocationCount);
		Assert.AreEqual(1, factory.Entries.Count);
		Assert.AreEqual(42, factory.Entries[0].Properties["value"]);
	}

	[TestMethod]
	public void MinimumLevelLoggerFactoryShouldRespectExactThreshold()
	{
		var factory = new MinimumLevelLoggerFactory(new RecordingLoggerFactory(), LogLevel.Warning);
		var logger = factory.CreateLogger<LoggingTests>();

		Assert.IsFalse(logger.IsEnabled(LogLevel.Information));
		Assert.IsTrue(logger.IsEnabled(LogLevel.Warning));
		Assert.IsTrue(logger.IsEnabled(LogLevel.Error));
		Assert.IsFalse(logger.IsEnabled(LogLevel.None));
	}

	[TestMethod]
	public void MinimumLevelLoggerFactoryShouldRespectInnerLoggerEnablement()
	{
		var factory = new MinimumLevelLoggerFactory(new SelectiveLoggerFactory(LogLevel.Error), LogLevel.Debug);
		var logger = factory.CreateLogger("test");

		Assert.IsFalse(logger.IsEnabled(LogLevel.Warning));
		Assert.IsTrue(logger.IsEnabled(LogLevel.Error));
	}

	[TestMethod]
	public void NullLoggerFactoryShouldReturnSharedDisabledLogger()
	{
		var factory = NullLoggerFactory.Instance;
		var first = factory.CreateLogger("first");
		var second = factory.CreateLogger("second");

		Assert.AreSame(first, second);
		Assert.IsFalse(first.IsEnabled(LogLevel.Trace));
		Assert.AreEqual("Transmitly.NullLogger", first.CategoryName);
	}

	[TestMethod]
	public void LogShouldCreateEntryWithEmptyPropertiesWhenNoneProvided()
	{
		var factory = new RecordingLoggerFactory();
		var logger = factory.CreateLogger<LoggingTests>();

		logger.LogInformation("test.empty", "No properties.");

		var entry = factory.Entries.Single();
		Assert.IsNotNull(entry.Properties);
		Assert.AreEqual(0, entry.Properties.Count);
	}

	[TestMethod]
	public void LogShouldCaptureProvidedExceptionAndProperties()
	{
		var factory = new RecordingLoggerFactory();
		var logger = factory.CreateLogger<LoggingTests>();
		var exception = new InvalidOperationException("boom");

		logger.LogError(
			"test.error",
			"Failure occurred.",
			exception,
			new Dictionary<string, object?> { ["attempt"] = 3 });

		var entry = factory.Entries.Single();
		Assert.AreSame(exception, entry.Exception);
		Assert.AreEqual(3, entry.Properties["attempt"]);
		Assert.AreEqual("test.error", entry.EventName);
	}

	[TestMethod]
	public async Task LoggingShouldRemainCorrectUnderConcurrentWrites()
	{
		const int operationCount = 256;
		var factory = new ConcurrentRecordingLoggerFactory();

		await Task.WhenAll(
			Enumerable.Range(0, operationCount)
				.Select(index => Task.Run(() =>
				{
					factory.CreateLogger<LoggingTests>()
						.LogInformation(
							"test.concurrent",
							"Concurrent entry.",
							index,
							static value => new Dictionary<string, object?> { ["index"] = value });
				})));

		Assert.AreEqual(operationCount, factory.Entries.Count);
		CollectionAssert.AreEquivalent(
			Enumerable.Range(0, operationCount).Cast<object>().ToList(),
			factory.Entries.Select(e => e.Properties["index"]!).ToList());
	}

#if FEATURE_ACTIVITY
	[TestMethod]
	public void LogShouldCaptureCurrentActivityCorrelation()
	{
		var factory = new RecordingLoggerFactory();
		var logger = factory.CreateLogger<LoggingTests>();

		using var activity = new Activity("transmitly.logging.test");
		activity.SetIdFormat(ActivityIdFormat.W3C);
		activity.TraceStateString = "vendor=value";
		activity.Start();

		logger.LogInformation("test.activity", "Captured activity.");

		var entry = factory.Entries.Single();
		Assert.AreEqual(activity.TraceId.ToString(), entry.TraceId);
		Assert.AreEqual(activity.SpanId.ToString(), entry.SpanId);
		Assert.AreEqual((byte)activity.ActivityTraceFlags, entry.TraceFlags);
		Assert.AreEqual(activity.TraceStateString, entry.TraceState);
	}
#endif

	private sealed class CaptureClientMiddleware : ICommunicationClientMiddleware
	{
		public ILoggerFactory? LoggerFactory { get; private set; }

		public ICommunicationsClient? CreateClient(ICreateCommunicationsClientContext context, ICommunicationsClient? previous)
		{
			LoggerFactory = context.LoggerFactory;
			return previous;
		}
	}

	private sealed class LoggingDispatchMiddleware : IDispatchMiddleware
	{
		public Task<IReadOnlyCollection<IDispatchResult?>> InvokeAsync(DispatchMiddlewareContext ctx, Func<DispatchMiddlewareContext, Task<IReadOnlyCollection<IDispatchResult?>>> next)
		{
			ctx.LoggerFactory.CreateLogger<LoggingDispatchMiddleware>()
				.LogInformation("test.middleware", "Dispatch middleware invoked.");
			return next(ctx);
		}
	}

	private sealed class LoggerAwareDispatcher(ILoggerFactory loggerFactory) : IChannelProviderDispatcher<object>
	{
		public static ILoggerFactory? CapturedLoggerFactory { get; set; }

		public Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(object communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
		{
			CapturedLoggerFactory = loggerFactory;
			return Task.FromResult<IReadOnlyCollection<IDispatchResult?>>([new DispatchResult(CommunicationsStatus.Success("logger-aware"))]);
		}
	}

	private sealed class RecordingLoggerFactory : ILoggerFactory
	{
		private readonly List<LogEntry> _entries = [];

		public IReadOnlyList<LogEntry> Entries => _entries;

		public ILogger CreateLogger(string categoryName)
		{
			return new RecordingLogger(categoryName, _entries);
		}
	}

	private sealed class RecordingLogger(string categoryName, List<LogEntry> entries) : ILogger
	{
		public string CategoryName { get; } = categoryName;

		public bool IsEnabled(LogLevel level) => level != LogLevel.None;

		public void Log(LogEntry entry)
		{
			entries.Add(entry);
		}
	}

	private sealed class ConcurrentRecordingLoggerFactory : ILoggerFactory
	{
		private readonly ConcurrentQueue<LogEntry> _entries = new();

		public IReadOnlyCollection<LogEntry> Entries => _entries.ToArray();

		public ILogger CreateLogger(string categoryName)
		{
			return new ConcurrentRecordingLogger(categoryName, _entries);
		}
	}

	private sealed class ConcurrentRecordingLogger(string categoryName, ConcurrentQueue<LogEntry> entries) : ILogger
	{
		public string CategoryName { get; } = categoryName;

		public bool IsEnabled(LogLevel level) => level != LogLevel.None;

		public void Log(LogEntry entry)
		{
			entries.Enqueue(entry);
		}
	}

	private sealed class SelectiveLoggerFactory(LogLevel enabledLevel) : ILoggerFactory
	{
		public ILogger CreateLogger(string categoryName)
		{
			return new SelectiveLogger(categoryName, enabledLevel);
		}
	}

	private sealed class SelectiveLogger(string categoryName, LogLevel enabledLevel) : ILogger
	{
		public string CategoryName { get; } = categoryName;

		public bool IsEnabled(LogLevel level) => level == enabledLevel;

		public void Log(LogEntry entry)
		{
		}
	}
}
