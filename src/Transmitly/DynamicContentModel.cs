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

        bool IDictionary.IsFixedSize => true;

        bool IDictionary.IsReadOnly => true;

        ICollection IDictionary.Keys => _bag.Keys;

        ICollection IDictionary.Values => _bag.Values;

        int ICollection.Count => _bag.Count;

        bool ICollection.IsSynchronized => ((ICollection)_bag).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)_bag).SyncRoot;

        ICollection<string> IDictionary<string, object?>.Keys => _bag.Keys;

        ICollection<object?> IDictionary<string, object?>.Values => _bag.Values;

        int ICollection<KeyValuePair<string, object?>>.Count => _bag.Count;

        bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => ((ICollection<KeyValuePair<string, object?>>)_bag).IsReadOnly;

        object? IDictionary<string, object?>.this[string key] { get => _bag[key]; set => _bag[key] = value; }

        object? IDictionary.this[object key] { get => _bag[Guard.AgainstNullOrWhiteSpace(key.ToString())] ?? throw new ArgumentNullException(nameof(key)); set => _bag[Guard.AgainstNullOrWhiteSpace(key.ToString())] = value; }

        internal DynamicContentModel(object? model, IReadOnlyCollection<IPlatformIdentity> platformIdentities, IReadOnlyList<Resource>? resources, IReadOnlyList<LinkedResource>? linkedResources)
        {
            foreach (var identity in Guard.AgainstNull(platformIdentities))
            {
                if (!string.IsNullOrWhiteSpace(identity.Id))
                    _bag.Add(identity.Id!, identity);
            }

            _bag.Add(PlatformIdentityPropertyKey, platformIdentities.ToList());
            _bag.Add(ResourcePropertyKey, resources?.Select(s => new { s.Name, s.ContentType }).ToList());
            _bag.Add(LinkedResourcePropertyKey, linkedResources?.Select(s => new { s.Name, s.Id, s.ContentType }).ToList());

            if (model == null)
                return;

            _bag.Add(TransactionPropertyKey, model);

            foreach (var property in model.GetType().GetProperties().Where(p => p?.GetIndexParameters()?.Length == 0))
            {
                if (!_bag.ContainsKey(property.Name))
                    _bag.Add(property.Name, property.GetValue(model));
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            if (_bag.TryGetValue(binder.Name, out var obj))
            {
                result = obj;
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public bool TryGetMember(string member, out object? result)
        {
            var binder = new InternalGetMemberBinder(member, true);
            if (TryGetMember(binder, out var obj))
            {
                result = obj;
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _bag.Keys;
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            _bag.Add(binder.Name, value);
            return base.TrySetMember(binder, value);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (TryGetMember(new InternalGetMemberBinder((string)indexes[0], false), out var obj))
            {
                result = obj!;
                return true;
            }
            return base.TryGetIndex(binder, indexes, out result!);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_bag).GetEnumerator();
        }

        void IDictionary.Add(object key, object? value)
        {
            _bag.Add(Guard.AgainstNullOrWhiteSpace(key?.ToString()), value);
        }

        void IDictionary.Clear()
        {
            _bag.Clear();
        }

        bool IDictionary.Contains(object key)
        {
            return ((IDictionary)_bag).Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)_bag).GetEnumerator();
        }

        void IDictionary.Remove(object key)
        {
            Guard.AgainstNull(key);
            _bag.Remove(Guard.AgainstNullOrWhiteSpace(key.ToString()));
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_bag).CopyTo(array, index);
        }

        bool IDictionary<string, object?>.ContainsKey(string key)
        {
            return _bag.ContainsKey(key);
        }

        void IDictionary<string, object?>.Add(string key, object? value)
        {
            _bag.Add(key, value);
        }

        bool IDictionary<string, object?>.Remove(string key)
        {
            return _bag.Remove(key);
        }

        bool IDictionary<string, object?>.TryGetValue(string key, out object? value)
        {
            return _bag.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<string, object?>>.Add(KeyValuePair<string, object?> item)
        {
            _bag.Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object?>>.Clear()
        {
            _bag.Clear();
        }

        bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item)
        {
            return _bag.Contains(item);
        }

        void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object?>>)_bag).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item)
        {
            return ((ICollection<KeyValuePair<string, object?>>)_bag).Remove(item);
        }

        IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator()
        {
            return _bag.GetEnumerator();
        }

        class InternalGetMemberBinder(string name, bool ignoreCase) : GetMemberBinder(name, ignoreCase)
        {
            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject? errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }
    }
}