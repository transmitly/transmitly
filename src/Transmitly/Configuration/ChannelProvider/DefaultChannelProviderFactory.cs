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
using Transmitly.ChannelProvider.Configuration;
using Transmitly.Delivery;
using Transmitly.Exceptions;

namespace Transmitly.Channel.Configuration
{
	/// <summary>
	/// Default channel provider factory
	/// </summary>
	public sealed class DefaultChannelProviderFactory(IEnumerable<IChannelProviderRegistration> registrations) : BaseChannelProviderFactory(registrations)
	{
		public override Task<IChannelProviderDispatcher?> ResolveDispatcherAsync(IChannelProviderRegistration channelProvider, IChannelProviderDispatcherRegistration channelProviderDispatcherRegistration)
		{
			IChannelProviderDispatcher? resolvedDispatcherInstance;
			if (channelProviderDispatcherRegistration.DispatcherType.GetConstructors().Length == 0)
				throw new CommunicationsException($"Cannot create an instance of {channelProviderDispatcherRegistration.DispatcherType}. No public constructors");

			if (channelProvider.Configuration == null)
			{
				resolvedDispatcherInstance = Activator.CreateInstance(channelProviderDispatcherRegistration.DispatcherType, [.. channelProviderDispatcherRegistration.DispatcherType.GetConstructors()[0].GetParameters().Select(x => Activator.CreateInstance(x.ParameterType))]) as IChannelProviderDispatcher;
			}
			else
			{
				resolvedDispatcherInstance = Activator.CreateInstance(channelProviderDispatcherRegistration.DispatcherType, channelProvider.Configuration) as IChannelProviderDispatcher;
			}
			return Task.FromResult(resolvedDispatcherInstance);
		}

		public override Task<IChannelProviderDeliveryReportRequestAdaptor> ResolveDeliveryReportRequestAdaptorAsync(IDeliveryReportRequestAdaptorRegistration channelProviderDeliveryReportRequestAdaptor)
		{
			Guard.AgainstNull(channelProviderDeliveryReportRequestAdaptor);
			var adaptor = Activator.CreateInstance(channelProviderDeliveryReportRequestAdaptor.Type) as IChannelProviderDeliveryReportRequestAdaptor;
			return Task.FromResult(Guard.AgainstNull(adaptor));
		}
	}
}