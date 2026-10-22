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

using Transmitly.ChannelProvider;

namespace Transmitly.Delivery;

public abstract class BaseDeliveryReportService : IDeliveryReportService
{
	private readonly HashSet<IObserver<DeliveryReport>> _observers = [];
	private readonly ILogger _logger;

	protected BaseDeliveryReportService(IReadOnlyCollection<IObserver<DeliveryReport>> observers)
		: this(observers, null)
	{
	}

	protected BaseDeliveryReportService(IReadOnlyCollection<IObserver<DeliveryReport>> observers, ILoggerFactory? loggerFactory)
	{
		_logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<BaseDeliveryReportService>();
		Subscribe(observers);
	}

	public IDisposable Subscribe(IObserver<DeliveryReport> observer)
	{
		_observers.Add(observer);

		return new Unsubscriber<DeliveryReport>(_observers, observer);
	}

	public IReadOnlyCollection<IDisposable> Subscribe(IReadOnlyCollection<IObserver<DeliveryReport>> observers)
	{
		var cancels = new List<IDisposable>(observers.Count);
		foreach (var observer in observers)
		{
			cancels.Add(Subscribe(observer));
		}
		return cancels;
	}

	public Task DispatchAsync(DeliveryReport deliveryReport)
	{
		Guard.AgainstNull(deliveryReport);
		foreach (IObserver<DeliveryReport> observer in _observers)
		{
			if (observer is DeliveryReportMonitor monitor)
				// nothing awaits this task, so a failure that isn't logged here is lost
				_ = Task.Run(() => NotifyAsync(monitor, deliveryReport));
			else
				observer.OnNext(deliveryReport);
		}
		return Task.CompletedTask;
	}

	public Task DispatchAsync(IReadOnlyCollection<DeliveryReport> deliveryReports)
	{
		foreach (var report in Guard.AgainstNull(deliveryReports))
		{
			DispatchAsync(report);
		}
		return Task.CompletedTask;
	}

	private async Task NotifyAsync(DeliveryReportMonitor monitor, DeliveryReport deliveryReport)
	{
		try
		{
			await monitor.OnNextAsync(deliveryReport).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				LogEvents.DeliveryReportHandlerFailed,
				"Delivery report handler failed.",
				ex,
				new Dictionary<string, object?>
				{
					["eventName"] = deliveryReport.EventName,
					["channelId"] = deliveryReport.ChannelId,
					["channelProviderId"] = deliveryReport.ChannelProviderId,
					["resourceId"] = deliveryReport.ResourceId
				});
		}
	}
}
