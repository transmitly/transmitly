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

namespace Transmitly.Tests;

[TestClass()]
public class ExtendedPropertiesTests
{
	[TestMethod()]
	public void GetValueTShouldReturnValuesAsExpected()
	{
		IExtendedProperties props = new ExtendedProperties();
		props.Add("unit", "test", true);
		Assert.IsTrue(props.GetValue<bool>("unit", "test"));
		Assert.IsNull(props.GetValue<bool?>("fake", "test"));
		Assert.IsFalse(props.GetValue<bool>("fake", "test"));
	}

	[TestMethod()]
	public void GetValueShouldReturnValuesAsExpected()
	{
		IExtendedProperties props = new ExtendedProperties();
		props.Add("unit", "test", true);
		Assert.IsTrue((bool?)props.GetValue("unit", "test"));
		Assert.IsNull(props.GetValue("fake", "test"));
	}

	[TestMethod()]
	public void GetValueTShouldReturnDefaultValuesAsExpected()
	{
		IExtendedProperties props = new ExtendedProperties();
		props.Add("unit", "test", true);
		Assert.IsTrue(props.GetValue("unit", "test", false));
		Assert.IsTrue(props.GetValue("fake", "test", true));
		Assert.IsNull(props.GetValue<bool?>("fake", "test", null));
		Assert.IsFalse(props.GetValue<bool?>("fake", "test", false));
		Assert.IsTrue(props.GetValue<bool?>("fake", "test", true));
	}

	[TestMethod()]
	public void TryGetValueTShouldReturnDefaultValuesAsExpected()
	{
		IExtendedProperties props = new ExtendedProperties();
		props.Add("unit", "test", true);

		Assert.IsTrue(props.TryGetValue<bool?>("unit", "test", out var result1));
		Assert.IsTrue(result1);

		Assert.IsFalse(props.TryGetValue<bool?>("fake", "test", out var result2));
		Assert.IsNull(result2);

		Assert.IsFalse(props.TryGetValue<bool>("fake", "test", out var result3));
		Assert.IsFalse(result3);
	}

	[TestMethod()]
	public void AddOrUpdateShouldAddOrUpdate()
	{
		IExtendedProperties props = new ExtendedProperties();
		props.AddOrUpdate("unit", "test", 1);

		Assert.AreEqual(1, props.GetValue("unit", "test"));

		props.AddOrUpdate("unit", "test", 2);

		Assert.AreEqual(2, props.GetValue("unit", "test"));
	}

	[TestMethod()]
	public void GetValueShouldConvertToExpectedType()
	{
		var expected = Guid.NewGuid();
		IExtendedProperties props = new ExtendedProperties();
		props.AddOrUpdate("unit", "test", expected.ToString("N"));

		Assert.AreEqual(expected, props.GetValue<Guid>("unit", "test"));
		Assert.AreEqual(expected, props.GetValue<Guid?>("unit", "test"));

		props.AddOrUpdate("unit", "test", expected);

		Assert.AreEqual(expected, props.GetValue<Guid>("unit", "test"));
		Assert.AreEqual(expected, props.GetValue<Guid?>("unit", "test"));

		var expected2 = 1234;
		props.AddOrUpdate("unit", "test", expected2);

		Assert.AreEqual(expected2, props.GetValue<int>("unit", "test"));
		Assert.AreEqual(expected2, props.GetValue<int?>("unit", "test"));

		props.AddOrUpdate("unit", "test", expected2);

		Assert.AreEqual(expected2, props.GetValue<int>("unit", "test"));
		Assert.AreEqual(expected2, props.GetValue<int?>("unit", "test"));

		props.AddOrUpdate("unit", "test", null);

		Assert.AreEqual(0, props.GetValue<int>("unit", "test"));
		Assert.IsNull(props.GetValue<int?>("unit", "test"));
	}
}