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

namespace Transmitly.Audience.Configuration
{
	/// <summary>
	/// Represents a registration for an audience resolver.
	/// </summary>
	/// <remarks>
	/// Initializes a new instance of the <see cref="AudienceResolverRegistration"/> class.
	/// </remarks>
	/// <param name="audienceTypeIdentifier">The identifier for the audience type.</param>
	/// <param name="audienceResolver">The audience resolver function.</param>
	internal class AudienceResolverRegistration(string? audienceTypeIdentifier, AudienceResolverFunc audienceResolver) : IAudienceResolver
	{

		/// <summary>
		/// Gets the identifier for the audience type.
		/// </summary>
		public string? AudienceTypeIdentifier { get; } = audienceTypeIdentifier;

		/// <summary>
		/// Gets the audience resolver function.
		/// </summary>
		public AudienceResolverFunc ResolveAsync { get; } = Guard.AgainstNull(audienceResolver);
	}
}
