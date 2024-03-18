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
using System.Net.Http;
using Transmitly.Exceptions;

namespace Transmitly.ChannelProvider
{
	public abstract class ChannelProviderRestClient<TCommunication>(HttpClient? httpClient) : ChannelProviderClient<TCommunication>
		where TCommunication : class
	{
		protected HttpClient HttpClient => GetHttpClient();
		private HttpClient? _httpClient = httpClient;
		private readonly object _resourceLock = new();

		protected abstract Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(HttpClient restClient, TCommunication communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken);

		protected virtual Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(HttpClient restClient, object communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
		{
			if (communication is not TCommunication)
				throw new CommunicationsException($"Provided communication is not of expected type, '{typeof(TCommunication).FullName}'");

			return DispatchAsync(restClient, (TCommunication)communication, communicationContext, cancellationToken);
		}

		public override Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(object communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
		{
			return DispatchAsync(GetHttpClient(), communication, communicationContext, cancellationToken);
		}

		public override Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(TCommunication communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
		{
			return DispatchAsync(GetHttpClient(), communication, communicationContext, cancellationToken);
		}

		protected HttpClient GetHttpClient()
		{
			if (_httpClient == null)
			{
				lock (_resourceLock)
				{
					if (_httpClient == null)
					{
						_httpClient = new HttpClient();
						ConfigureHttpClient(_httpClient);
					}
				}
			}
			return _httpClient;
		}

		protected virtual void ConfigureHttpClient(HttpClient client)
		{
			Guard.AgainstNull(client);
			client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
		}
	}
}