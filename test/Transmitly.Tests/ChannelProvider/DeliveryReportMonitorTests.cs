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

namespace Transmitly.ChannelProvider.Tests;

[TestClass()]
public class DeliveryReportMonitorTests
{
	[TestMethod()]
	public async Task ReportMonitorShouldFilterByEventName()
	{
		var handler = new Mock<IObserver<DeliveryReport>>();
		handler.Setup(s => s.OnNext(It.IsAny<DeliveryReport>())).Verifiable(Times.Exactly(1));
		var monitor = new DeliveryReportMonitor(handler.Object, ["test"]);
		monitor.OnNext(new DeliveryReport("test", null, null, null, null, null, PredefinedCommunicationStatuses.Unknown, null, null, null));
		monitor.OnNext(new DeliveryReport("test-2", null, null, null, null, null, PredefinedCommunicationStatuses.Unknown, null, null, null));
		await Task.Delay(10);//Delivery report calls are thrown out as tasks
		handler.Verify();
	}

	[TestMethod()]
	public async Task ReportMonitorShouldFilterByChannelId()
	{
		var handler = new Mock<IObserver<DeliveryReport>>();
		handler.Setup(s => s.OnNext(It.IsAny<DeliveryReport>())).Verifiable(Times.Exactly(1));
		var monitor = new DeliveryReportMonitor(handler.Object, null, ["channel"], null);
		monitor.OnNext(new DeliveryReport("test", "channel", null, null, null, null, PredefinedCommunicationStatuses.Unknown, null, null, null));
		monitor.OnNext(new DeliveryReport("test", "channel-2", null, null, null, null, PredefinedCommunicationStatuses.Unknown, null, null, null));
		await Task.Delay(10);//Delivery report calls are thrown out as tasks
		handler.Verify();
	}

	[TestMethod()]
	public async Task ReportMonitorShouldFilterByChannelProviderId()
	{
		var handler = new Mock<IObserver<DeliveryReport>>();
		handler.Setup(s => s.OnNext(It.IsAny<DeliveryReport>())).Verifiable(Times.Exactly(1));
		var monitor = new DeliveryReportMonitor(handler.Object, null, null, ["provider"]);
		monitor.OnNext(new DeliveryReport("test", "channel", "provider", null, null, null, PredefinedCommunicationStatuses.Unknown, null, null, null));
		monitor.OnNext(new DeliveryReport("test", "channel-2", "provider-2", null, null, null, PredefinedCommunicationStatuses.Unknown, null, null, null));
		await Task.Delay(10);//Delivery report calls are thrown out as tasks
		handler.Verify();
	}

	[TestMethod()]
	public async Task ReportMonitorShouldFilterByAll()
	{
		var handler = new Mock<IObserver<DeliveryReport>>();
		handler.Setup(s => s.OnNext(It.IsAny<DeliveryReport>())).Verifiable(Times.Exactly(1));
		var monitor = new DeliveryReportMonitor(handler.Object, ["test-2"], ["channel-2"], ["provider-2"]);
		monitor.OnNext(new DeliveryReport("test", "channel", "provider", null, null, null, PredefinedCommunicationStatuses.Unknown, null, null, null));
		monitor.OnNext(new DeliveryReport("test-2", "channel-2", "provider-2", null, null, null, PredefinedCommunicationStatuses.Unknown, null, null, null));
		await Task.Delay(10);//Delivery report calls are thrown out as tasks
		handler.Verify();
	}

	[TestMethod()]
	public async Task ReportMonitorShouldFireForAll()
	{
		var handler = new Mock<IObserver<DeliveryReport>>();
		handler.Setup(s => s.OnNext(It.IsAny<DeliveryReport>())).Verifiable(Times.Exactly(3));
		var monitor = new DeliveryReportMonitor(handler.Object);
		monitor.OnNext(new DeliveryReport("test", "channel", "provider", null, null, null, PredefinedCommunicationStatuses.Unknown, null, null, null));
		monitor.OnNext(new DeliveryReport("test-2", "channel-2", "provider-2", null, null, null, PredefinedCommunicationStatuses.Unknown, null, null, null));
		monitor.OnNext(new DeliveryReport("test-2", "channel-2", "provider-3", null, null, null, PredefinedCommunicationStatuses.Unknown, null, null, null));
		await Task.Delay(25);//Delivery report calls are thrown out as tasks
		handler.Verify();
	}
}