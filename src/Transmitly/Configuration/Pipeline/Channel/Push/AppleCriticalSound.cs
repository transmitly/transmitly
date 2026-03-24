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

namespace Transmitly.Channel.Configuration.Push;

/// <summary>
/// Apple critical sound configuration.
/// </summary>
public sealed class AppleCriticalSound
{
	/// <summary>
	/// Whether the alert should be marked as critical.
	/// </summary>
	public bool? IsCritical { get; init; }

	/// <summary>
	/// Sound name in the application bundle or <c>Library/Sounds</c>.
	/// </summary>
	public string? Name { get; init; }

	/// <summary>
	/// Sound volume from <c>0.0</c> to <c>1.0</c>.
	/// </summary>
	public double? Volume { get; init; }
}
