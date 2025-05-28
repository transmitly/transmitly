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

using System.Globalization;

namespace Transmitly.Template.Configuration;

internal sealed class StringContentTemplateRegistration : IContentTemplateRegistration
{
	private readonly string? _content;

	public StringContentTemplateRegistration(string content, string? cultureInfo = null)
	{
		Guard.AgainstNullOrWhiteSpace(content);
		CultureInfo = GuardCulture.AgainstNull(cultureInfo);
		_content = content;
	}

	public CultureInfo CultureInfo { get; }

	public Task<string?> GetContentAsync(IDispatchCommunicationContext context)
	{
		return Task.FromResult(_content);
	}
}