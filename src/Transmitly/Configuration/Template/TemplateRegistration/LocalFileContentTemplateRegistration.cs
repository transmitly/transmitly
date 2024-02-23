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

namespace Transmitly.Template.Configuration
{
	internal sealed class LocalFileContentTemplateRegistration : IContentTemplateRegistration
	{
		private readonly string _path;
		private readonly bool _exists;
		
		public CultureInfo CultureInfo { get; }

		public LocalFileContentTemplateRegistration(string path, bool throwIfNotFound = true, string? cultureInfo = null)
		{
			_path = Guard.AgainstNullOrWhiteSpace(path);
			CultureInfo = GuardCulture.AgainstNull(cultureInfo);
			_exists = File.Exists(path);
			if (throwIfNotFound && !_exists)
			{
				throw new FileNotFoundException($"Template File, '{path}', not found.");
			}
		}

#if NET6_0_OR_GREATER
		public async Task<string?> GetContentAsync(IDispatchCommunicationContext context)
		{
			if (!_exists)
				return null;
			return await File.ReadAllTextAsync(_path);
		}
#else
		public Task<string?> GetContentAsync(IDispatchCommunicationContext context)
		{
			if(!_exists)
				return Task.FromResult<string?>(null);
			return Task.FromResult<string?>(File.ReadAllText(_path));
		}
#endif
	}
}