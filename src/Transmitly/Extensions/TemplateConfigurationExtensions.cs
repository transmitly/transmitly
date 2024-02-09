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
using Transmitly.Exceptions;
using Transmitly.Template.Configuration;

namespace Transmitly
{
	public static class TemplateConfigurationExtensions
	{
		public static Task<string?> RenderAsync(this IContentTemplateConfiguration contentTemplateConfiguration, IDispatchCommunicationContext communicationContext, bool configurationRequired = false)
		{
			var registration = GetTemplateRegistration(contentTemplateConfiguration, communicationContext.CultureInfo, configurationRequired);
			if (registration == null)
				return Task.FromResult<string?>(null);
			return communicationContext.TemplateEngine.RenderAsync(registration, communicationContext.ContentModel);
		}

		public static IContentTemplateRegistration? GetTemplateRegistration(this IContentTemplateConfiguration contentTemplateConfiguration, CultureInfo culture, bool configurationRequired = true)
		{
			var result = contentTemplateConfiguration?.TemplateRegistrations.FirstOrDefault(f => f.CultureInfo == culture);

			if (result == null && configurationRequired)
				throw new CommunicationsException($"Required template registration not found for culture '{culture.Name}'");
			return result;
		}

		public static IContentTemplateConfiguration AddStringTemplate(this IContentTemplateConfiguration templateConfig, string contents, string? cultureInfo = null)
		{
			Guard.AgainstNull(templateConfig);

			templateConfig.TemplateRegistrations.Add(new StringContentTemplateRegistration(contents, cultureInfo));
			return templateConfig;
		}
	}
}