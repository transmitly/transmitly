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

namespace Transmitly.Template.Configuration;

/// <summary>
/// Represents a builder for configuring template settings.
/// </summary>
public sealed class TemplateConfigurationBuilder
{
	private readonly CommunicationsClientBuilder _communicationsConfiguration;
	private readonly Action<ITemplateEngineRegistration> _addTemplateEngine;

	/// <summary>
	/// Initializes a new instance of the <see cref="TemplateConfigurationBuilder"/> class.
	/// </summary>
	/// <param name="communicationsConfiguration">The communications configuration builder.</param>
	/// <param name="addTemplateEngine">The action to add a template engine.</param>
	internal TemplateConfigurationBuilder(CommunicationsClientBuilder communicationsConfiguration, Action<ITemplateEngineRegistration> addTemplateEngine)
	{
		_communicationsConfiguration = Guard.AgainstNull(communicationsConfiguration);
		_addTemplateEngine = Guard.AgainstNull(addTemplateEngine);
	}

	/// <summary>
	/// Adds a template engine to the communications configuration.
	/// </summary>
	/// <param name="engineInstance">The template engine instance.</param>
	/// <param name="id">The optional ID for the template engine.</param>
	/// <returns>The communications configuration builder.</returns>
	public CommunicationsClientBuilder Add(ITemplateEngine engineInstance, string? id = null)
	{
		_addTemplateEngine(new TemplateEngineRegistration(engineInstance, id));
		return _communicationsConfiguration;
	}
}