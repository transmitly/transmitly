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
using Transmitly.Pipeline.Configuration;
using Transmitly.PlatformIdentity.Configuration;

namespace Transmitly.Tests;

[TestClass]
public class DefaultCommunicationsDeliveryClientTests
{
	private Mock<IDeliveryReportService>? _deliveryReportServiceMock;
	private DefaultCommunicationsClient? _client;

	[TestInitialize]
	public void Setup()
	{
		_deliveryReportServiceMock = new Mock<IDeliveryReportService>();
		var pipelineService = new Mock<IPipelineService>().Object;
		var dispatchCoordinatorService = new Mock<IDispatchCoordinatorService>().Object;
		var platformIdentityService = new Mock<IPlatformIdentityService>().Object;

		_client = new DefaultCommunicationsClient(
			pipelineService,
			dispatchCoordinatorService,
			platformIdentityService,
			_deliveryReportServiceMock.Object
		);
	}

	[TestMethod]
	public async Task DispatchAsyncSingleReportCallsDeliveryReportService()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		var report = new DeliveryReport("Event", null, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
		_deliveryReportServiceMock!
			.Setup(s => s.DispatchAsync(report))
			.Returns(Task.CompletedTask)
			.Verifiable();

		await _client!.DispatchAsync(report);

		_deliveryReportServiceMock.Verify(s => s.DispatchAsync(report), Times.Once);
	}

	[TestMethod]
	public async Task DispatchAsyncMultipleReportsCallsDeliveryReportService()
	{
		var reports = new List<DeliveryReport>
			{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
				new("Event1", null, null, null, null, null, null, null, null, null),
				new("Event2", null, null, null, null, null, null, null, null, null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
			};
		_deliveryReportServiceMock!
			.Setup(s => s.DispatchAsync(reports))
			.Returns(Task.CompletedTask)
			.Verifiable();

		await _client!.DispatchAsync(reports);

		_deliveryReportServiceMock.Verify(s => s.DispatchAsync(reports), Times.Once);
	}
}