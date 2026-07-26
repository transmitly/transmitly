// Copyright (c) Code Impressions, LLC. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using AutoFixture;
using Transmitly.Channel.Configuration.WhatsApp;
using Transmitly.Exceptions;
using Transmitly.Tests;
using Transmitly.Channel.WhatsApp;

namespace Transmitly.Channel.WhatsApp.Tests;

[TestClass]
public class WhatsAppChannelTests : BaseUnitTest
{
	[TestMethod]
	[DataRow("+14155552671", "whatsapp", true)]
	[DataRow("whatsapp:+14155552671", null, true)]
	[DataRow("WHATSAPP:+14155552671", null, true)]
	[DataRow("whatsapp:+442071838750", null, true)]
	[DataRow("+14155552671", null, false)]
	[DataRow("sms:+14155552671", null, false)]
	[DataRow("not-a-phone", "whatsapp", false)]
	public void SupportsIdentityAddressTest(string value, string? type, bool expected)
	{
		var whatsApp = new WhatsAppChannel(new WhatsAppChannelConfiguration());
		var addressType = string.Equals(type, "whatsapp", StringComparison.OrdinalIgnoreCase)
			? PlatformIdentityAddress.Types.WhatsApp()
			: type;

		var result = whatsApp.SupportsIdentityAddress(new PlatformIdentityAddress(value, type: addressType));

		Assert.AreEqual(expected, result, value);
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldGuardAgainstNullContext()
	{
		var channel = fixture.Create<WhatsAppChannel>();

		await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => channel.GenerateCommunicationAsync(null!));
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldGenerateValidWhatsAppCommunication()
	{
		var mockContext = CreateDispatchCommunicationContextMock();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var context = mockContext.Object;
		var from = "8888".AsIdentityAddress();
		var config = new WhatsAppChannelConfiguration(_ => from);
		var body = fixture.Freeze<string>();
		config.Message.AddStringTemplate(body);

		var sut = new WhatsAppChannel(config);

		var result = await sut.GenerateCommunicationAsync(context);

		Assert.IsInstanceOfType<IWhatsApp>(result);

		Assert.AreEqual(from, result.From);
		Assert.AreEqual(body, result.Message);
		Assert.AreEqual(context.MessagePriority, result.Priority);
		Assert.AreEqual(context.TransportPriority, result.TransportPriority);
		CollectionAssert.AreEquivalent(mockContext.Object.PlatformIdentities.SelectMany(m => m.Addresses).ToArray(), result.To);
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldAllowAttachmentOnlyPayload()
	{
		var mockContext = CreateDispatchCommunicationContextMock();
		var resource = new Resource("menu.pdf", "application/pdf", new MemoryStream([1, 2, 3]));
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([resource]);
		var sut = new WhatsAppChannel(new WhatsAppChannelConfiguration());

		var result = await sut.GenerateCommunicationAsync(mockContext.Object);

		Assert.IsNull(result.Message);
		Assert.AreEqual(1, result.Attachments.Count);
		Assert.AreEqual(resource.Name, result.Attachments.Single().Name);
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldRenderLocationAndContacts()
	{
		var mockContext = CreateDispatchCommunicationContextMock();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var config = new WhatsAppChannelConfiguration();
		config.AddLocation(location =>
		{
			location.Latitude.AddTemplateResolver(_ => Task.FromResult<string?>("40.7128"));
			location.Longitude.AddTemplateResolver(_ => Task.FromResult<string?>("-74.0060"));
			location.Name.AddTemplateResolver(_ => Task.FromResult<string?>("Downtown Store"));
			location.Address.AddTemplateResolver(_ => Task.FromResult<string?>("123 Main Street"));
		});
		config.AddContact(contact =>
		{
			contact.FirstName.AddTemplateResolver(_ => Task.FromResult<string?>("Ada"));
			contact.LastName.AddTemplateResolver(_ => Task.FromResult<string?>("Lovelace"));
			contact.Organization.AddTemplateResolver(_ => Task.FromResult<string?>("Transmitly"));
			contact.AddPhone(phone =>
			{
				phone.PhoneNumber.AddTemplateResolver(_ => Task.FromResult<string?>("+14155552671"));
				phone.Type.AddTemplateResolver(_ => Task.FromResult<string?>("work"));
				phone.WhatsAppId.AddTemplateResolver(_ => Task.FromResult<string?>("14155552671"));
			});
			contact.AddEmail(email =>
			{
				email.EmailAddress.AddTemplateResolver(_ => Task.FromResult<string?>("ada@example.com"));
				email.Type.AddTemplateResolver(_ => Task.FromResult<string?>("work"));
			});
		});
		var sut = new WhatsAppChannel(config);

		var result = await sut.GenerateCommunicationAsync(mockContext.Object);

		Assert.IsNotNull(result.Location);
		Assert.AreEqual(40.7128d, result.Location.Latitude, 0.0001d);
		Assert.AreEqual(-74.0060d, result.Location.Longitude, 0.0001d);
		Assert.AreEqual("Downtown Store", result.Location.Name);
		Assert.AreEqual("123 Main Street", result.Location.Address);
		Assert.AreEqual(1, result.Contacts.Count);
		Assert.AreEqual("Ada Lovelace", result.Contacts.Single().FormattedName);
		Assert.AreEqual("Transmitly", result.Contacts.Single().Organization);
		Assert.AreEqual("+14155552671", result.Contacts.Single().Phones.Single().PhoneNumber);
		Assert.AreEqual("14155552671", result.Contacts.Single().Phones.Single().WhatsAppId);
		Assert.AreEqual("ada@example.com", result.Contacts.Single().Emails.Single().EmailAddress);
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldSkipEmptyContactEntries()
	{
		var mockContext = CreateDispatchCommunicationContextMock();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var config = new WhatsAppChannelConfiguration();
		config.Message.AddStringTemplate("Hello");
		config.AddContact(_ => { });
		var sut = new WhatsAppChannel(config);

		var result = await sut.GenerateCommunicationAsync(mockContext.Object);

		Assert.AreEqual(0, result.Contacts.Count);
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldRenderTemplatePayloadInActionOrder()
	{
		var mockContext = CreateDispatchCommunicationContextMock();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var config = new WhatsAppChannelConfiguration();
		config.AddTemplate(template =>
		{
			template.AddName("order_update");
			template.AddLanguage("en");
			template.AddHeaderParameter(_ => Task.FromResult<string?>("HeaderValue"));
			template.AddBodyParameter(_ => Task.FromResult<string?>("First"));
			template.AddBodyParameter(_ => Task.FromResult<string?>("Second"));
			template.AddAction(WhatsAppTemplateActionType.Url, 2, action =>
			{
				action.Text.AddTemplateResolver(_ => Task.FromResult<string?>("View Order"));
				action.Value.AddTemplateResolver(_ => Task.FromResult<string?>("https://example.com/orders/123"));
			});
			template.AddAction(WhatsAppTemplateActionType.QuickReply, 0, action =>
			{
				action.Text.AddTemplateResolver(_ => Task.FromResult<string?>("Track"));
				action.Value.AddTemplateResolver(_ => Task.FromResult<string?>("track-order"));
			});
		});
		var sut = new WhatsAppChannel(config);

		var result = await sut.GenerateCommunicationAsync(mockContext.Object);

		Assert.IsNotNull(result.Template);
		Assert.AreEqual("order_update", result.Template.Name);
		Assert.AreEqual("en", result.Template.Language);
		CollectionAssert.AreEqual(new[] { "HeaderValue" }, result.Template.HeaderParameters.ToArray());
		CollectionAssert.AreEqual(new[] { "First", "Second" }, result.Template.BodyParameters.ToArray());
		Assert.AreEqual(2, result.Template.Actions.Count);
		Assert.AreEqual(0, result.Template.Actions.First().Index);
		Assert.AreEqual(WhatsAppTemplateActionType.QuickReply, result.Template.Actions.First().ActionType);
		Assert.AreEqual(2, result.Template.Actions.Last().Index);
		Assert.AreEqual(WhatsAppTemplateActionType.Url, result.Template.Actions.Last().ActionType);
	}

	[TestMethod]
	public void ShouldSetProvidedChannelProviderIds()
	{
		var list = fixture.Freeze<string[]>();
		var config = new WhatsAppChannelConfiguration(_ => fixture.Create<IPlatformIdentityAddress>());
		config.AddChannelProviderFilter(list);
		var sut = new WhatsAppChannel(config);
		CollectionAssert.AreEquivalent(list, sut.AllowedChannelProviderIds.ToArray());
	}

	[TestMethod]
	public async Task GeneratingCommunicationShouldThrowWithoutAnyContent()
	{
		var mockContext = CreateDispatchCommunicationContextMock();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var context = mockContext.Object;
		var sut = fixture.Create<WhatsAppChannel>();

		await Assert.ThrowsExactlyAsync<CommunicationsException>(() => sut.GenerateCommunicationAsync(context));
	}

	[TestMethod]
	public async Task ShouldSetProvidedFromAddressResolver()
	{
		var from = fixture.Freeze<IPlatformIdentityAddress>();
		var mockContext = CreateDispatchCommunicationContextMock();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var context = mockContext.Object;
		var body = fixture.Freeze<string>();
		var config = new WhatsAppChannelConfiguration(_ => from);
		config.Message.AddStringTemplate(body);
		var sut = new WhatsAppChannel(config);

		var result = await sut.GenerateCommunicationAsync(context);

		Assert.AreSame(from, result.From);
	}

	[TestMethod]
	public async Task ContentModelResourceShouldAddWhatsAppAttachment()
	{
		var from = fixture.Freeze<IPlatformIdentityAddress>();
		var mockContext = CreateDispatchCommunicationContextMock();
		var resource = new Resource("res", "ct", new MemoryStream());
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([resource]);
		var context = mockContext.Object;
		var body = fixture.Freeze<string>();
		var config = new WhatsAppChannelConfiguration(_ => from);
		config.Message.AddStringTemplate(body);

		var sut = new WhatsAppChannel(config);

		var result = await sut.GenerateCommunicationAsync(context);

		Assert.AreEqual(1, result.Attachments.Count);
		Assert.AreEqual(resource.Name, result.Attachments.First().Name);
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldThrowWhenLocationCoordinatesAreIncomplete()
	{
		var mockContext = CreateDispatchCommunicationContextMock();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var config = new WhatsAppChannelConfiguration();
		config.AddLocation(location =>
		{
			location.Latitude.AddStringTemplate("40.7128");
			location.Name.AddStringTemplate("Downtown Store");
		});
		var sut = new WhatsAppChannel(config);

		await Assert.ThrowsExactlyAsync<CommunicationsException>(() => sut.GenerateCommunicationAsync(mockContext.Object));
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldThrowWhenLocationCoordinatesAreInvalid()
	{
		var mockContext = CreateDispatchCommunicationContextMock();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var config = new WhatsAppChannelConfiguration();
		config.AddLocation(location =>
		{
			location.Latitude.AddStringTemplate("north");
			location.Longitude.AddStringTemplate("-74.0060");
		});
		var sut = new WhatsAppChannel(config);

		await Assert.ThrowsExactlyAsync<CommunicationsException>(() => sut.GenerateCommunicationAsync(mockContext.Object));
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldThrowWhenTemplateLanguageIsMissing()
	{
		var mockContext = CreateDispatchCommunicationContextMock();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		var config = new WhatsAppChannelConfiguration();
		config.AddTemplate(template =>
		{
			template.AddName("order_update");
		});
		var sut = new WhatsAppChannel(config);

		await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => sut.GenerateCommunicationAsync(mockContext.Object));
	}
}
