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

namespace Transmitly.Tests.Integration
{
	internal sealed class OptionalConfigurationTestChannelProviderDispatcher : IChannelProviderDispatcher<UnitTestCommunication>
	{
		public IReadOnlyCollection<string>? RegisteredEvents { get; } = [];

		public Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(UnitTestCommunication communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
		{
			if (communication.Subject == "Skip me!")
				return Task.FromResult<IReadOnlyCollection<IDispatchResult?>>([]);//ie, I don't handle this kind of message.
			var result = new DispatchResult(DispatchResultStatus.Success("Dispatched"));

			return Task.FromResult<IReadOnlyCollection<IDispatchResult?>>([result]);
		}

		public Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(object communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
		{
			return DispatchAsync((UnitTestCommunication)communication, communicationContext, cancellationToken);
		}
	}
}
