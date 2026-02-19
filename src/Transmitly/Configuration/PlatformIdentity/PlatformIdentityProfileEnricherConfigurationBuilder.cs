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

namespace Transmitly.PlatformIdentity.Configuration;

/// <summary>
/// Represents a builder for configuring platform identity profile enrichers.
/// </summary>
public sealed class PlatformIdentityProfileEnricherConfigurationBuilder
{
	private readonly CommunicationsClientBuilder _communicationsConfiguration;
	private readonly Action<IPlatformIdentityProfileEnricherRegistration> _addPlatformIdentityProfileEnricher;

	/// <summary>
	/// Initializes a new instance of the <see cref="PlatformIdentityProfileEnricherConfigurationBuilder"/> class.
	/// </summary>
	/// <param name="communicationsConfiguration">The communications configuration builder.</param>
	/// <param name="addPlatformIdentityProfileEnricher">The action to add a platform identity profile enricher.</param>
	internal PlatformIdentityProfileEnricherConfigurationBuilder(
		CommunicationsClientBuilder communicationsConfiguration,
		Action<IPlatformIdentityProfileEnricherRegistration> addPlatformIdentityProfileEnricher)
	{
		_communicationsConfiguration = Guard.AgainstNull(communicationsConfiguration);
		_addPlatformIdentityProfileEnricher = Guard.AgainstNull(addPlatformIdentityProfileEnricher);
	}

	/// <summary>
	/// Adds a platform identity profile enricher to the communications configuration.
	/// </summary>
	/// <param name="platformIdentityType">Optional platform identity type filter.</param>
	/// <param name="order">The deterministic execution order for the enricher. Lower values run first.</param>
	/// <returns>The communications configuration builder.</returns>
	public CommunicationsClientBuilder Add<TEnricher>(string? platformIdentityType = null, int order = 0)
		where TEnricher : IPlatformIdentityProfileEnricher
	{
		_addPlatformIdentityProfileEnricher(new PlatformIdentityProfileEnricherRegistration(typeof(TEnricher), order, platformIdentityType));

		return _communicationsConfiguration;
	}
}

