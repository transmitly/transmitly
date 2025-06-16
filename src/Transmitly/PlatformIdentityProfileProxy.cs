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

namespace Transmitly;

sealed class PlatformIdentityProfileProxy : DynamicObject, IPlatformIdentityProfile, ICustomTypeDescriptor
{
	private readonly IPlatformIdentityProfile _source;
	private readonly IReadOnlyCollection<IPlatformIdentityAddress> _addresses;
	private readonly Type _sourceType;
	private readonly PropertyDescriptorCollection _proxyProperties;

	internal IPlatformIdentityProfile Source => _source;

	public PlatformIdentityProfileProxy(IPlatformIdentityProfile source, IReadOnlyCollection<IPlatformIdentityAddress>? overrideAddresses = null)
	{
		_source = Guard.AgainstNull(source);
		_addresses = overrideAddresses ?? source.Addresses;
		_sourceType = Guard.AgainstNull(source.GetType());

		var originalProps = TypeDescriptor.GetProperties(source);
		var props = new List<PropertyDescriptor>();

		// Wrap every property descriptor so that the ComponentType is the type of our proxy.
		foreach (PropertyDescriptor pd in originalProps)
		{
			props.Add(new ProxyPropertyDescriptor(pd, this));
		}
		_proxyProperties = new PropertyDescriptorCollection([.. props], true);
	}

	string? IPlatformIdentityProfile.Id => _source.Id;

	string? IPlatformIdentityProfile.Type => _source.Type;

	IReadOnlyCollection<IPlatformIdentityAddress> IPlatformIdentityProfile.Addresses => _addresses;

	IReadOnlyCollection<IPlatformIdentityChannelPreferenceSet>? IPlatformIdentityProfile.ChannelPreferences => _source.ChannelPreferences;

	AttributeCollection ICustomTypeDescriptor.GetAttributes() =>
		TypeDescriptor.GetAttributes(_sourceType);

	string ICustomTypeDescriptor.GetClassName() =>
		TypeDescriptor.GetClassName(_sourceType)!;

	string ICustomTypeDescriptor.GetComponentName() =>
		TypeDescriptor.GetComponentName(_sourceType)!;

	TypeConverter ICustomTypeDescriptor.GetConverter() =>
		TypeDescriptor.GetConverter(_sourceType);

	EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() =>
		TypeDescriptor.GetDefaultEvent(_sourceType)!;

	PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() =>
		TypeDescriptor.GetDefaultProperty(_sourceType)!;

	object ICustomTypeDescriptor.GetEditor(Type editorBaseType) =>
		TypeDescriptor.GetEditor(_sourceType, editorBaseType)!;

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents() =>
		TypeDescriptor.GetEvents(_sourceType);

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[]? attributes) =>
		TypeDescriptor.GetEvents(_sourceType, Guard.AgainstNull(attributes));

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() =>
		_proxyProperties;

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[]? attributes) =>
		_proxyProperties;

	object? ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor? pd)
	{
		return this;
	}

}

sealed class ProxyPropertyDescriptor(PropertyDescriptor basePropertyDescriptor, PlatformIdentityProfileProxy proxy) : PropertyDescriptor(basePropertyDescriptor)
{
	public override Type ComponentType => proxy.GetType();

	public override bool IsReadOnly => basePropertyDescriptor.IsReadOnly;

	public override Type PropertyType => basePropertyDescriptor.PropertyType;

	public override bool CanResetValue(object component) =>
		basePropertyDescriptor.CanResetValue(proxy.Source);

	public override object? GetValue(object? component)
	{
		return basePropertyDescriptor.GetValue(proxy.Source);
	}

	public override void ResetValue(object component) =>
		basePropertyDescriptor.ResetValue(proxy.Source);

	public override void SetValue(object? component, object? value) =>
		basePropertyDescriptor.SetValue(proxy.Source, value);

	public override bool ShouldSerializeValue(object component) =>
		basePropertyDescriptor.ShouldSerializeValue(proxy.Source);
}
