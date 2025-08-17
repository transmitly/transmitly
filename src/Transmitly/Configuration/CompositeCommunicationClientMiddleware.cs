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

using Transmitly.Exceptions;

namespace Transmitly;

sealed class CompositeCommunicationClientMiddleware : ICommunicationClientMiddleware
{
	private readonly List<ICommunicationClientMiddleware> _middlewares = [];
	public CompositeCommunicationClientMiddleware(ICommunicationClientMiddleware defaultCommunicationClientMiddleware)
	{
		_middlewares = [defaultCommunicationClientMiddleware];
	}

	public CompositeCommunicationClientMiddleware()
	{

	}

	public void AddFactory(ICommunicationClientMiddleware factory, int? index)
	{
		if (!index.HasValue)
		{
			_middlewares.Add(factory);
		}
		else if (index.Value < 0 || index.Value > _middlewares.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index), "Index must be within the range of the current middlewares.");
		}
		else
		{
			_middlewares.Insert(index.Value, factory);
		}
	}

	public ICommunicationsClient CreateClient(ICreateCommunicationsClientContext context, ICommunicationsClient? previous)
	{
		ICommunicationsClient? client = null;
		foreach (var middleware in _middlewares)
		{
			client = middleware.CreateClient(context, client);
		}

		if (client == null)
		{
			throw new CommunicationsException("No available communications client middelware created a communications client.");
		}
		return client;
	}
}