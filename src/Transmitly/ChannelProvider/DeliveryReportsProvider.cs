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
	internal sealed class DeliveryReportsProvider : IDeliveryReportProvider
	{
		private readonly HashSet<IObserver<DeliveryReport>> _observers = [];

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

		public void DeliveryReport(DeliveryReport deliveryReport)
		{
			foreach (IObserver<DeliveryReport> observer in _observers)
			{
				observer.OnNext(deliveryReport);
			}
		}
	}
}