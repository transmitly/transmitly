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

namespace Transmitly.Audience.Configuration
{
	/// <summary>
	/// Represents a builder for configuring audience resolvers in the communications configuration.
	/// </summary>
	public sealed class AudienceResolverConfigurationBuilder
	{
		private readonly CommunicationsClientBuilder _communicationsConfiguration;
		private readonly Action<IAudienceResolver> _addAudienceResolver;

		/// <summary>
		/// Initializes a new instance of the <see cref="AudienceResolverConfigurationBuilder"/> class.
		/// </summary>
		/// <param name="communicationsConfiguration">The communications configuration builder.</param>
		/// <param name="addAudienceResolver">The action to add an audience resolver.</param>
		internal AudienceResolverConfigurationBuilder(CommunicationsClientBuilder communicationsConfiguration, Action<IAudienceResolver> addAudienceResolver)
		{
			_communicationsConfiguration = Guard.AgainstNull(communicationsConfiguration);
			_addAudienceResolver = Guard.AgainstNull(addAudienceResolver);
		}

		/// <summary>
		/// Adds a generic audience resolver to the communications configuration.
		/// </summary>
		/// <param name="audienceResolver">The audience resolver function.</param>
		/// <returns>The communications configuration builder.</returns>
		public CommunicationsClientBuilder AddGeneric(AudienceResolverFunc audienceResolver)
		{
			_addAudienceResolver(new AudienceResolverRegistration(null, audienceResolver));
			return _communicationsConfiguration;
		}

		/// <summary>
		/// Adds an audience resolver to the communications configuration.
		/// </summary>
		/// <param name="audienceTypeIdentifier">The identifier for the audience type.</param>
		/// <param name="audienceResolver">The audience resolver function.</param>
		/// <returns>The communications configuration builder.</returns>
		public CommunicationsClientBuilder Add(string audienceTypeIdentifier, AudienceResolverFunc audienceResolver)
		{
			if (string.IsNullOrWhiteSpace(audienceTypeIdentifier))
			{
				throw new ArgumentException($"'{nameof(audienceTypeIdentifier)}' cannot be null or whitespace.\r\nConsider using '{nameof(Add)}' method for registering an audience resolver without an audience type identifier", nameof(audienceTypeIdentifier));
			}
			_addAudienceResolver(new AudienceResolverRegistration(audienceTypeIdentifier, audienceResolver));
			return _communicationsConfiguration;
		}
	}
}