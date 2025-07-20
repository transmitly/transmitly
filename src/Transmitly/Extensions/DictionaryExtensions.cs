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

namespace Transmitly;

/// <summary>
/// Extension methods for dictionaries.
/// </summary>
internal static class DictionaryExtensions
{
	/// <summary>
	/// Safely gets the value associated with the specified key from the dictionary.
	/// </summary>
	/// <typeparam name="TReturn">The type of the return value.</typeparam>
	/// <param name="dict">The dictionary.</param>
	/// <param name="key">The key.</param>
	/// <returns>The value associated with the specified key, or the default value of TReturn if the key is not found.</returns>
	public static TReturn? Get<TReturn>(this IDictionary<string, object> dict, string key)
	{
		if (dict.TryGetValue(key, out object? value))
			return (TReturn)value;
		return default;
	}
}
