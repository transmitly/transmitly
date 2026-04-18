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
using Transmitly.Delivery;

namespace Transmitly.Tests;

[TestClass]
public class SimulationSupportTests
{
	[TestMethod]
	public async Task AddSimulationSupportShouldReturnDefaultSuccessfulDispatchResult()
	{
		var reports = new ConcurrentQueue<DeliveryReport>();
		var client = CreateClient(builder => builder.AddSimulationSupport(), reports);

		var result = await client.DispatchAsync("test", "unit-test-address-to", new { }).ConfigureAwait(false);

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(1, result.Results.Count);

		var dispatchResult = result.Results.Single();
		Assert.IsNotNull(dispatchResult);
		Assert.AreEqual(Id.ChannelProvider.Simulation(), dispatchResult!.ChannelProviderId);
		Assert.AreEqual("unit-test-channel", dispatchResult.ChannelId);
		Assert.AreEqual("Dispatched", dispatchResult.Status.Type);

		await WaitForReportCountAsync(reports, expectedCount: 1).ConfigureAwait(false);
		Assert.AreEqual(1, reports.Count);

		var report = reports.Single();
		Assert.AreEqual(DeliveryReport.Event.Dispatched(), report.EventName);
		Assert.AreEqual(dispatchResult.ChannelProviderId, report.ChannelProviderId);
		Assert.AreEqual(dispatchResult.ChannelId, report.ChannelId);
		Assert.AreEqual(dispatchResult.ResourceId, report.ResourceId);
	}

	[TestMethod]
	public async Task AddSimulationSupportShouldAllowSimulationToBeDisabled()
	{
		var reports = new ConcurrentQueue<DeliveryReport>();
		var client = CreateClient(
			builder => builder.AddSimulationSupport(options => options.SimulateDispatchResult = false),
			reports);

		var result = await client.DispatchAsync("test", "unit-test-address-to", new { }).ConfigureAwait(false);

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(0, result.Results.Count);
		Assert.AreEqual(0, reports.Count);
	}

	[TestMethod]
	public async Task AddSimulationSupportShouldUseConfiguredSimulationHandler()
	{
		var reports = new ConcurrentQueue<DeliveryReport>();
		var handlerInvoked = false;
		object? capturedCommunication = null;
		string? capturedProviderId = null;
		string? capturedPipelineIntent = null;
		const string customProviderId = "custom";
		const string customResourceId = "resource-123";

		var client = CreateClient(
			builder => builder.AddSimulationSupport(options =>
			{
				options.SimulateDispatchResultHandler = (communication, context) =>
				{
					handlerInvoked = true;
					capturedCommunication = communication;
					capturedProviderId = context.ChannelProviderId;
					capturedPipelineIntent = context.PipelineIntent;

					IReadOnlyCollection<IDispatchResult?> results =
					[
						new DispatchResult(CommunicationsStatus.Success("Tests", "Simulated"), customResourceId),
						null
					];

					return Task.FromResult(results);
				};
			}, customProviderId),
			reports);

		var result = await client.DispatchAsync("test", "unit-test-address-to", new { }).ConfigureAwait(false);

		Assert.IsTrue(handlerInvoked);
		Assert.IsInstanceOfType<UnitTestCommunication>(capturedCommunication);
		Assert.AreEqual(Id.ChannelProvider.Simulation(customProviderId), capturedProviderId);
		Assert.AreEqual("test", capturedPipelineIntent);
		Assert.AreEqual(1, result.Results.Count);

		var dispatchResult = result.Results.Single();
		Assert.IsNotNull(dispatchResult);
		Assert.AreEqual(Id.ChannelProvider.Simulation(customProviderId), dispatchResult!.ChannelProviderId);
		Assert.AreEqual(customResourceId, dispatchResult.ResourceId);

		await WaitForReportCountAsync(reports, expectedCount: 1).ConfigureAwait(false);
		Assert.AreEqual(1, reports.Count);

		var report = reports.Single();
		Assert.AreEqual(DeliveryReport.Event.Dispatched(), report.EventName);
		Assert.AreEqual(Id.ChannelProvider.Simulation(customProviderId), report.ChannelProviderId);
		Assert.AreEqual("unit-test-channel", report.ChannelId);
		Assert.AreEqual(customResourceId, report.ResourceId);
	}

	[TestMethod]
	public async Task AddSimulationSupportShouldEmitDispatchedReportForEachNonNullCustomResult()
	{
		var reports = new ConcurrentQueue<DeliveryReport>();
		var client = CreateClient(
			builder => builder.AddSimulationSupport(options =>
			{
				options.SimulateDispatchResultHandler = (_, _) =>
				{
					IReadOnlyCollection<IDispatchResult?> results =
					[
						new DispatchResult(CommunicationsStatus.Success("Tests", "Simulated"), "resource-1"),
						new DispatchResult(CommunicationsStatus.Success("Tests", "Simulated"), "resource-2"),
						null
					];

					return Task.FromResult(results);
				};
			}),
			reports);

		var result = await client.DispatchAsync("test", "unit-test-address-to", new { }).ConfigureAwait(false);

		Assert.AreEqual(2, result.Results.Count);

		await WaitForReportCountAsync(reports, expectedCount: 2).ConfigureAwait(false);
		Assert.AreEqual(2, reports.Count);
		CollectionAssert.AreEquivalent(
			new[] { "resource-1", "resource-2" },
			reports.Select(r => r.ResourceId).ToArray());
	}

	[TestMethod]
	public async Task AddSimulationSupportShouldAllowCustomHandlerToReturnEmptyResults()
	{
		var reports = new ConcurrentQueue<DeliveryReport>();
		var client = CreateClient(
			builder => builder.AddSimulationSupport(options =>
			{
				options.SimulateDispatchResultHandler = (_, _) =>
					Task.FromResult<IReadOnlyCollection<IDispatchResult?>>([]);
			}),
			reports);

		var result = await client.DispatchAsync("test", "unit-test-address-to", new { }).ConfigureAwait(false);

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(0, result.Results.Count);
		Assert.AreEqual(0, reports.Count);
	}

	[TestMethod]
	public async Task AddSimulationSupportShouldTreatNullHandlerResultsAsEmpty()
	{
		var reports = new ConcurrentQueue<DeliveryReport>();
		var client = CreateClient(
			builder => builder.AddSimulationSupport(options =>
			{
				options.SimulateDispatchResultHandler = (_, _) =>
					Task.FromResult<IReadOnlyCollection<IDispatchResult?>?>(null)!;
			}),
			reports);

		var result = await client.DispatchAsync("test", "unit-test-address-to", new { }).ConfigureAwait(false);

		Assert.IsTrue(result.IsSuccessful);
		Assert.AreEqual(0, result.Results.Count);
		Assert.AreEqual(0, reports.Count);
	}

	[TestMethod]
	public async Task AddSimulationSupportShouldPreserveHandlerExceptionInDispatchedReport()
	{
		var reports = new ConcurrentQueue<DeliveryReport>();
		var expectedException = new InvalidOperationException("simulated failure");
		var client = CreateClient(
			builder => builder.AddSimulationSupport(options =>
			{
				options.SimulateDispatchResultHandler = (_, _) =>
				{
					IReadOnlyCollection<IDispatchResult?> results =
					[
						new TestDispatchResult(
							CommunicationsStatus.ClientError("Tests", "SimulatedFailure"),
							"resource-error",
							expectedException)
					];

					return Task.FromResult(results);
				};
			}),
			reports);

		var result = await client.DispatchAsync("test", "unit-test-address-to", new { }).ConfigureAwait(false);

		Assert.AreEqual(1, result.Results.Count);
		await WaitForReportCountAsync(reports, expectedCount: 1).ConfigureAwait(false);

		var report = reports.Single();
		Assert.AreEqual("resource-error", report.ResourceId);
		Assert.AreSame(expectedException, report.Exception);
		Assert.AreEqual("SimulatedFailure", report.Status.Type);
	}

	[TestMethod]
	public async Task AddSimulationSupportShouldFollowExistingClientCancellationBehavior()
	{
		var reports = new ConcurrentQueue<DeliveryReport>();
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		var client = CreateClient(builder => builder.AddSimulationSupport(), reports);
		var result = await client.DispatchAsync("test", "unit-test-address-to", new { }, cancellationToken: cts.Token).ConfigureAwait(false);

		Assert.AreEqual(2, result.Results.Count);
		Assert.IsTrue(result.Results.Any(r => r?.Status.Type == "Dispatched"));
		Assert.IsTrue(result.Results.Any(r => r?.Status.Type == "The token has had cancellation requested."));

		await WaitForReportCountAsync(reports, expectedCount: 1).ConfigureAwait(false);
		Assert.AreEqual(1, reports.Count);
	}

	[TestMethod]
	public void SimulationProviderIdShouldIncludeOptionalSuffix()
	{
		Assert.AreEqual("Simulation", Id.ChannelProvider.Simulation());
		Assert.AreEqual("Simulation.custom", Id.ChannelProvider.Simulation("custom"));
	}

	private static ICommunicationsClient CreateClient(Action<CommunicationsClientBuilder> configure, ConcurrentQueue<DeliveryReport> reports)
	{
		var builder = new CommunicationsClientBuilder();
		configure(builder);
		builder
			.AddDeliveryReportHandler(report =>
			{
				reports.Enqueue(report);
				return Task.CompletedTask;
			})
			.AddPipeline("test", options =>
			{
				options.AddChannel(new UnitTestChannel("unit-test-address").HandlesAddressStartsWith("unit-test"));
			});

		return builder.BuildClient();
	}

	private static async Task WaitForReportCountAsync(ConcurrentQueue<DeliveryReport> reports, int expectedCount)
	{
		var timeout = DateTime.UtcNow.AddSeconds(2);
		while (reports.Count < expectedCount && DateTime.UtcNow < timeout)
		{
			await Task.Delay(20).ConfigureAwait(false);
		}
	}

	private sealed class TestDispatchResult(
		CommunicationsStatus status,
		string resourceId,
		Exception? exception = null) : IDispatchResult
	{
		public string? ResourceId { get; } = resourceId;
		public CommunicationsStatus Status { get; } = status;
		public string? ChannelProviderId => null;
		public string? ChannelId => null;
		public string? PipelineId => null;
		public string? PipelineIntent => null;
		public Exception? Exception { get; } = exception;
	}
}
