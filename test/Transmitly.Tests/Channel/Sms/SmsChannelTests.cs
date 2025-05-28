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

using AutoFixture;
using Moq;
using Transmitly.Channel.Configuration.Sms;
using Transmitly.Exceptions;
using Transmitly.Tests;

namespace Transmitly.Channel.Sms.Tests;

[TestClass()]
public class SmsChannelTests : BaseUnitTest
{

	[TestMethod()]
	[DataRow("+14155552671", true)]
	[DataRow("+442071838750", true)]
	[DataRow("+551155256325", true)]
	[DataRow("551155256325", true)]
	[DataRow("51155256325", true)]
	[DataRow("(511)55256325", false)]
	[DataRow("511-552-56325", false)]
	[DataRow("+1 511-552-56325", false)]
	[DataRow("+1 51155256325", false)]
	[DataRow("+37060112345", true)]
	[DataRow("+64010", true)]//NZ service number
	[DataRow("+1231234567890", true)]
	[DataRow("+2902124", true)]
	[DataRow("15551231234", true)]
	public void SupportsIdentityAddressTest(string value, bool expected)
	{
		var sms = new SmsChannel(new SmsChannelConfiguration());
		var result = sms.SupportsIdentityAddress(value.AsIdentityAddress());
		Assert.AreEqual(expected, result, value);
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldGuardAgainstNullContext()
	{
		var channel = fixture.Create<SmsChannel>();

		await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => channel.GenerateCommunicationAsync(null!));
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldGenerateValidSmsCommunication()
	{
		var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var context = mockContext.Object;
		var from = "8888".AsIdentityAddress();
		var config = new SmsChannelConfiguration(_ => from);
		var body = fixture.Freeze<string>();
		config.Message.AddStringTemplate(body);

		var sut = new SmsChannel(config);


		var result = await sut.GenerateCommunicationAsync(context);

		Assert.IsInstanceOfType(result, typeof(ISms));

		Assert.AreEqual(from, result.From);
		Assert.AreEqual(body, result.Message);
		Assert.AreEqual(context.TransportPriority, result.TransportPriority);
		CollectionAssert.AreEquivalent(mockContext.Object.PlatformIdentities.SelectMany(m => m.Addresses).ToArray(), result.To);
	}

	[TestMethod]
	public void ShouldSetProvidedChannelProviderIds()
	{
		var list = fixture.Freeze<string[]>();
		var config = new SmsChannelConfiguration(_ => fixture.Create<IIdentityAddress>());
		config.AddChannelProviderFilter(list);
		var sut = new SmsChannel(config);
		CollectionAssert.AreEquivalent(list, sut.AllowedChannelProviderIds.ToArray());
	}

	[TestMethod]
	public void GeneratingCommunicationShouldThrowWithoutMessageTemplate()
	{
		var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var context = mockContext.Object;
		var sut = fixture.Create<SmsChannel>();
		var body = fixture.Freeze<string>();

		Assert.ThrowsExactlyAsync<CommunicationsException>(() => sut.GenerateCommunicationAsync(context));
	}

	[TestMethod]
	public async Task ShouldSetProvidedFromAddressResolver()
	{
		var from = fixture.Freeze<IIdentityAddress>();
		var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var context = mockContext.Object;
		var body = fixture.Freeze<string>();
		var config = new SmsChannelConfiguration(_ => from);
		config.Message.AddStringTemplate(body);
		var sut = new SmsChannel(config);

		var result = await sut.GenerateCommunicationAsync(context);

		Assert.AreSame(from, result.From);
	}

	[TestMethod]
	public async Task ContentModelResourceShouldAddSmsAttachment()
	{
		var from = fixture.Freeze<IIdentityAddress>();
		var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
		var resource = new Resource("res", "ct", new MemoryStream());
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([resource]);
		var context = mockContext.Object;
		var body = fixture.Freeze<string>();
		var config = new SmsChannelConfiguration(_ => from);
		config.Message.AddStringTemplate(body);

		var sut = new SmsChannel(config);

		var result = await sut.GenerateCommunicationAsync(context);

		Assert.AreEqual(1, result.Attachments.Count);
		Assert.AreEqual(resource.Name, result.Attachments.First().Name);
	}
}