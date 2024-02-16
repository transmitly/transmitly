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

namespace Transmitly.Template.Configuration
{
	internal sealed class EmbeddedResourceContentTemplateRegistration : IContentTemplateRegistration
	{
		private readonly Assembly _assembly;
		private readonly string _resourceId;

		public EmbeddedResourceContentTemplateRegistration(string resourceId, string? cultureInfo = null, Assembly? resourceAssembly = null)
		{
			_assembly = Guard.AgainstNull(resourceAssembly ?? Assembly.GetEntryAssembly());
			_resourceId = Guard.AgainstNullOrWhiteSpace(resourceId);
			CultureInfo = GuardCulture.AgainstNull(cultureInfo);

			if (!Array.Exists(_assembly.GetManifestResourceNames(), r => r == _resourceId))
				throw new CommunicationsException($"Embedded Content Template cannot find resource '{_resourceId}' in assembly '{_assembly.FullName}'.");
		}

		public CultureInfo CultureInfo { get; }

		public Task<string?> GetContentAsync()
		{
			using var stream = _assembly.GetManifestResourceStream(_resourceId);
			using var reader = new StreamReader(stream!);
			return Task.FromResult<string?>(reader.ReadToEnd());
		}
	}
}