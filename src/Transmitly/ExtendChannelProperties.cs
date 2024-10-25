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
using System.ComponentModel;
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

		int IReadOnlyCollection<KeyValuePair<string, object?>>.Count => _bag.Count;

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
			if (!_bag.TryAdd(key, value))
				_bag[key] = value;
#else
			if (!_bag.ContainsKey(key))
				_bag.Add(key, value);
			else
				_bag[key] = value;
#endif
		}

		object? IExtendedProperties.this[string propertyKey] =>
			((IExtendedProperties)this).GetValue(propertyKey);

		/// <inheritdoc />
		object? IExtendedProperties.this[string providerKey, string key] =>
			((IExtendedProperties)this).GetValue(providerKey, key);

		/// <inheritdoc />
		T? IExtendedProperties.GetValue<T>(string providerKey, string propertyKey)
			where T : default
		{
			var key = GetCompositeKey(providerKey, propertyKey);

#if FEATURE_DICTIONARYTRYADD
			if (_bag.TryGetValue(key, out var result))
				return ConvertType<T>(result);
#else
			if (_bag.ContainsKey(key))
				return ConvertType<T>(_bag[key]);
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
			if (result)
				value = ConvertType<T>(objValue);
			else
				value = default;

			return result;
		}

		/// <inheritdoc />
		T IExtendedProperties.GetValue<T>(string providerKey, string propertyKey, T defaultValue)
		{
			var key = GetCompositeKey(providerKey, propertyKey);

#if FEATURE_DICTIONARYTRYADD
			if (_bag.TryGetValue(key, out var result))
				return ConvertType<T>(result) ?? defaultValue;
#else
			if (_bag.ContainsKey(key))
				return ConvertType<T>(_bag[key]) ?? defaultValue;

#endif
			return defaultValue;
		}

		private static string GetCompositeKey(string providerKey, string propertyKey)
		{
			if (string.IsNullOrWhiteSpace(providerKey))
				return propertyKey;
			return providerKey + "." + propertyKey;
		}

#if NET8_0_OR_GREATER
		//https://aka.ms/dotnet-warnings/SYSLIB0051
		[Obsolete("https://aka.ms/dotnet-warnings/SYSLIB0051", DiagnosticId = "SYSLIB0051")]
#endif
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

		T? IExtendedProperties.GetValue<T>(string propertyKey) where T : default
		{
			return ((IExtendedProperties)this).GetValue<T>(string.Empty, propertyKey);
		}

		T IExtendedProperties.GetValue<T>(string propertyKey, T defaultValue)
		{
			return ((IExtendedProperties)this).GetValue(string.Empty, propertyKey, defaultValue);
		}

		object? IExtendedProperties.GetValue(string propertyKey)
		{
			return ((IExtendedProperties)this).GetValue(string.Empty, propertyKey);
		}

		bool IExtendedProperties.TryGetValue<T>(string propertyKey, out T? value) where T : default
		{
			return ((IExtendedProperties)this).TryGetValue(string.Empty, propertyKey, out value);
		}

		void IExtendedProperties.AddOrUpdate(string propertyKey, object? value)
		{
			((IExtendedProperties)this).AddOrUpdate(string.Empty, propertyKey, value);
		}

		void IExtendedProperties.Add(string propertyKey, object? value)
		{
			((IExtendedProperties)this).Add(string.Empty, propertyKey, value);
		}

		private static T? ConvertType<T>(object? value)
		{
			if (value == null)
				return default;

			if ((value is T returnValue))
				return returnValue;

			var typeConverter = TypeDescriptor.GetConverter(typeof(T?));
			return (T?)typeConverter.ConvertFrom(value);
		}
	}
}