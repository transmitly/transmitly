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

#if NETFRAMEWORK
using System.Net.Http;
#endif
using System.Net.Http.Headers;
using Transmitly.Exceptions;

namespace Transmitly.ChannelProvider;

public abstract class ChannelProviderRestDispatcher<TCommunication> : ChannelProviderDispatcher<TCommunication>, IDisposable
	where TCommunication : class
{
	private readonly Lazy<HttpClient> _lazyHttpClient;
	private bool _disposedValue;
	protected HttpClient HttpClient => _lazyHttpClient.Value;

	protected ChannelProviderRestDispatcher(HttpClient? httpClient = null)
	{
		_lazyHttpClient = new Lazy<HttpClient>(() =>
		{
			var client = httpClient ?? new HttpClient();
			ConfigureHttpClient(client);
			return client;
		});
	}

	protected abstract Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(HttpClient restClient, TCommunication communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken);

	protected virtual Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(HttpClient restClient, object communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
	{
		if (communication is not TCommunication)
			throw new CommunicationsException($"Provided communication is not of expected type, '{typeof(TCommunication).FullName}'");

		return DispatchAsync(restClient, (TCommunication)communication, communicationContext, cancellationToken);
	}

	public override Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(object communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
	{
		return DispatchAsync(HttpClient, communication, communicationContext, cancellationToken);
	}

	public override Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(TCommunication communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
	{
		return DispatchAsync(HttpClient, communication, communicationContext, cancellationToken);
	}

	protected virtual void ConfigureHttpClient(HttpClient httpClient)
	{
		Guard.AgainstNull(httpClient);
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing && _lazyHttpClient.IsValueCreated)
			{
				_lazyHttpClient.Value.Dispose();
			}

			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}