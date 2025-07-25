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

using Transmitly.Template.Configuration;

namespace Transmitly.Tests;

internal sealed class UnitTestTemplateEngine : ITemplateEngine
{
	public async Task<string?> RenderAsync(IContentTemplateRegistration? registration, IDispatchCommunicationContext context)
	{
		if (registration == null)
			return null;
		var contentModel = context.ContentModel;
		var templateContent = await registration.GetContentAsync(context);

		if (string.IsNullOrWhiteSpace(templateContent) || contentModel == null)
			return default;

		Type type = contentModel!.GetType();

		foreach (var property in type.GetProperties())
		{
			templateContent = templateContent!.Replace("{{" + property.Name + "}}", property.GetValue(contentModel)?.ToString());
		}

		return templateContent;
	}
}