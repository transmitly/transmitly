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

using System.Runtime.Serialization;

namespace Transmitly
{
	/// <summary>
	/// Represents a collection of keys and values.
	/// </summary>
	public interface IExtendedProperties : ISerializable, IReadOnlyCollection<KeyValuePair<string, object?>>
	{
		/// <summary>
		/// Gets the value associated with the specified keys.
		/// </summary>
		/// <param name="providerKey">The provider key of the value to get or set.</param>
		/// <param name="propertyKey">The key of the value to get or set.</param>
		/// <returns>
		/// The value associated with the specified keys. If the specified key is not found,
		/// a null value is returned
		/// </returns>
		object? this[string providerKey, string propertyKey] { get; }

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="propertyKey">The key of the value to get or set.</param>
		/// <returns>
		/// The value associated with the specified keys. If the specified key is not found,
		/// a null value is returned
		/// </returns>
		object? this[string propertyKey] { get; }

		/// <summary>
		/// Gets the value associated with the specified keys.
		/// </summary>
		/// <typeparam name="T">Expected return type.</typeparam>
		/// <param name="providerKey">The provider key of the value to get or set.</param>
		/// <param name="propertyKey">The key of the value to get or set.</param>
		/// <returns>
		/// The value associated with the specified keys. If the specified key is not found,
		/// a default value is returned
		/// </returns>
		/// <exception cref="InvalidCastException">Stored type is not of type T.</exception>
		T? GetValue<T>(string providerKey, string propertyKey);

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <typeparam name="T">Expected return type.</typeparam>
		/// <param name="propertyKey">The key of the value to get or set.</param>
		/// <returns>
		/// The value associated with the specified keys. If the specified key is not found,
		/// a default value is returned
		/// </returns>
		/// <exception cref="InvalidCastException">Stored type is not of type T.</exception>
		T? GetValue<T>(string propertyKey);

		/// <summary>
		/// Gets the value associated with the specified keys.
		/// </summary>
		/// <typeparam name="T">Expected return type.</typeparam>
		/// <param name="providerKey">The provider key of the value to get or set.</param>
		/// <param name="propertyKey">The key of the value to get or set.</param>
		/// <param name="defaultValue">The value to return if no value associated with keys or default.</param>
		/// <returns>
		/// The value associated with the specified keys. If the specified key is not found,
		/// the default value is returned
		/// </returns>
		/// <exception cref="InvalidCastException">Stored type is not of type T.</exception>
		T GetValue<T>(string providerKey, string propertyKey, T defaultValue);

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <typeparam name="T">Expected return type.</typeparam>
		/// <param name="propertyKey">The key of the value to get or set.</param>
		/// <param name="defaultValue">The value to return if no value associated with keys or default.</param>
		/// <returns>
		/// The value associated with the specified keys. If the specified key is not found,
		/// the default value is returned
		/// </returns>
		/// <exception cref="InvalidCastException">Stored type is not of type T.</exception>
		T GetValue<T>(string propertyKey, T defaultValue);

		/// <summary>
		/// Gets the value associated with the specified keys.
		/// </summary>
		/// <param name="providerKey">The provider key of the value to get or set.</param>
		/// <param name="propertyKey">The key of the value to get or set.</param>
		/// <returns>
		/// The value associated with the specified keys. If the specified key is not found,
		/// a default value is returned
		/// </returns>
		object? GetValue(string providerKey, string propertyKey);

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="propertyKey">The key of the value to get or set.</param>
		/// <returns>
		/// The value associated with the specified keys. If the specified key is not found,
		/// a default value is returned
		/// </returns>
		object? GetValue(string propertyKey);

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <typeparam name="T">Expected result type.</typeparam>
		/// <param name="providerKey">The provider key of the value to get.</param>
		/// <param name="propertyKey">The key of the value to get.</param>
		/// <param name="value">
		///  When this method returns, contains the value associated with the specified key,
		///  if the key is found; otherwise, the default value for the type of the value parameter.
		///  This parameter is passed uninitialized.
		/// </param>
		/// <returns>true if the specified key is found; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">Key is null.</exception>
		/// <exception cref="InvalidCastException">Stored type is not of same type.</exception>
		bool TryGetValue<T>(string providerKey, string propertyKey, out T? value);


		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <typeparam name="T">Expected result type.</typeparam>
		/// <param name="propertyKey">The key of the value to get.</param>
		/// <param name="value">
		///  When this method returns, contains the value associated with the specified key,
		///  if the key is found; otherwise, the default value for the type of the value parameter.
		///  This parameter is passed uninitialized.
		/// </param>
		/// <returns>true if the specified key is found; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">Key is null.</exception>
		/// <exception cref="InvalidCastException">Stored type is not of same type.</exception>
		bool TryGetValue<T>(string propertyKey, out T? value);

		/// <summary>
		/// Add or update existing value with specified key.
		/// </summary>
		/// <param name="providerKey">The provider key of the value to get.</param>
		/// <param name="propertyKey">The key of the value to get.</param>
		/// <param name="value">The value to store.</param>
		void AddOrUpdate(string providerKey, string propertyKey, object? value);

		/// <summary>
		/// Add or update existing value with specified key.
		/// </summary>
		/// <param name="propertyKey">The key of the value to get.</param>
		/// <param name="value">The value to store.</param>
		void AddOrUpdate(string propertyKey, object? value);

		/// <summary>
		/// Add the value with specified key.
		/// </summary>
		/// <param name="providerKey">The provider key of the value to get.</param>
		/// <param name="propertyKey">The key of the value to get.</param>
		/// <param name="value">The value to store.</param>
		/// <exception cref="ArgumentException">A value with the same key already exists.</exception>
		void Add(string providerKey, string propertyKey, object? value);

		/// <summary>
		/// Add the value with specified key.
		/// </summary>
		/// <param name="propertyKey">The key of the value to get.</param>
		/// <param name="value">The value to store.</param>
		/// <exception cref="ArgumentException">A value with the same key already exists.</exception>
		void Add(string propertyKey, object? value);
	}
}