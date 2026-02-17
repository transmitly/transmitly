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

namespace Transmitly;

/// <summary>
/// Context information for dispatch middleware.
/// </summary>
public sealed class DispatchMiddlewareContext(IChannelProvider provider,
		IDispatchCommunicationContext context,
		object communication,
		IChannelProviderDispatcher dispatcher,
		CancellationToken token)
{

	/// <summary>
	/// Gets the channel provider used for dispatching.
	/// </summary>
	public IChannelProvider Provider { get; } = Guard.AgainstNull(provider);

	/// <summary>
	/// Gets the dispatch communication context.
	/// </summary>
	public IDispatchCommunicationContext Context { get; } = Guard.AgainstNull(context);

	/// <summary>
	/// Gets the channel communication being dispatched.
	/// </summary>
	public object Communication { get; } = Guard.AgainstNull(communication);

	/// <summary>
	/// Gets the dispatcher instance.
	/// </summary>
	public IChannelProviderDispatcher Dispatcher { get; } = Guard.AgainstNull(dispatcher);

	/// <summary>
	/// Gets the cancellation token.
	/// </summary>
	public CancellationToken Token { get; } = token;
}

