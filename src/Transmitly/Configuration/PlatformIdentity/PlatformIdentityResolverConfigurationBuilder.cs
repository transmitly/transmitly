﻿// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
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

namespace Transmitly.PlatformIdentity.Configuration;

/// <summary>
/// Represents a builder for configuring platform identity resolvers in the communications configuration.
/// </summary>
public sealed class PlatformIdentityResolverConfigurationBuilder
{
	private readonly CommunicationsClientBuilder _communicationsConfiguration;
	private readonly Action<IPlatformIdentityResolverRegistration> _addPlatformIdentityResolver;

	/// <summary>
	/// Initializes a new instance of the <see cref="PlatformIdentityResolverConfigurationBuilder"/> class.
	/// </summary>
	/// <param name="communicationsConfiguration">The communications configuration builder.</param>
	/// <param name="addPlatformIdentityResolver">The action to add an platform identity resolver.</param>
	internal PlatformIdentityResolverConfigurationBuilder(CommunicationsClientBuilder communicationsConfiguration, Action<IPlatformIdentityResolverRegistration> addPlatformIdentityResolver)
	{
		_communicationsConfiguration = Guard.AgainstNull(communicationsConfiguration);
		_addPlatformIdentityResolver = Guard.AgainstNull(addPlatformIdentityResolver);
	}

	/// <summary>
	/// Adds an platform identity resolver to the communications configuration.
	/// </summary>
	/// <param name="platformIdentityType">The platform identity type.</param>
	/// <returns>The communications configuration builder.</returns>
	public CommunicationsClientBuilder Add<TResolver>(string? platformIdentityType = null)
		where TResolver : IPlatformIdentityResolver
	{
		_addPlatformIdentityResolver(new PlatformIdentityResolverRegistration(typeof(TResolver), platformIdentityType));

		return _communicationsConfiguration;
	}
}