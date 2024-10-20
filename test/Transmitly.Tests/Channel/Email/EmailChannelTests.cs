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
using Transmitly.Exceptions;
using Transmitly.Tests;

namespace Transmitly.Channel.Email.Tests
{
    [TestClass()]
    public class EmailChannelTests : BaseUnitTest
    {
        [TestMethod()]
        //https://gist.github.com/cjaoude/fd9910626629b53c4d25
        [DataRow("email@transmit.ly", true)]
        [DataRow("email@subdomain.example.com", true)]
        [DataRow("\"email\"@example.com", true)]
        [DataRow("1234567890@example.com", true)]
        [DataRow("email@example-one.com", true)]
        [DataRow("_______@example.com", true)]
        [DataRow("email@example.name", true)]
        [DataRow("email@example.museum", true)]
        [DataRow("email@example.co.jp", true)]
        [DataRow("email@example.web", true)]
        [DataRow("あいうえお@example.com", true)]
        [DataRow("email+suffix@example.com", true)]
        [DataRow("email.dot.dot+suffix+suffix-@example.com", true)]

        //while valid, these are uncommon and not matched
        [DataRow("email @example.com", false)]
        [DataRow("firstname.lastname @example.com", false)]
        [DataRow("firstname+lastname @example.com", false)]
        [DataRow("email@123.123.123.123", false)]
        [DataRow("email@[123.123.123.123]", false)]
        [DataRow("firstname-lastname @example.co", false)]
        [DataRow("much.”more\\ unusual”@example.com", false)]
        [DataRow("very.unusual.”@”.unusual.com @example.com", false)]
        [DataRow("very.”(),:;<>[]”.VERY.”very@\\ \"very”.unusual@strange.example.com", false)]

        //invalid 
        [DataRow("plainaddress", false)]
        [DataRow("#@%^%#$@#$@#.com", false)]
        [DataRow("@example.com", false)]
        [DataRow("Joe Smith <email @example.com>", false)]
        [DataRow("email.example.com", false)]
        [DataRow("email@example @example.com", false)]
        [DataRow(".email @example.com", false)]
        [DataRow("email.@example.com", false)]
        [DataRow("email..email @example.com", false)]
        [DataRow("email@example.com (Joe Smith)", false)]
        [DataRow("email@example", false)]
        [DataRow("email@-example.com", false)]
        [DataRow("email@111.222.333.44444", false)]
        [DataRow("email @example..com", false)]
        [DataRow("Abc..123@example.com", false)]
        public void MatchesEmailAddressesAsExpected(string email, bool expected)
        {
            var channel = new EmailChannel(fixture.Create<IIdentityAddress>());

            var result = channel.SupportsIdentityAddress(email.AsIdentityAddress());
            Assert.AreEqual(expected, result, email);
        }

        [TestMethod]
        public async Task GenerateCommunicationAsyncShouldGuardAgainstNullContext()
        {
            var channel = new EmailChannel(fixture.Create<IIdentityAddress>());

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => channel.GenerateCommunicationAsync(null!));
        }
        private EmailChannel NewEmailChannel()
        {
            return fixture.Build<EmailChannel>().FromFactory<IIdentityAddress>((from) => new EmailChannel(from)).Create();
        }
        [TestMethod]
        public async Task GenerateCommunicationAsyncShouldGenerateValidSmsCommunication()
        {
            var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
            mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
            var context = mockContext.Object;
            var sut = NewEmailChannel();
            var body = fixture.Freeze<string>();
            sut.HtmlBody.AddStringTemplate(body);

            var result = await sut.GenerateCommunicationAsync(context);

            Assert.IsInstanceOfType(result, typeof(IEmail));
            var email = (IEmail)result;
            Assert.AreEqual(sut.FromAddress, email.From);
            Assert.AreEqual(body, email.HtmlBody);
            Assert.AreEqual(context.TransportPriority, email.TransportPriority);
            Assert.AreEqual(sut.DeliveryReportCallbackUrl, email.DeliveryReportCallbackUrl);
            Assert.AreEqual(sut.DeliveryReportCallbackUrlResolver, email.DeliveryReportCallbackUrlResolver);
            CollectionAssert.AreEquivalent(mockContext.Object.PlatformIdentities.SelectMany(m => m.Addresses).ToArray(), email.To);
        }

        [TestMethod]
        public void ShouldSetProvidedChannelProviderIds()
        {
            var list = fixture.Freeze<string[]>();
            var sut = new EmailChannel(fixture.Create<IIdentityAddress>(), list);
            CollectionAssert.AreEquivalent(list, sut.AllowedChannelProviderIds.ToArray());
        }

        [TestMethod]
        public void GeneratingCommunicationShouldThrowWithoutSubject()
        {
            var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
            mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);

            var context = mockContext.Object;
            var sut = NewEmailChannel();

            Assert.ThrowsExceptionAsync<CommunicationsException>(() => sut.GenerateCommunicationAsync(context));
        }

        [TestMethod]
        public void ShouldSetProvidedFromAddress()
        {
            var from = fixture.Create<IIdentityAddress>();
            var channelProviderIds = fixture.Freeze<string[]>();

            var sut = new EmailChannel(from, channelProviderIds);
            Assert.AreSame(from, sut.FromAddress);

            sut = new EmailChannel(from);
            Assert.AreSame(from, sut.FromAddress);
        }

        [TestMethod]
        public async Task ShouldSetProvidedFromAddressResolver()
        {
            var from = fixture.Freeze<IIdentityAddress>();
            var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
            mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
            var context = mockContext.Object;
            var body = fixture.Freeze<string>();


            var sut = new EmailChannel((ctx) => from);
            sut.Subject.AddStringTemplate(body);
            var result = await sut.GenerateCommunicationAsync(context);
            var email = (IEmail)result;
            Assert.AreSame(from, email.From);
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
            var sut = new EmailChannel(from);
            sut.Subject.AddStringTemplate(body);

            var result = await sut.GenerateCommunicationAsync(context);
            var email = (IEmail)result;
            Assert.AreEqual(1, email.Attachments.Count);
            Assert.AreEqual(resource.Name, email.Attachments.First().Name);
        }
    }
}