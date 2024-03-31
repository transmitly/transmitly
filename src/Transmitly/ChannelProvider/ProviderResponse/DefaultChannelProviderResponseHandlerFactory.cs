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

namespace Transmitly.ChannelProvider.ProviderResponse
{
	public sealed class DefaultChannelProviderResponseHandlerFactory : IChannelProviderResponseHandlerFactory
	{
		private readonly List<IChannelProviderStatusReportHandler> _responseHandlers = [];

		private static readonly Lazy<DefaultChannelProviderResponseHandlerFactory> lazy = new(() => new DefaultChannelProviderResponseHandlerFactory());
		// :(
		public IDeliveryReportProvider? DeliveryReportProvider { get; private set; }

		public IReadOnlyCollection<IChannelProviderStatusReportHandler> Handlers => _responseHandlers.AsReadOnly();

		public static DefaultChannelProviderResponseHandlerFactory Instance { get { return lazy.Value; } }

		private DefaultChannelProviderResponseHandlerFactory()
		{
		}

		internal void SetDeliveryReportProvider(IDeliveryReportProvider deliveryReportProvider)
		{
			DeliveryReportProvider = deliveryReportProvider;
		}

		public void AddHandler<THandler>(int index = 0)
			where THandler : IChannelProviderStatusReportHandler
		{
			var result = Activator.CreateInstance(typeof(THandler));
			_responseHandlers.Insert(index, ((IChannelProviderStatusReportHandler)Guard.AgainstNull(result)));
		}

		public void AddHandler(IChannelProviderStatusReportHandler handler, int index = 0)
		{
			_responseHandlers.Insert(index, Guard.AgainstNull(handler));
		}

	}
}
