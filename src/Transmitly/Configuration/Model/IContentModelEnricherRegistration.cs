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

namespace Transmitly.Model.Configuration;

/// <summary>
/// Represents a content model enricher registration.
/// </summary>
public interface IContentModelEnricherRegistration
{
	/// <summary>
	/// Gets the enricher type.
	/// </summary>
	Type EnricherType { get; }

	/// <summary>
	/// Gets the enricher scope.
	/// </summary>
	ContentModelEnricherScope Scope { get; }

	/// <summary>
	/// Gets whether to continue invoking additional enrichers when a model is enriched.
	/// </summary>
	bool ContinueOnEnrichedModel { get; }

	/// <summary>
	/// Gets the optional filter predicate.
	/// </summary>
	Func<IDispatchCommunicationContext, bool>? Predicate { get; }

	/// <summary>
	/// Gets the optional order for this enricher.
	/// </summary>
	int? Order { get; }
}
