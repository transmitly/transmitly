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
/// Provides extension methods for configuring channel providers.
/// </summary>
public static class ChannelProviderBuilderExtensions
{
	/// <summary>
	/// Adds dispatch simulation support to the communications client builder.
	/// </summary>
	/// <param name="builder">The communications client builder to configure.</param>
	/// <param name="configure">Optional simulation configuration.</param>
	/// <param name="providerId">Optional provider identifier suffix.</param>
	/// <returns>The same instance of <see cref="CommunicationsClientBuilder"/>.</returns>
	public static CommunicationsClientBuilder AddSimulationSupport(this CommunicationsClientBuilder builder, Action<ChannelProviderSimulationOptions>? configure = null, string? providerId = null)
	{
		Guard.AgainstNull(builder);

		var options = new ChannelProviderSimulationOptions();
		configure?.Invoke(options);

		builder.ChannelProvider
			.Build(Id.ChannelProvider.Simulation(providerId), options)
			.AddDispatcher<SimulationChannelProviderDispatcher, object>()
			.Register();

		return builder;
	}

	/// <summary>
	/// Gets the channel provider ID for the simulation provider, optionally appending a provider Id.
	/// </summary>
	/// <param name="channelProviders">The channel providers instance.</param>
	/// <param name="providerId">The optional provider identifier suffix.</param>
	/// <returns>The channel provider Id for the simulation provider.</returns>
	public static string Simulation(this ChannelProviders channelProviders, string? providerId = null) =>
		channelProviders.GetId("Simulation", providerId);
}