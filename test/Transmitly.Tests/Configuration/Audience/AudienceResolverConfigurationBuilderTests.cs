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

using Moq;

namespace Transmitly.Audience.Configuration.Tests
{
	[TestClass()]
	public class AudienceResolverConfigurationBuilderTests
	{
		[TestMethod()]
		public void AddTest()
		{
			var audienceBuilder = new AudienceResolverConfigurationBuilder(new CommunicationsClientBuilder(), (opt) => { Assert.Fail(); });
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			Assert.ThrowsException<ArgumentException>(() => audienceBuilder.Add(null, (x) => { Assert.Fail(); return Task.FromResult<IAudience?>(null); }));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
		}

		[TestMethod]
		public void AddShouldAddTheResolverWithAddMethod()
		{
			var expectedId = "unit-test";
			void addConfirm(IAudienceResolver provider)
			{
				Assert.IsNotNull(provider);
				Assert.AreEqual(expectedId, provider.AudienceTypeIdentifier);
			}

			var client = new Mock<IAudienceResolver>();
			var builder = new CommunicationsClientBuilder();
			var providerBuilder = new AudienceResolverConfigurationBuilder(builder, addConfirm);
			providerBuilder.Add(expectedId, (id) => throw new NotImplementedException());
		}

		[TestMethod]
		public void AddGenericShouldAddTheResolverWithAddMethod()
		{
			static Task<IAudience?> expectedProvider(object? id) => throw new NotImplementedException();

			static void addConfirm(IAudienceResolver provider)
			{
				Assert.IsNotNull(provider);
				Assert.AreEqual(null, provider.AudienceTypeIdentifier);
				Assert.AreSame(expectedProvider, provider.ResolveAsync);
			}

			var client = new Mock<IAudienceResolver>();
			var builder = new CommunicationsClientBuilder();
			var providerBuilder = new AudienceResolverConfigurationBuilder(builder, addConfirm);
			providerBuilder.AddGeneric(expectedProvider);
		}
	}
}