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
using System.Dynamic;

namespace Transmitly
{
	internal sealed class DynamicContentModel : DynamicObject, IDictionary, IDictionary<string, object?>
	{
		private readonly Dictionary<string, object?> _bag = [];

		private const string TransactionPropertyKey = "trx";
		private const string PlatformIdentityPropertyKey = "aud";
		private const string ResourcePropertyKey = "att";
		private const string LinkedResourcePropertyKey = "lnk";

		bool IDictionary.IsFixedSize => false;
		bool IDictionary.IsReadOnly => false;
		ICollection IDictionary.Keys => _bag.Keys;
		ICollection IDictionary.Values => _bag.Values;
		int ICollection.Count => _bag.Count;
		bool ICollection.IsSynchronized => ((ICollection)_bag).IsSynchronized;
		object ICollection.SyncRoot => ((ICollection)_bag).SyncRoot;
		ICollection<string> IDictionary<string, object?>.Keys => _bag.Keys;
		ICollection<object?> IDictionary<string, object?>.Values => _bag.Values;
		int ICollection<KeyValuePair<string, object?>>.Count => _bag.Count;
		bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => false;

		object? IDictionary<string, object?>.this[string key]
		{
			get => _bag[key];
			set => _bag[key] = value;
		}

		object? IDictionary.this[object key]
		{
			get
			{
				var k = ValidateKey(key);
				return _bag[k];
			}
			set
			{
				var k = ValidateKey(key);
				_bag[k] = value;
			}
		}

		/// <summary>
		/// Creates a dynamic model by recursively converting objects/collections.
		/// Special keys (platform identities, resources, etc.) are added, and the "model"
		/// is merged into the bag with recursion.
		/// </summary>
		internal DynamicContentModel(
			object? model,
			IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities,
			IReadOnlyList<Resource>? resources,
			IReadOnlyList<LinkedResource>? linkedResources)
		{
			if (model == null)
				return;

			//hack: as of today, we're going to only allow a single identity be used in a dynamic content model.
			// the changes to the dispatch strategy will ensure we do not send the same communications to multiple recipients
			// the change also means that to access audience info we will use aud.PropertyName, instead of the previous aud[0].PropertyName
			// in the future we should be able to do this in a more dynamic way by replacing tokens and managing the index for the developers.
			// for example aud.PropertyName -> aud[index].PropertyName
			if (platformIdentities.Count > 1)
				throw new NotSupportedException("Only single platform identity is supported.");

			foreach (var identity in Guard.AgainstNull(platformIdentities))
			{
				if (!string.IsNullOrWhiteSpace(identity.Id))
				{
					_bag.Add(identity.Id!, identity);
					_bag.Add(PlatformIdentityPropertyKey, ConvertToDynamic(identity));
				}
			}
			//hack end
			_bag[ResourcePropertyKey] = ConvertToDynamic(resources?.Select(s => new { s.Name, s.ContentType }).ToList());
			_bag[LinkedResourcePropertyKey] = ConvertToDynamic(linkedResources?.Select(s => new { s.Name, s.Id, s.ContentType }).ToList());
			_bag[TransactionPropertyKey] = ConvertToDynamic(model);

			if (model is IDictionary<string, object?> expandoObj)
			{
				foreach (var kvp in expandoObj)
				{
					if (!_bag.ContainsKey(kvp.Key))
						_bag[kvp.Key] = ConvertToDynamic(kvp.Value);
				}
			}
			else
			{
				foreach (var property in model.GetType().GetProperties()
							 .Where(p => p.GetIndexParameters().Length == 0))
				{
					if (!_bag.ContainsKey(property.Name))
						_bag[property.Name] = ConvertToDynamic(property.GetValue(model));
				}
			}
		}

		private static string ValidateKey(object? key)
		{
			if (key == null)
				Guard.AgainstNull(key);
			string keyStr = key.ToString() ?? throw new ArgumentNullException(nameof(key));
			if (string.IsNullOrWhiteSpace(keyStr))
				throw new ArgumentException("Key cannot be empty or whitespace.", nameof(key));
			return keyStr;
		}

		// Recursively converts any object to a dynamic-friendly format.
		// Primitives and strings are returned as-is. Dictionaries and enumerables
		// are recursively processed, and other objects are wrapped in a DynamicContentModel.
		private static object? ConvertToDynamic(object? input)
		{
			if (input == null)
				return null;

			// Return primitive types, strings, decimals, DateTimes as is.
			var type = input.GetType();
			if (input is string || type.IsPrimitive || input is decimal || input is DateTime || input is Guid)
				return input;

			// If already a DynamicContentModel, no need to convert further.
			if (input is DynamicContentModel)
				return input;

			// Handle generic dictionaries.
			if (input is IDictionary<string, object?> dict)
			{
				var newDict = new DynamicContentModel(null, new List<IPlatformIdentityProfile>(), null, null);
				foreach (var kvp in dict)
				{
					newDict._bag[kvp.Key] = ConvertToDynamic(kvp.Value);
				}
				return newDict;
			}

			// Handle non-generic dictionaries.
			if (input is IDictionary nonGen)
			{
				var newDict = new DynamicContentModel(null, new List<IPlatformIdentityProfile>(), null, null);
				foreach (DictionaryEntry entry in nonGen)
				{
					string key = entry.Key?.ToString() ?? "";
					newDict._bag[key] = ConvertToDynamic(entry.Value);
				}
				return newDict;
			}

			// Handle enumerables (except string).
			if (input is IEnumerable enumerable && input is not string)
			{
				var list = new List<object?>();
				foreach (var item in enumerable)
				{
					list.Add(ConvertToDynamic(item));
				}
				return list;
			}

			// Otherwise, assume it's a custom object and create a dynamic wrapper.
			return CreateDynamicFromObject(input);
		}

		private static DynamicContentModel CreateDynamicFromObject(object model)
		{
			var result = new DynamicContentModel(null, new List<IPlatformIdentityProfile>(), null, null);
			foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(model))
			{
				if (!result._bag.ContainsKey(prop.Name))
				{
					result._bag[prop.Name] = ConvertToDynamic(prop.GetValue(model));
				}
			}
			return result;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object? result)
		{
			if (_bag.TryGetValue(binder.Name, out var value))
			{
				result = value;
				return true;
			}
			result = null;
			return false;
		}

		public override bool TrySetMember(SetMemberBinder binder, object? value)
		{
			_bag[binder.Name] = value;
			return true;
		}

		public override IEnumerable<string> GetDynamicMemberNames() => _bag.Keys;

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
		{
			if (indexes.Length == 1 && indexes[0] is string key && _bag.TryGetValue(key, out result))
			{
				return true;
			}
			result = null;
			return false;
		}

		public void Add(object key, object? value) => _bag.Add(ValidateKey(key), value);
		public void Clear() => _bag.Clear();
		public bool Contains(object key) => _bag.ContainsKey(ValidateKey(key));
		IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)_bag).GetEnumerator();
		public void Remove(object key) => _bag.Remove(ValidateKey(key));
		public void CopyTo(Array array, int index) => ((ICollection)_bag).CopyTo(array, index);

		// IDictionary<string, object?> implementation
		public bool ContainsKey(string key) => _bag.ContainsKey(key);
		public void Add(string key, object? value) => _bag.Add(key, value);
		public bool Remove(string key) => _bag.Remove(key);
		public bool TryGetValue(string key, out object? value) => _bag.TryGetValue(key, out value);

		// ICollection<KeyValuePair<string, object?>> implementation
		public void Add(KeyValuePair<string, object?> item) => _bag.Add(item.Key, item.Value);
		public void ClearItems() => _bag.Clear();
		public bool Contains(KeyValuePair<string, object?> item) => _bag.Contains(item);
		public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) =>
			((ICollection<KeyValuePair<string, object?>>)_bag).CopyTo(array, arrayIndex);
		public bool Remove(KeyValuePair<string, object?> item) =>
			((ICollection<KeyValuePair<string, object?>>)_bag).Remove(item);

		IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator() => _bag.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _bag.GetEnumerator();

		private sealed class InternalGetMemberBinder(string name, bool ignoreCase) : GetMemberBinder(name, ignoreCase)
		{
			public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject? errorSuggestion) =>
				throw new NotImplementedException();
		}

		public bool TryGetMember(string member, out object? result)
		{
			var binder = new InternalGetMemberBinder(member, true);
			return TryGetMember(binder, out result);
		}
	}
}
