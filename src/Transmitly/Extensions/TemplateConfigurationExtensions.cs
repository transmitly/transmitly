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

using System.Globalization;
using System.Reflection;
using Transmitly.Exceptions;
using Transmitly.Template.Configuration;

namespace Transmitly
{
	/// <summary>
	/// Template configuration related extensions.
	/// </summary>
	public static class TemplateConfigurationExtensions
	{
		/// <summary>
		/// Renders the template configuration with the registered template engine.
		/// </summary>
		/// <param name="contentTemplateConfiguration">Template configuration.</param>
		/// <param name="communicationContext">Context of the communication dispatch.</param>
		/// <param name="configurationRequired">Whether the template configuration is required to exist.</param>
		/// <returns>Rendered content. Otherwise null.</returns>
		/// <exception cref="CommunicationsException">If the template configuration is not found and <paramref name="configurationRequired"/>=true.</exception>
		public static Task<string?> RenderAsync(this IContentTemplateConfiguration contentTemplateConfiguration, IDispatchCommunicationContext communicationContext, bool configurationRequired = false)
		{
			var registration = GetTemplateRegistration(contentTemplateConfiguration, communicationContext.CultureInfo, configurationRequired);
			if (registration == null)
				return Task.FromResult<string?>(null);
			return communicationContext.TemplateEngine.RenderAsync(registration, communicationContext);
		}

		/// <summary>
		/// Gets the template configuration for the provided culture.
		/// </summary>
		/// <param name="contentTemplateConfiguration">Template configuration.</param>
		/// <param name="culture">Culture of the template configuration.</param>
		/// <param name="configurationRequired">Whether the template configuration is required to exist.</param>
		/// <returns>Template configuration; Otherwise null.</returns>
		/// <exception cref="CommunicationsException">If the template configuration is not found and <paramref name="configurationRequired"/>=true.</exception>
		public static IContentTemplateRegistration? GetTemplateRegistration(this IContentTemplateConfiguration contentTemplateConfiguration, CultureInfo culture, bool configurationRequired = true)
		{
			var result = contentTemplateConfiguration?.TemplateRegistrations.FirstOrDefault(f => f.CultureInfo == culture);

			if (result == null && configurationRequired)
				throw new CommunicationsException($"Required template registration not found for culture '{culture.Name}'");
			return result;
		}

		/// <summary>
		/// Registers a string template configuration.
		/// </summary>
		/// <param name="templateConfig">Template configuration.</param>
		/// <param name="contents">Content of the template.</param>
		/// <param name="cultureInfo">Specified culture of the template; otherwise <see cref="CultureInfo.InvariantCulture"></param>
		/// <returns>Template configuration.</returns>
		public static IContentTemplateConfiguration AddStringTemplate(this IContentTemplateConfiguration templateConfig, string contents, string? cultureInfo = null)
		{
			Guard.AgainstNull(templateConfig);

			templateConfig.TemplateRegistrations.Add(new StringContentTemplateRegistration(contents, cultureInfo));
			return templateConfig;
		}

		/// <summary>
		/// Registers an embedded resource template configuration.
		/// </summary>
		/// <param name="templateConfig">Template configuration.</param>
		/// <param name="resourceId">Assembly resource id.</param>
		/// <param name="assembly">Optional assembly the resource id is located in.</param>
		/// <param name="cultureInfo">Specified culture of the template; otherwise <see cref="CultureInfo.InvariantCulture"></param>
		/// <returns>Template configuration.</returns>
		public static IContentTemplateConfiguration AddEmbeddedResourceTemplate(this IContentTemplateConfiguration templateConfig, string resourceId, Assembly? assembly = null, string? cultureInfo = null)
		{
			Guard.AgainstNull(templateConfig);

			templateConfig.TemplateRegistrations.Add(new EmbeddedResourceContentTemplateRegistration(resourceId, cultureInfo, assembly));
			return templateConfig;
		}

		/// <summary>
		/// Registers a dynamic template configuration.
		/// </summary>
		/// <param name="templateConfiguration">Template configuration.</param>
		/// <param name="templateResolver">Delegate that will resolve template content.</param>
		/// <param name="cultureInfo">Specified culture of the template; otherwise <see cref="CultureInfo.InvariantCulture"></param>
		/// <returns>Template configuration.</returns>
		public static IContentTemplateConfiguration AddDynamicTemplate(this IContentTemplateConfiguration templateConfiguration, Func<IDispatchCommunicationContext, Task<string?>> templateResolver, string? cultureInfo = null)
		{
			Guard.AgainstNull(templateConfiguration);
			templateConfiguration.TemplateRegistrations.Add(new DelegateContentTemplateRegistration(templateResolver, cultureInfo));
			return templateConfiguration;
		}
	}
}