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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
