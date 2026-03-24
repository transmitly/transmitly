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

namespace Transmitly;

internal sealed class DynamicContentModel : DynamicObject, IDictionary, IDictionary<string, object?>
{
	private readonly Dictionary<string, object?> _bag = [];
	private readonly HashSet<string> _protectedKeys;
	private readonly bool _isReadOnly;

	private const string TransactionPropertyKey = "trx";
	private const string PlatformIdentityPropertyKey = "pid";
	private const string PlatformIdentityAliasPropertyKey = "to";
	private const string ResourcePropertyKey = "att";
	private const string LinkedResourcePropertyKey = "lnk";

	internal static IReadOnlyCollection<string> ProtectedContentPropertyKeys { get; } =
		new[] { TransactionPropertyKey, PlatformIdentityPropertyKey, PlatformIdentityAliasPropertyKey };

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
		set
		{
			EnsureWritableForKey(key);
			_bag[key] = value;
		}
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
			EnsureWritableForKey(k);
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
		IReadOnlyList<LinkedResource>? linkedResources,
		bool isReadOnly = false,
		IEnumerable<string>? protectedKeys = null)
	{
		_isReadOnly = isReadOnly;
		_protectedKeys = protectedKeys != null
			? new HashSet<string>(protectedKeys, StringComparer.OrdinalIgnoreCase)
			: new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		if (model == null)
			return;

		//hack: as of today, we're going to only allow a single identity be used in a dynamic content model.
		// the changes to the dispatch strategy will ensure we do not send the same communications to multiple recipients
		// the change also means that to access platform identity info we will use pid.PropertyName, instead of the previous pid[0].PropertyName
		// in the future we should be able to do this in a more dynamic way by replacing tokens and managing the index for the developers.
		// for example pid.PropertyName -> pid[index].PropertyName
		if (platformIdentities.Count > 1)
			throw new NotSupportedException("Only single platform identity is supported.");

		foreach (var identity in Guard.AgainstNull(platformIdentities).Where(id => !string.IsNullOrWhiteSpace(id.Id)))
		{
			var dynamicIdentity = ConvertToDynamic(identity, isReadOnly: true);
			_bag.Add(identity.Id!, identity);
			_bag.Add(PlatformIdentityPropertyKey, dynamicIdentity);
			_bag.Add(PlatformIdentityAliasPropertyKey, dynamicIdentity);
		}
		//hack end
		_bag[ResourcePropertyKey] = ConvertToDynamic(resources?.Select(s => new { s.Name, s.ContentType }).ToList());
		_bag[LinkedResourcePropertyKey] = ConvertToDynamic(linkedResources?.Select(s => new { s.Name, s.Id, s.ContentType }).ToList());
		_bag[TransactionPropertyKey] = ConvertToDynamic(model, isReadOnly: true);

		if (model is IDictionary<string, object?> expandoObj)
		{
			foreach (var kvp in expandoObj.Where(kvp => !_bag.ContainsKey(kvp.Key)))
			{
				_bag[kvp.Key] = ConvertToDynamic(kvp.Value);
			}
		}
		else
		{
			foreach (var property in model.GetType().GetProperties()
						 .Where(p => p.GetIndexParameters().Length == 0 && !_bag.ContainsKey(p.Name)))
			{
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
	private static object? ConvertToDynamic(object? input, bool isReadOnly = false)
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
			var newDict = new DynamicContentModel(null, new List<IPlatformIdentityProfile>(), null, null, isReadOnly: isReadOnly);
			foreach (var kvp in dict)
			{
				newDict._bag[kvp.Key] = ConvertToDynamic(kvp.Value, isReadOnly);
			}
			return newDict;
		}

		// Handle non-generic dictionaries.
		if (input is IDictionary nonGen)
		{
			var newDict = new DynamicContentModel(null, new List<IPlatformIdentityProfile>(), null, null, isReadOnly: isReadOnly);
			foreach (DictionaryEntry entry in nonGen)
			{
				string key = entry.Key?.ToString() ?? "";
				newDict._bag[key] = ConvertToDynamic(entry.Value, isReadOnly);
			}
			return newDict;
		}

		// Handle enumerables (except string).
		if (input is IEnumerable enumerable && input is not string)
		{
			var list = new List<object?>();
			foreach (var item in enumerable)
			{
				list.Add(ConvertToDynamic(item, isReadOnly));
			}
			return list;
		}

		// Otherwise, assume it's a custom object and create a dynamic wrapper.
		return CreateDynamicFromObject(input, isReadOnly);
	}

	private static DynamicContentModel CreateDynamicFromObject(object model, bool isReadOnly)
	{
		var result = new DynamicContentModel(null, new List<IPlatformIdentityProfile>(), null, null, isReadOnly: isReadOnly);
		foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(model))
		{
			if (!result._bag.ContainsKey(prop.Name))
			{
				result._bag[prop.Name] = ConvertToDynamic(prop.GetValue(model), isReadOnly);
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
		EnsureWritableForKey(binder.Name);
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

	public void Add(object key, object? value)
	{
		var k = ValidateKey(key);
		EnsureWritableForKey(k);
		_bag.Add(k, value);
	}

	public void Clear()
	{
		EnsureWritable();

		if (_protectedKeys.Any(_bag.ContainsKey))
			throw CreateProtectedPropertyException("clear");

		_bag.Clear();
	}

	public bool Contains(object key) => _bag.ContainsKey(ValidateKey(key));
	IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)_bag).GetEnumerator();
	public void Remove(object key)
	{
		var k = ValidateKey(key);
		EnsureWritableForKey(k);
		_bag.Remove(k);
	}

	public void CopyTo(Array array, int index) => ((ICollection)_bag).CopyTo(array, index);

	// IDictionary<string, object?> implementation
	public bool ContainsKey(string key) => _bag.ContainsKey(key);
	public void Add(string key, object? value)
	{
		EnsureWritableForKey(key);
		_bag.Add(key, value);
	}

	public bool Remove(string key)
	{
		EnsureWritableForKey(key);
		return _bag.Remove(key);
	}

	public bool TryGetValue(string key, out object? value) => _bag.TryGetValue(key, out value);

	// ICollection<KeyValuePair<string, object?>> implementation
	public void Add(KeyValuePair<string, object?> item)
	{
		EnsureWritableForKey(item.Key);
		_bag.Add(item.Key, item.Value);
	}

	public void ClearItems() => Clear();

	public bool Contains(KeyValuePair<string, object?> item) => _bag.Contains(item);
	public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) =>
		((ICollection<KeyValuePair<string, object?>>)_bag).CopyTo(array, arrayIndex);

	public bool Remove(KeyValuePair<string, object?> item)
	{
		EnsureWritableForKey(item.Key);
		return ((ICollection<KeyValuePair<string, object?>>)_bag).Remove(item);
	}

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

	internal void CopyProtectedPropertiesFrom(DynamicContentModel source)
	{
		Guard.AgainstNull(source);

		foreach (var key in _protectedKeys)
		{
			if (source._bag.TryGetValue(key, out var value))
			{
				_bag[key] = value;
			}
		}
	}

	private void EnsureWritable()
	{
		if (_isReadOnly)
			throw new InvalidOperationException("This content model section is read-only.");
	}

	private void EnsureWritableForKey(string key)
	{
		EnsureWritable();
		if (_protectedKeys.Contains(key))
			throw CreateProtectedPropertyException(key);
	}

	private static InvalidOperationException CreateProtectedPropertyException(string key) =>
		new($"The content model property '{key}' is protected and cannot be modified by content model enrichers.");
}
