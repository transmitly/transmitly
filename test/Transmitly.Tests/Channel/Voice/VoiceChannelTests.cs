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
using Transmitly.Channel.Configuration.Voice;
using Transmitly.Exceptions;
using Transmitly.Tests;

namespace Transmitly.Channel.Voice.Tests;

[TestClass]
public class VoiceChannelTests : BaseUnitTest
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
	public void SupportsIdentityAddressTest(string value, bool expected)
	{
		var sms = new VoiceChannel(new VoiceChannelConfiguration(null));
		var result = sms.SupportsIdentityAddress(value.AsIdentityAddress());
		Assert.AreEqual(expected, result, value);
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldGuardAgainstNullContext()
	{
		var channel = fixture.Create<VoiceChannel>();

		await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => channel.GenerateCommunicationAsync(null!));
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldGenerateValidSmsCommunication()
	{
		var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var context = mockContext.Object;
		var from = "8888".AsIdentityAddress();
		var config = new VoiceChannelConfiguration(_ => from);
		var body = fixture.Freeze<string>();
		config.Message.AddStringTemplate(body);

		var sut = new VoiceChannel(config);

		var result = await sut.GenerateCommunicationAsync(context);

		Assert.IsInstanceOfType(result, typeof(IVoice));

		Assert.AreEqual(from, result.From);
		Assert.AreEqual(body, result.Message);
		Assert.AreEqual(context.TransportPriority, result.TransportPriority);
		CollectionAssert.AreEquivalent(mockContext.Object.PlatformIdentities.SelectMany(m => m.Addresses).ToArray(), result.To);
	}

	[TestMethod]
	public void ShouldSetProvidedChannelProviderIds()
	{
		var list = fixture.Freeze<string[]>();
		var config = new VoiceChannelConfiguration(null);
		config.AddChannelProviderFilter(list);
		var sut = new VoiceChannel(config);
		CollectionAssert.AreEquivalent(list, sut.AllowedChannelProviderIds.ToArray());
	}

	[TestMethod]
	public void GeneratingCommunicationShouldThrowWithoutMessageTemplate()
	{
		var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var context = mockContext.Object;
		var sut = fixture.Create<VoiceChannel>();
		var body = fixture.Freeze<string>();

		Assert.ThrowsExactlyAsync<CommunicationsException>(() => sut.GenerateCommunicationAsync(context));
	}

	[TestMethod]
	public void ShouldSetProvidedFromAddress()
	{
		var from = fixture.Freeze<IIdentityAddress>();
		var config = new VoiceChannelConfiguration(_ => from);
		var sut = new VoiceChannel(config);
		Assert.IsNotNull(config.FromAddressResolver);
		Assert.AreSame(from, config.FromAddressResolver(new Mock<IDispatchCommunicationContext>().Object));
	}

	[TestMethod]
	public async Task ShouldSetProvidedFromAddressResolver()
	{
		var from = fixture.Freeze<IIdentityAddress>();
		var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var context = mockContext.Object;
		var body = fixture.Freeze<string>();

		var config = new VoiceChannelConfiguration(_ => from);
		config.Message.AddStringTemplate(body);
		var sut = new VoiceChannel(config);

		var result = await sut.GenerateCommunicationAsync(context);
		Assert.AreSame(from, result.From);
	}
}