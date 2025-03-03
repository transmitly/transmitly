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

        /// <summary>
        /// Creates a dynamic model by recursively converting objects/collections.
        /// Special keys (platform identities, resources, etc.) are added, and the "model"
        /// is merged into the bag with recursion.
        /// </summary>
        internal DynamicContentModel(
            object? model,
            IReadOnlyCollection<IPlatformIdentity> platformIdentities,
            IReadOnlyList<Resource>? resources,
            IReadOnlyList<LinkedResource>? linkedResources)
        {
            // Add each platform identity using its Id (if available)
            foreach (var identity in Guard.AgainstNull(platformIdentities))
            {
                if (!string.IsNullOrWhiteSpace(identity.Id))
                    _bag[identity.Id!] = ConvertToDynamic(identity);
            }

            // Add special keys, recursively converting each value
            _bag[PlatformIdentityPropertyKey] = ConvertToDynamic(platformIdentities.ToList());
            _bag[ResourcePropertyKey] = ConvertToDynamic(resources?.Select(s => new { s.Name, s.ContentType }).ToList());
            _bag[LinkedResourcePropertyKey] = ConvertToDynamic(linkedResources?.Select(s => new { s.Name, s.Id, s.ContentType }).ToList());

            if (model == null)
                return;

            // Store the original model (converted recursively) under a special key
            _bag[TransactionPropertyKey] = ConvertToDynamic(model);

            // Merge properties from the model, recursively converting each value.
            if (model is IDictionary<string, object?> expandoObj)
            {
                foreach (var kvp in expandoObj)
                {
                    if (!_bag.ContainsKey(kvp.Key))
                        _bag[kvp.Key] = ConvertToDynamic(kvp.Value);
                }
            }
            else if (model is ICustomTypeDescriptor custom)
            {
                foreach (PropertyDescriptor prop in custom.GetProperties())
                {
                    if (!_bag.ContainsKey(prop.Name))
                        _bag[prop.Name] = ConvertToDynamic(prop.GetValue(model));
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

        // Key validation helper
        private static string ValidateKey(object? key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
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
            if (input is string || type.IsPrimitive || input is decimal || input is DateTime)
                return input;

            // If already a DynamicContentModel, no need to convert further.
            if (input is DynamicContentModel)
                return input;

            // Handle generic dictionaries.
            if (input is IDictionary<string, object?> dict)
            {
                var newDict = new DynamicContentModel(null, new List<IPlatformIdentity>(), null, null);
                foreach (var kvp in dict)
                {
                    newDict._bag[kvp.Key] = ConvertToDynamic(kvp.Value);
                }
                return newDict;
            }

            // Handle non-generic dictionaries.
            if (input is IDictionary nonGen)
            {
                var newDict = new DynamicContentModel(null, new List<IPlatformIdentity>(), null, null);
                foreach (DictionaryEntry entry in nonGen)
                {
                    string key = entry.Key?.ToString() ?? "";
                    newDict._bag[key] = ConvertToDynamic(entry.Value);
                }
                return newDict;
            }

            // Handle enumerables (except string).
            if (input is IEnumerable enumerable && !(input is string))
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

        // Creates a DynamicContentModel from an object's public properties (non-indexed),
        // recursively converting each value.
        private static DynamicContentModel CreateDynamicFromObject(object model)
        {
            var result = new DynamicContentModel(null, new List<IPlatformIdentity>(), null, null);
            foreach (var property in model.GetType().GetProperties()
                         .Where(p => p.GetIndexParameters().Length == 0))
            {
                result._bag[property.Name] = ConvertToDynamic(property.GetValue(model));
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

        public void Add(KeyValuePair<string, object?> item) => _bag.Add(item.Key, item.Value);
        public void ClearItems() => _bag.Clear();
        public bool Contains(KeyValuePair<string, object?> item) => _bag.Contains(item);
        public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) =>
            ((ICollection<KeyValuePair<string, object?>>)_bag).CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<string, object?> item) =>
            ((ICollection<KeyValuePair<string, object?>>)_bag).Remove(item);

        IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator() => _bag.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _bag.GetEnumerator();

        // Internal binder class used for non-dynamic TryGetMember overload.
        private sealed class InternalGetMemberBinder : GetMemberBinder
        {
            public InternalGetMemberBinder(string name, bool ignoreCase) : base(name, ignoreCase) { }
            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject? errorSuggestion) =>
                throw new NotImplementedException();
        }

        // Additional non-dynamic TryGetMember overload.
        public bool TryGetMember(string member, out object? result)
        {
            var binder = new InternalGetMemberBinder(member, true);
            return TryGetMember(binder, out result);
        }
    }
}
