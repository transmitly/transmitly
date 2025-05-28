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

///Source=https://stackoverflow.com/a/39597587
static class AsyncLinqExtensions
{
	public static async Task<bool> AllAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task<bool>> predicate)
	{

#if NET6_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(nameof(source));
		ArgumentNullException.ThrowIfNull(nameof(predicate));
#else
		if (source == null)
			throw new ArgumentNullException(nameof(source));
		if (predicate == null)
			throw new ArgumentNullException(nameof(predicate));
#endif

		foreach (var item in source)
		{
			var result = await predicate(item);
			if (!result)
				return false;
		}
		return true;
	}

	// This is for synchronous predicates with an async source.
	public static async Task<bool> AllAsync<TSource>(this IEnumerable<Task<TSource>> source, Func<TSource, bool> predicate)
	{
#if NET6_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(nameof(source));
		ArgumentNullException.ThrowIfNull(nameof(predicate));
#else
		if (source == null)
			throw new ArgumentNullException(nameof(source));
		if (predicate == null)
			throw new ArgumentNullException(nameof(predicate));
#endif

		foreach (var item in source)
		{
			var awaitedItem = await item;
			if (!predicate(awaitedItem))
				return false;
		}
		return true;
	}
}