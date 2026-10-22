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

using Moq;
using Transmitly.Delivery;
using Transmitly.Tests.Mocks;

namespace Transmitly.Tests.Delivery
{

	[TestClass]
	public class BaseDeliveryReportServiceTests
	{
		private static DeliveryReport CreateReport()
		{
			return new DeliveryReport(
				"TestEvent",
				"ChannelId",
				"ProviderId",
				"Pipeline",
				"PipelineId",
				"ResourceId",
				CommunicationsStatus.Info("Test", "Info"),
				null,
				null,
				null
			);
		}

		[TestMethod]
		public void SubscribeSingleObserverObserverReceivesReport()
		{
			var observer = new Mock<IObserver<DeliveryReport>>();
			var service = new MockDeliveryReportService(Array.Empty<IObserver<DeliveryReport>>());

			service.Subscribe(observer.Object);
			var report = CreateReport();

			service.DispatchAsync(report).Wait();

			observer.Verify(o => o.OnNext(report), Times.Once);
		}

		[TestMethod]
		public void SubscribeMultipleObserversAllReceiveReport()
		{
			var observer1 = new Mock<IObserver<DeliveryReport>>();
			var observer2 = new Mock<IObserver<DeliveryReport>>();
			var observers = new[] { observer1.Object, observer2.Object };
			var service = new MockDeliveryReportService(observers);

			var report = CreateReport();
			service.DispatchAsync(report).Wait();

			observer1.Verify(o => o.OnNext(report), Times.Once);
			observer2.Verify(o => o.OnNext(report), Times.Once);
		}

		[TestMethod]
		public void SubscribeReturnsDisposableUnsubscribesObserver()
		{
			var observer = new Mock<IObserver<DeliveryReport>>();
			var service = new MockDeliveryReportService(Array.Empty<IObserver<DeliveryReport>>());

			var subscription = service.Subscribe(observer.Object);
			subscription.Dispose();

			var report = CreateReport();
			service.DispatchAsync(report).Wait();

			observer.Verify(o => o.OnNext(It.IsAny<DeliveryReport>()), Times.Never);
		}

		[TestMethod]
		public void SubscribeMultipleObserversReturnsDisposablesAllUnsubscribed()
		{
			var observer1 = new Mock<IObserver<DeliveryReport>>();
			var observer2 = new Mock<IObserver<DeliveryReport>>();
			var observers = new[] { observer1.Object, observer2.Object };
			var service = new MockDeliveryReportService(Array.Empty<IObserver<DeliveryReport>>());

			var disposables = service.Subscribe(observers);
			foreach (var d in disposables)
				d.Dispose();

			var report = CreateReport();
			service.DispatchAsync(report).Wait();

			observer1.Verify(o => o.OnNext(It.IsAny<DeliveryReport>()), Times.Never);
			observer2.Verify(o => o.OnNext(It.IsAny<DeliveryReport>()), Times.Never);
		}

		[TestMethod]
		public void DispatchAsyncShouldNotWaitForHandlers()
		{
			var release = new TaskCompletionSource<bool>();
			var monitor = new DeliveryReportMonitor(_ => release.Task);
			var service = new MockDeliveryReportService([monitor]);

			var dispatch = service.DispatchAsync(CreateReport());

			Assert.IsTrue(dispatch.IsCompleted);
			release.SetResult(true);
		}

		[TestMethod]
		public async Task FailingHandlerShouldBeLoggedWithoutStoppingOtherHandlers()
		{
			var failure = new InvalidOperationException("handler failed");
			var logged = new TaskCompletionSource<LogEntry>();
			var otherHandlerRan = new TaskCompletionSource<bool>();
			var logger = new Mock<ILogger>();
			logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
			logger.SetupGet(l => l.CategoryName).Returns("test");
			logger.Setup(l => l.Log(It.IsAny<LogEntry>())).Callback<LogEntry>(e => logged.TrySetResult(e));
			var loggerFactory = new Mock<ILoggerFactory>();
			loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
			var service = new DefaultDeliveryReportService(
			[
				new DeliveryReportMonitor(async _ =>
				{
					await Task.Yield();
					throw failure;
				}),
				new DeliveryReportMonitor(_ =>
				{
					otherHandlerRan.TrySetResult(true);
					return Task.CompletedTask;
				})
			], loggerFactory.Object);

			await service.DispatchAsync(CreateReport());

			var timeout = Task.Delay(TimeSpan.FromSeconds(5));
			Assert.AreSame(logged.Task, await Task.WhenAny(logged.Task, timeout), "The handler failure was never logged");
			Assert.AreSame(failure, logged.Task.Result.Exception);
			Assert.AreEqual(LogLevel.Error, logged.Task.Result.Level);
			Assert.AreSame(otherHandlerRan.Task, await Task.WhenAny(otherHandlerRan.Task, timeout), "A failing handler stopped the remaining handlers.");
		}

		[TestMethod]
		public void DispatchAsyncMultipleReportsAllObserversReceiveAll()
		{
			var observer = new Mock<IObserver<DeliveryReport>>();
			var service = new MockDeliveryReportService(new[] { observer.Object });

			var reports = new[] { CreateReport(), CreateReport() };
			service.DispatchAsync(reports).Wait();

			observer.Verify(o => o.OnNext(It.IsAny<DeliveryReport>()), Times.Exactly(reports.Length));
		}
	}
}
