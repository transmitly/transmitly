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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Transmitly
{
	/// <summary>
	/// Provides a set of guard methods for validating arguments.
	/// </summary>
	[DebuggerStepThrough]
	public static class Guard
	{
		/// <summary>
		/// Throws an <see cref="ArgumentNullException"/> if the specified string is null, empty, or consists only of white-space characters.
		/// </summary>
		/// <param name="argument">The string argument to check.</param>
		/// <param name="paramName">The name of the parameter.</param>
		/// <returns>The original string argument if it is not null, empty, or consists only of white-space characters.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the specified string is null, empty, or consists only of white-space characters.</exception>
		public static string AgainstNullOrWhiteSpace(
			[NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
		{
			if (string.IsNullOrWhiteSpace(argument))
			{
				throw new ArgumentNullException(paramName);
			}
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
			return argument!;
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.
		}

		/// <summary>
		/// Throws an <see cref="ArgumentNullException"/> if the specified collection is null or empty.
		/// </summary>
		/// <typeparam name="T">The type of the collection elements.</typeparam>
		/// <param name="collection">The collection to check.</param>
		/// <param name="paramName">The name of the parameter.</param>
		/// <returns>The original collection if it is not null or empty.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the specified collection is null or empty.</exception>
		public static ICollection<T> AgainstNullOrEmpty<T>(
			[NotNull] ICollection<T>? collection,
			[CallerArgumentExpression(nameof(collection))] string? paramName = null)
		{
			if (collection == null || collection.Count == 0)
			{
				throw new ArgumentNullException(paramName);
			}

			return collection;
		}

		public static IReadOnlyCollection<T> AgainstNullOrEmpty<T>(
			[NotNull] IReadOnlyCollection<T>? collection,
			[CallerArgumentExpression(nameof(collection))] string? paramName = null)
		{
			if (collection == null || collection.Count == 0)
			{
				throw new ArgumentNullException(paramName);
			}

			return collection;
		}

		/// <summary>
		/// Throws an <see cref="ArgumentNullException"/> if the specified argument is null.
		/// </summary>
		/// <typeparam name="T">The type of the argument.</typeparam>
		/// <param name="argument">The argument to check.</param>
		/// <param name="paramName">The name of the parameter.</param>
		/// <returns>The original argument if it is not null.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the specified argument is null.</exception>
		public static T AgainstNull<T>(
			[NotNull] T? argument,
			[CallerArgumentExpression(nameof(argument))] string? paramName = null)
			where T : class
		{
			if (argument is null)
			{
				throw new ArgumentNullException(paramName);
			}

			return argument;
		}
	}
}
