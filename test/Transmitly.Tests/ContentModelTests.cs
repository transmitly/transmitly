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

using System.Dynamic;
using Transmitly.Tests.Mocks;

namespace Transmitly.Tests
{
	[TestClass]
	public class ContentModelTests
	{
		[TestMethod]
		public void ContentModelShouldWrapProvidedModelAndRecipients()
		{
			var expectedId = Guid.NewGuid().ToString("N");
			const string expectedCode = "123456";
			object? x = null;
			ITransactionModel transModel = TransactionModel.Create(new
			{
				OtpCode = expectedCode,
				BrandName = "Unit Test",
				NullValue = x,
				Level0 = new
				{
					Level1 = new
					{
						Level1Value = true
					}
				}
			});

			IReadOnlyCollection<IPlatformIdentityProfile> identities = [new TestPlatformIdentity1(expectedId)];

			ContentModel contentModel = new(transModel, identities);
			dynamic model = contentModel.Model;
			Assert.IsNotNull(contentModel.Model);
			Assert.AreEqual(expectedCode, model.OtpCode);
			Assert.AreEqual(expectedCode, model.trx.OtpCode);
			Assert.IsTrue(model.Level0.Level1.Level1Value);
			Assert.IsTrue(model.trx.Level0.Level1.Level1Value);
			Assert.IsNull(model.NullValue);
			Assert.IsNotNull(model[expectedId]);
			Assert.AreEqual(expectedId, model[expectedId].Id);
			Assert.IsNotNull(model.aud);
			Assert.AreEqual(expectedId, model.aud.Id);
		}

		[TestMethod]
		public void DynamicContentModel_ShouldExcludeIndexProperties()
		{
			dynamic obj = new DynamicContentModel(new ObjWithIndexer(), [], null, null);
			Assert.IsNotNull(obj);
			Assert.AreEqual(1, obj.Id);
			var dictionary = (IDictionary<string, object?>)obj;
			Assert.AreEqual(4, dictionary.Keys.Count);
		}

		[TestMethod]
		public void DynamicContentModel_ShouldHandleExpandoObject()
		{
			const string expectedCode = "123456";
			dynamic contentModel = new ExpandoObject();
			contentModel.Root = new
			{
				OtpCode = expectedCode,
				BrandName = "Unit Test",
				NullValue = (object?)null,
				Level0 = new
				{
					Level1 = new
					{
						Level1Value = true
					}
				}
			};

			dynamic model = new DynamicContentModel((object)contentModel, [], null, null);
			Assert.IsNotNull(model.Root);
			Assert.AreEqual(expectedCode, model.Root.OtpCode);
			Assert.AreEqual(expectedCode, model.trx.Root.OtpCode);
			Assert.IsTrue(model.Root.Level0.Level1.Level1Value);
			Assert.IsTrue(model.trx.Root.Level0.Level1.Level1Value);
			Assert.IsNull(model.Root.NullValue);
		}

		//see hack comment in DynamicContentModel for reasoning
		[TestMethod]
		public void DynamicContentModel_PreventMoreThanOnePlatformIdentity()
		{
			Assert.ThrowsException<NotSupportedException>(() => new DynamicContentModel(new ObjWithIndexer(), [new TestPlatformIdentity1("123"), new TestPlatformIdentity1("234")], null, null));
		}
	}
}