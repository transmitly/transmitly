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

            IReadOnlyCollection<IPlatformIdentity> identities = [new TestPlatformIdentity1(expectedId)];

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
            Assert.IsNotNull(model.aud[0]);
            Assert.AreEqual(expectedId, model.aud[0].Id);
        }

        [TestMethod]
        public void DynamicContentModel_ShouldExcludeIndexProperties()
        {
            dynamic obj = new DynamicContentModel(new ObjWithIndexer(), [], null, null);
            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.Id);
            var dictionary = (IDictionary<string, object?>)obj;
            Assert.AreEqual(5, dictionary.Keys.Count);
        }
    }
}