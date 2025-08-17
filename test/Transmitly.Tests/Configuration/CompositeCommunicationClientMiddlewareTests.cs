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
using Transmitly;
using Transmitly.Exceptions;

[TestClass]
public class CompositeCommunicationClientMiddlewareTests
{
	[TestMethod]
	public void ConstructorDefaultCreatesEmptyMiddlewareList()
	{
		var composite = new CompositeCommunicationClientMiddleware();

		Assert.ThrowsException<CommunicationsException>(() =>
			composite.CreateClient(Mock.Of<ICreateCommunicationsClientContext>(), null));
	}

	[TestMethod]
	public void ConstructorWithDefaultMiddlewareInitializesWithMiddleware()
	{

		var mockMiddleware = new Mock<ICommunicationClientMiddleware>();
		var mockClient = new Mock<ICommunicationsClient>();

		mockMiddleware.Setup(m => m.CreateClient(It.IsAny<ICreateCommunicationsClientContext>(), It.IsAny<ICommunicationsClient>()))
			.Returns(mockClient.Object);


		var composite = new CompositeCommunicationClientMiddleware(mockMiddleware.Object);
		var result = composite.CreateClient(Mock.Of<ICreateCommunicationsClientContext>(), null);


		Assert.IsNotNull(result);
		Assert.AreEqual(mockClient.Object, result);
	}

	[TestMethod]
	public void AddFactoryWithoutIndexAddsToEnd()
	{

		var composite = new CompositeCommunicationClientMiddleware();
		var mockMiddleware1 = new Mock<ICommunicationClientMiddleware>();
		var mockMiddleware2 = new Mock<ICommunicationClientMiddleware>();
		var mockClient = new Mock<ICommunicationsClient>();

		mockMiddleware2.Setup(m => m.CreateClient(It.IsAny<ICreateCommunicationsClientContext>(), It.IsAny<ICommunicationsClient>()))
			.Returns(mockClient.Object);


		composite.AddFactory(mockMiddleware1.Object, null);
		composite.AddFactory(mockMiddleware2.Object, null);
		var result = composite.CreateClient(Mock.Of<ICreateCommunicationsClientContext>(), null);


		Assert.IsNotNull(result);
		mockMiddleware1.Verify(m => m.CreateClient(It.IsAny<ICreateCommunicationsClientContext>(), null), Times.Once);
		mockMiddleware2.Verify(m => m.CreateClient(It.IsAny<ICreateCommunicationsClientContext>(), It.IsAny<ICommunicationsClient>()), Times.Once);
	}

	[TestMethod]
	public void AddFactoryWithValidIndexInsertsAtPosition()
	{

		var composite = new CompositeCommunicationClientMiddleware();
		var mockMiddleware1 = new Mock<ICommunicationClientMiddleware>();
		var mockMiddleware2 = new Mock<ICommunicationClientMiddleware>();
		var mockClient = new Mock<ICommunicationsClient>();

		mockMiddleware2.Setup(m => m.CreateClient(It.IsAny<ICreateCommunicationsClientContext>(), It.IsAny<ICommunicationsClient>()))
			.Returns(mockClient.Object);
		mockMiddleware1.Setup(m => m.CreateClient(It.IsAny<ICreateCommunicationsClientContext>(), It.IsAny<ICommunicationsClient>()))
			.Returns((ICreateCommunicationsClientContext ctx, ICommunicationsClient client) => client);


		composite.AddFactory(mockMiddleware1.Object, null);
		composite.AddFactory(mockMiddleware2.Object, 0); // Insert at beginning
		var result = composite.CreateClient(Mock.Of<ICreateCommunicationsClientContext>(), null);


		Assert.IsNotNull(result);
		mockMiddleware2.Verify(m => m.CreateClient(It.IsAny<ICreateCommunicationsClientContext>(), null), Times.Once);
		mockMiddleware1.Verify(m => m.CreateClient(It.IsAny<ICreateCommunicationsClientContext>(), It.IsAny<ICommunicationsClient>()), Times.Once);
	}

	[TestMethod]
	public void AddFactoryWithInvalidIndexThrowsArgumentOutOfRangeException()
	{
		var composite = new CompositeCommunicationClientMiddleware();
		var mockMiddleware = new Mock<ICommunicationClientMiddleware>();

		Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
			composite.AddFactory(mockMiddleware.Object, -1));
		Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
			composite.AddFactory(mockMiddleware.Object, 1));
	}

	[TestMethod]
	public void CreateClientWithMultipleMiddlewaresChainsProperly()
	{
		var composite = new CompositeCommunicationClientMiddleware();
		var mockMiddleware1 = new Mock<ICommunicationClientMiddleware>();
		var mockMiddleware2 = new Mock<ICommunicationClientMiddleware>();
		var mockClient1 = new Mock<ICommunicationsClient>();
		var mockClient2 = new Mock<ICommunicationsClient>();

		mockMiddleware1.Setup(m => m.CreateClient(It.IsAny<ICreateCommunicationsClientContext>(), It.IsAny<ICommunicationsClient>()))
			.Returns(mockClient1.Object);
		mockMiddleware2.Setup(m => m.CreateClient(It.IsAny<ICreateCommunicationsClientContext>(), It.IsAny<ICommunicationsClient>()))
			.Returns(mockClient2.Object);

		composite.AddFactory(mockMiddleware1.Object, null);
		composite.AddFactory(mockMiddleware2.Object, null);


		var result = composite.CreateClient(Mock.Of<ICreateCommunicationsClientContext>(), null);


		Assert.IsNotNull(result);
		Assert.AreEqual(mockClient2.Object, result);
		mockMiddleware1.Verify(m => m.CreateClient(It.IsAny<ICreateCommunicationsClientContext>(), null), Times.Once);
		mockMiddleware2.Verify(m => m.CreateClient(It.IsAny<ICreateCommunicationsClientContext>(), mockClient1.Object), Times.Once);
	}

	[TestMethod]
	public void CreateClientWithNoClientCreatedThrowsInvalidOperationException()
	{
		var composite = new CompositeCommunicationClientMiddleware();
		var mockMiddleware = new Mock<ICommunicationClientMiddleware>();
		mockMiddleware.Setup(m => m.CreateClient(It.IsAny<ICreateCommunicationsClientContext>(), It.IsAny<ICommunicationsClient>()))
			.Returns((ICommunicationsClient)null!);

		composite.AddFactory(mockMiddleware.Object, null);

		Assert.ThrowsException<CommunicationsException>(() =>
			composite.CreateClient(Mock.Of<ICreateCommunicationsClientContext>(), null));
	}
}