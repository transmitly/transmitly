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

namespace Transmitly.ChannelProvider
{
	internal class DeliveryReportMonitor : IObserver<DeliveryReport>
	{
		private readonly IReadOnlyCollection<string> _restrictedToEventNames;
		private readonly IReadOnlyCollection<string> _restrictedToChannelIds;
		private readonly IReadOnlyCollection<string> _restrictedToChannelProviderIds;
		private readonly DeliveryReportAsyncHandler? _handler;
		private readonly IObserver<DeliveryReport>? _observer;
		private IDisposable? _cancellation;
		public DeliveryReportMonitor(
			IObserver<DeliveryReport> reportObservable,
			IReadOnlyCollection<string>? restrictedToEventNames = null,
			IReadOnlyCollection<string>? restrictedToChannelIds = null,
			IReadOnlyCollection<string>? restrictedToChannelProviderIds = null)
				: this(restrictedToEventNames, restrictedToChannelIds, restrictedToChannelProviderIds
		)
		{
			_observer = Guard.AgainstNull(reportObservable);
		}

		public DeliveryReportMonitor(
			DeliveryReportAsyncHandler reportHandler,
			IReadOnlyCollection<string>? restrictedToEventNames = null,
			IReadOnlyCollection<string>? restrictedToChannelIds = null,
			IReadOnlyCollection<string>? restrictedToChannelProviderIds = null)
				: this(restrictedToEventNames, restrictedToChannelIds, restrictedToChannelProviderIds
		)
		{

			_handler = Guard.AgainstNull(reportHandler);
		}

		private DeliveryReportMonitor(
			IReadOnlyCollection<string>? restrictedToEventNames,
			IReadOnlyCollection<string>? restrictedToChannelIds,
			IReadOnlyCollection<string>? restrictedToChannelProviderIds
		)
		{
			_restrictedToEventNames = restrictedToEventNames ?? [];//Empty = Any
			_restrictedToChannelIds = restrictedToChannelIds ?? [];
			_restrictedToChannelProviderIds = restrictedToChannelProviderIds ?? [];
		}

		public virtual void Subscribe(IObservable<DeliveryReport> provider) =>
			_cancellation = provider.Subscribe(this);

		public virtual void Unsubscribe()
		{
			_cancellation?.Dispose();
		}

		public virtual void OnCompleted()
		{
			_observer?.OnCompleted();
		}

		public virtual void OnError(Exception error)
		{
			_observer?.OnError(error);
		}

		public virtual void OnNext(DeliveryReport value)
		{
			if (!ShouldFireEvent(value))
				return;

			if (_handler != null)
				_ = Task.Run(() => _handler(value));

			if (_observer != null)
				_ = Task.Run(() => _observer.OnNext(value));
		}

		private bool ShouldFireEvent(DeliveryReport value)
		{
			if (_restrictedToEventNames.Count != 0 && !_restrictedToEventNames.Any(e => e.Equals(value.EventName, StringComparison.OrdinalIgnoreCase)))
				return false;

			if (_restrictedToChannelIds.Count != 0 && value.ChannelId != null && !_restrictedToChannelIds.Any(c => c.Equals(value.ChannelId, StringComparison.OrdinalIgnoreCase)))
				return false;

			if (_restrictedToChannelProviderIds.Count != 0 && value.ChannelProviderId != null && !_restrictedToChannelProviderIds.Any(c => c.Equals(value.ChannelProviderId, StringComparison.OrdinalIgnoreCase)))
				return false;

			return true;
		}
	}
}