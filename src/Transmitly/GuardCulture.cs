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
using System.Globalization;

namespace Transmitly.Util
{
	/// <summary>
	/// Provides methods for guarding against null values and invalid culture names.
	/// </summary>
	[DebuggerStepThrough]
	public static class GuardCulture
	{
		private static readonly HashSet<string> CultureNames = CreateCultureNames();

		/// <summary>
		/// Returns the specified culture if it is not null, otherwise returns the invariant culture.
		/// </summary>
		/// <param name="name">The name of the culture.</param>
		/// <returns>The specified culture or the invariant culture.</returns>
		public static CultureInfo AgainstNull(string? name)
		{
			if (!IsValid(name))
				return CultureInfo.InvariantCulture;
			return new CultureInfo(name!);
		}

		/// <summary>
		/// Returns the specified culture if it is not null, otherwise returns the invariant culture.
		/// </summary>
		/// <param name="cultureInfo">The culture info.</param>
		/// <returns>The specified culture or the invariant culture.</returns>
		public static CultureInfo AgainstNull(CultureInfo? cultureInfo)
		{
			if (cultureInfo == null)
				return CultureInfo.InvariantCulture;
			return cultureInfo;
		}

		/// <summary>
		/// Checks if the culture name is valid
		/// </summary>
		/// <param name="name">Culture name</param>
		/// <returns>true if valid; otherwise false</returns>
		private static bool IsValid(string? name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return false;

			return CultureNames.Contains(name!);
		}

		/// <summary>
		/// Gets the available culture names
		/// </summary>
		/// <returns>Available culture names</returns>
		private static HashSet<string> CreateCultureNames()
		{
			var cultureInfoArray = CultureInfo.GetCultures(CultureTypes.AllCultures)
										  .Where(x => !string.IsNullOrWhiteSpace(x.Name))
										  .ToArray();
			var allNames = new HashSet<string>(
				cultureInfoArray.Select(x => x.TwoLetterISOLanguageName),
				StringComparer.OrdinalIgnoreCase
			);
			allNames.UnionWith(cultureInfoArray.Select(x => x.Name));
			return allNames;
		}
	}
}