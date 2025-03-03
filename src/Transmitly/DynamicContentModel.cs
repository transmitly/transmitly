using System.Collections;
using System.ComponentModel;
using System.Dynamic;

namespace Transmitly
{
    internal sealed class DynamicContentModel : DynamicObject, IDictionary, IDictionary<string, object?>
    {
        private readonly Dictionary<string, object?> _bag = new Dictionary<string, object?>();

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

        internal DynamicContentModel(
            object? model,
            IReadOnlyCollection<IPlatformIdentity> platformIdentities,
            IReadOnlyList<Resource>? resources,
            IReadOnlyList<LinkedResource>? linkedResources)
        {
            // Add each platform identity with a valid non-empty Id
            foreach (var identity in Guard.AgainstNull(platformIdentities))
            {
                if (!string.IsNullOrWhiteSpace(identity.Id))
                    _bag[identity.Id!] = identity;
            }

            // Add special keys
            _bag[PlatformIdentityPropertyKey] = platformIdentities.Select(x => ToExpandoObject(x)).ToList();
            _bag[ResourcePropertyKey] = resources?.Select(s => ToExpandoObject(new { s.Name, s.ContentType })).ToList();
            _bag[LinkedResourcePropertyKey] = linkedResources?.Select(s => ToExpandoObject(new { s.Name, s.Id, s.ContentType })).ToList();

            if (model != null)
            {
                _bag[TransactionPropertyKey] = model;

                if (model is IDictionary<string, object?> expandoObj)
                {
                    foreach (var kvp in expandoObj)
                    {
                        if (!_bag.ContainsKey(kvp.Key))
                            _bag[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    foreach (var property in model.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0))
                    {
                        if (!_bag.ContainsKey(property.Name))
                            _bag[property.Name] = property.GetValue(model);
                    }
                }
            }
        }

        private static ExpandoObject ToExpandoObject(object obj)
        {
            IDictionary<string, object?> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(obj.GetType()))
            {
                expando.Add(property.Name, property.GetValue(obj));
            }

            return (ExpandoObject)expando;
        }

        private static string ValidateKey(object? key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            string keyStr = key.ToString() ?? throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(keyStr))
                throw new ArgumentException("Key cannot be empty or whitespace.", nameof(key));
            return keyStr;
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

        public bool ContainsKey(string key) => _bag.ContainsKey(key);

        public void Add(string key, object? value) => _bag.Add(key, value);

        public bool Remove(string key) => _bag.Remove(key);

        public bool TryGetValue(string key, out object? value) => _bag.TryGetValue(key, out value);

        public void Add(KeyValuePair<string, object?> item) => _bag.Add(item.Key, item.Value);

        public void ClearItems() => _bag.Clear();

        public bool Contains(KeyValuePair<string, object?> item) => _bag.Contains(item);

        public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) =>
            ((ICollection<KeyValuePair<string, object?>>)_bag).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, object?> item) =>
            ((ICollection<KeyValuePair<string, object?>>)_bag).Remove(item);

        IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator() => _bag.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _bag.GetEnumerator();

        // Internal binder class (kept simple since it's only used for non-dynamic TryGetMember overload)
        private sealed class InternalGetMemberBinder : GetMemberBinder
        {
            public InternalGetMemberBinder(string name, bool ignoreCase) : base(name, ignoreCase) { }
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
