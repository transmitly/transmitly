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

using System.Collections;
using System.Runtime.Serialization;

namespace Transmitly
{
	/// <inheritdoc />
	public sealed class ExtendedProperties : IExtendedProperties
	{
		internal ExtendedProperties()
		{

		}

		readonly Dictionary<string, object?> _bag = [];

		int IReadOnlyCollection<KeyValuePair<string, object?>>.Count => throw new NotImplementedException();

		/// <inheritdoc />
		void IExtendedProperties.Add(string providerKey, string propertyKey, object? value)
		{
			var key = GetCompositeKey(providerKey, propertyKey);
			_bag.Add(key, value);
		}

		/// <inheritdoc />
		void IExtendedProperties.AddOrUpdate(string providerKey, string propertyKey, object? value)
		{
			var key = GetCompositeKey(providerKey, propertyKey);
#if FEATURE_DICTIONARYTRYADD
			_bag.TryAdd(key, value);
#else
			if (_bag.ContainsKey(key))
				_bag[key] = value;
			else
				_bag.Add(key, value);
#endif

		}

		/// <inheritdoc />
		object? IExtendedProperties.this[string providerKey, string key] =>
			((IExtendedProperties)this).GetValue(providerKey, key);

		/// <inheritdoc />
		T? IExtendedProperties.GetValue<T>(string providerKey, string propertyKey)
			where T : default
		{
			var key = GetCompositeKey(providerKey, propertyKey);

#if FEATURE_DICTIONARYTRYADD
			if (!_bag.TryGetValue(key, out var result))
				return (T?)result;
#else
			if (_bag.ContainsKey(key))
				return (T?)_bag[key];
#endif
			return default;
		}

		/// <inheritdoc />
		object? IExtendedProperties.GetValue(string providerKey, string propertyKey)
		{
			var key = GetCompositeKey(providerKey, propertyKey);
#if FEATURE_DICTIONARYTRYADD
			if (_bag.TryGetValue(key, out var result))
				return result;
#else
			if (_bag.ContainsKey(key))
				return _bag[key];
#endif
			return null;
		}

		/// <inheritdoc />
		bool IExtendedProperties.TryGetValue<T>(string providerKey, string propertyKey, out T? value)
			where T : default
		{
			var key = GetCompositeKey(providerKey, propertyKey);

			var result = _bag.TryGetValue(key, out var objValue);
			value = (T?)objValue;
			return result;
		}

		/// <inheritdoc />
		T IExtendedProperties.GetValue<T>(string providerKey, string propertyKey, T defaultValue)
		{
			if (defaultValue == null)
				throw new ArgumentNullException(nameof(defaultValue));

			var key = GetCompositeKey(providerKey, propertyKey);
#if FEATURE_DICTIONARYTRYADD
			if (_bag.TryGetValue(key, out var result))
				return (T?)result ?? defaultValue;
#else
			if (_bag.ContainsKey(key))
				return (T?)_bag[key] ?? defaultValue;
			
#endif
			return defaultValue;
		}

		private static string GetCompositeKey(string providerKey, string propertyKey)
		{
			return string.Join(".", providerKey, propertyKey);
		}

#if NET8_0_OR_GREATER
		//https://aka.ms/dotnet-warnings/SYSLIB0051
		[Obsolete("https://aka.ms/dotnet-warnings/SYSLIB0051", DiagnosticId = "SYSLIB0051")]
#endif
		/// <inheritdoc />
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			_bag.GetObjectData(info, context);
		}

		/// <inheritdoc />
		IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator()
		{
			return _bag.GetEnumerator();
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_bag).GetEnumerator();
		}
	}
}