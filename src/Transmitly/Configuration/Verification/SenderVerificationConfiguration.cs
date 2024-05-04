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

using Transmitly.Template.Configuration;

namespace Transmitly.Verification.Configuration
{
	public sealed class SenderVerificationConfiguration : ISenderVerificationConfiguration
	{
		public Type DefaultSenderVerificationProvider { get; private set; } = typeof(DefaultSenderVerificationService);
		public Type CodeGenerationServiceType { get; private set; } = typeof(DefaultSenderVerificationCodeProvider);
		public string? StatusCallbackUrl { get; set; }

		public IContentTemplateConfiguration Message { get; } = new ContentTemplateConfiguration();

		internal SenderVerificationConfiguration()
		{
		}

		public ISenderVerificationConfiguration UseDefaultSenderVerificationService<TService>() where TService : ISenderVerificationService
		{
			DefaultSenderVerificationProvider = typeof(TService);
			return this;
		}

		public ISenderVerificationConfiguration UseCodeGenerationService<TGenerator>() where TGenerator : ISenderVerificationCodeGenerator
		{
			CodeGenerationServiceType = typeof(TGenerator);
			return this;
		}
	}
}