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
using System.ComponentModel;
using System.Dynamic;

namespace Transmitly.Delivery
{
	sealed class WrappedPlatformIdentity(ExpandoObject expando) : DynamicObject, IPlatformIdentity, ICustomTypeDescriptor
	{
		private readonly ExpandoObject _expando = Guard.AgainstNull(expando);

		public string? Id => GetValue(nameof(IPlatformIdentity.Id)) as string;
		public string? Type => GetValue(nameof(IPlatformIdentity.Type)) as string;
		public IReadOnlyCollection<IIdentityAddress> Addresses =>
			GetValue(nameof(IPlatformIdentity.Addresses)) as IReadOnlyCollection<IIdentityAddress> ?? Array.Empty<IIdentityAddress>();
		public IReadOnlyCollection<string> ChannelPreferences =>
			GetValue(nameof(IPlatformIdentity.ChannelPreferences)) as IReadOnlyCollection<string> ?? Array.Empty<string>();

		private object? GetValue(string propertyName)
		{
			var dict = (IDictionary<string, object>)_expando!;
			dict.TryGetValue(propertyName, out var value);
			return value;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object? result)
		{
			var dict = (IDictionary<string, object>)_expando!;
			return dict.TryGetValue(binder.Name, out result);
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			var dict = (IDictionary<string, object>)_expando!;
			return dict.Keys;
		}


		public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(_expando);
		public string GetClassName() => TypeDescriptor.GetClassName(_expando)!;
		public string GetComponentName() => TypeDescriptor.GetComponentName(_expando)!;
		public TypeConverter GetConverter() => TypeDescriptor.GetConverter(_expando);
		public EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(_expando)!;
		public PropertyDescriptor GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(_expando)!;
		public object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(_expando, editorBaseType)!;
		public EventDescriptorCollection GetEvents(Attribute[]? attributes) => TypeDescriptor.GetEvents(_expando, attributes ?? []);
		public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(_expando);
		public PropertyDescriptorCollection GetProperties(Attribute[]? attributes) => TypeDescriptor.GetProperties(_expando, attributes);
		public PropertyDescriptorCollection GetProperties() => TypeDescriptor.GetProperties(_expando);
		public object GetPropertyOwner(PropertyDescriptor? pd) => _expando;

	}
}
