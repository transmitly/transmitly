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

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Transmitly;
using Transmitly.PlatformIdentity.Configuration;

namespace Tandely.Notifications.Service
{
	public sealed class CustomerRepository(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IPlatformIdentityResolver
	{
		private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

		public async Task<IReadOnlyCollection<IPlatformIdentityProfile>?> ResolveIdentityProfiles(IReadOnlyCollection<IPlatformIdentityReference> identityReferences)
		{
			var tasks = identityReferences.Select(async identityReference =>
			{
				var response = await _httpClient.GetAsync($"customers/{identityReference.Id}");
				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStreamAsync();
					var customer = await JsonSerializer.DeserializeAsync<Customer?>(content, jsonOptions.Value.SerializerOptions);
					return customer;
				}
				return null;
			});
			await Task.WhenAll(tasks);
			return tasks.Select(x => x.Result).Where(x => x != null).Cast<IPlatformIdentityProfile>().ToList().AsReadOnly();
		}
	}
}
