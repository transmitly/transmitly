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
using Transmitly.ChannelProvider.Configuration;
using Transmitly.Pipeline.Configuration;
using Transmitly.Template.Configuration;
using Transmitly.Tests.Integration;
using Transmitly.Delivery;
using Transmitly.PlatformIdentity.Configuration;
using Transmitly.Persona.Configuration;
using AutoFixture;
namespace Transmitly.Tests
{
    [TestClass]
    public class DefaultCommunicationClientTests:BaseUnitTest
    {


        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        public void ShouldGuardEmptyPipelineName(string value)
        {
            var (pipeline, channelProvider, template, reportHandler, identityResolver, persona) = GetStores();

            var client = new DefaultCommunicationsClient(pipeline.Object, channelProvider.Object, template.Object, persona.Object, identityResolver.Object, reportHandler.Object);
            Assert.ThrowsExceptionAsync<ArgumentNullException>(() => client.DispatchAsync(value, "test", new { }));
        }

        [TestMethod]
        public async Task ShouldSendBasedOnAllowedChannelProviderRestrictions()
        {
            const string ChannelProvider0 = "channel-provider-0";
            const string ChannelProvider1 = "channel-provider-1";
            const string ChannelId = "unit-test-channel";
            var client = new CommunicationsClientBuilder()
                    .ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, object>(ChannelProvider0, "unit-test-channel")
                    .ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, object>(ChannelProvider1, "unit-test-channel")
                    .AddPipeline("test-pipeline", options =>
                    {
                        options.AddChannel(new UnitTestChannel("c0-from", ChannelId, ChannelProvider0));
                        options.AddChannel(new UnitTestChannel("c1-from", ChannelId, ChannelProvider1));
                        options.AddChannel(new UnitTestChannel("c2-from", "channel-not-included", ChannelProvider0));
                    })
                    .BuildClient();
            var result = await client.DispatchAsync("test-pipeline", "unit-test-address-0", new { });
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(1, result.Results.Count);
            Assert.AreEqual(ChannelId, result.Results?.First()?.ChannelId);
            Assert.AreEqual(ChannelProvider0, result.Results?.First()?.ChannelProviderId);

        }

        private static (
            Mock<IPipelineFactory> pipeline,
            Mock<IChannelProviderFactory> channelProvider,
            Mock<ITemplateEngineFactory> template,
            Mock<IDeliveryReportReporter> deliveryReportHandler,
            Mock<IPlatformIdentityResolverFactory> platformIdnetityResolver,
            Mock<IPersonaFactory> persona
            ) GetStores()
        {
            return (
                new Mock<IPipelineFactory>(),
                new Mock<IChannelProviderFactory>(),
                new Mock<ITemplateEngineFactory>(),
                new Mock<IDeliveryReportReporter>(),
                new Mock<IPlatformIdentityResolverFactory>(),
                new Mock<IPersonaFactory>()
            );
        }

        [TestMethod]
        public async Task ShouldDispatchUsingCorrectChannelProviderDispatcher()
        {
            var tly = new CommunicationsClientBuilder();

            tly.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, ISms>("test-channel-provider");
            tly.ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, IEmail>("test-channel-provider");
            tly.ChannelProvider.Add<OptionalConfigurationTestChannelProviderDispatcher, UnitTestCommunication>("test-channel-provider");

            tly.AddPipeline("test-pipeline", options =>
            {
                options.AddEmail("from@address.com".AsIdentityAddress(), email =>
                {
                    email.Subject.AddStringTemplate("Test sub");
                });
            });

            var client = tly.BuildClient();
            Assert.IsNotNull(client);
            var result = await client.DispatchAsync("test-pipeline", "test@test.com", new { });
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(1, result.Results.Count);
            Assert.AreEqual(DispatchStatus.Dispatched, result.Results.First()?.DispatchStatus);
            Assert.AreEqual("EmailCommunication", result.Results.First()?.ResourceId);
        }

        [TestMethod]
        public async Task ShouldRespectAllowedChannelProviderPreference()
        {
            const string PipelineName = "test-pipeline";
            IReadOnlyCollection<IIdentityAddress> testRecipients = ["8885556666".AsIdentityAddress()];
            var model = TransactionModel.Create(new { });

            var tly = new CommunicationsClientBuilder()
                .ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, ISms>("sms-provider")
                .ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, IVoice>("voice-provider")
                .AddPipeline(PipelineName, options =>
                {
                    options.AddSms(sms =>
                    {
                        sms.Message.AddStringTemplate("SmsText");
                    });

                    options.AddVoice(voice =>
                    {
                        voice.Message.AddStringTemplate("Voice");
                    });
                })
                .BuildClient();

            var result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Voice()]);

            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(1, result.Results.Count);
            Assert.AreEqual(Id.Channel.Voice(), result.Results?.First()?.ChannelId);

            result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Sms()]);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(1, result.Results.Count);
            Assert.AreEqual(Id.Channel.Sms(), result.Results?.First()?.ChannelId);

            result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Email()]);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(0, result.Results.Count);

            result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Voice(), Id.Channel.Sms()]);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(1, result.Results.Count);
            Assert.AreEqual(Id.Channel.Sms(), result.Results?.First()?.ChannelId);
        }

        [TestMethod]
        public async Task ShouldRespectAllowedChannelProviderPreferenceAnyDeliveryStrategy()
        {
            const string PipelineName = "test-pipeline";
            IReadOnlyCollection<IIdentityAddress> testRecipients = ["8885556666".AsIdentityAddress()];
            var model = TransactionModel.Create(new { });

            var tly = new CommunicationsClientBuilder()
                .ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, ISms>("sms-provider")
                .ChannelProvider.Add<MinimalConfigurationTestChannelProviderDispatcher, IVoice>("voice-provider")
                .AddPipeline(PipelineName, options =>
                {
                    options.AddSms(sms =>
                    {
                        sms.Message.AddStringTemplate("SmsText");
                    });

                    options.AddVoice(voice =>
                    {
                        voice.Message.AddStringTemplate("Voice");
                    });
                    options.UseAnyMatchPipelineDeliveryStrategy();
                })
                .BuildClient();

            var result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Voice()]);

            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(1, result.Results.Count);
            Assert.AreEqual(Id.Channel.Voice(), result.Results?.First()?.ChannelId);

            result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Sms()]);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(1, result.Results.Count);
            Assert.AreEqual(Id.Channel.Sms(), result.Results?.First()?.ChannelId);

            result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Email()]);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(0, result.Results.Count);

            result = await tly.DispatchAsync(PipelineName, testRecipients, model, [Id.Channel.Voice(), Id.Channel.Sms()]);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(2, result.Results.Count);
            Assert.AreEqual(Id.Channel.Sms(), result.Results?.First()?.ChannelId);
#pragma warning disable S2589 // Boolean expressions should not be gratuitous
            Assert.AreEqual(Id.Channel.Voice(), result.Results?.Skip(1)?.First()?.ChannelId);
#pragma warning restore S2589 // Boolean expressions should not be gratuitous
        }

        [TestMethod()]
        public async Task ShouldThrowIfNullIdentityAddressCollection()
        {
            var sut = fixture.Create<DefaultCommunicationsClient>();
            
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                sut.DispatchAsync("test", (IReadOnlyCollection<IIdentityAddress>)null!, null!, null!, CancellationToken.None)
            );

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                sut.DispatchAsync("test", (string)null!, (object)null!, null, CancellationToken.None)
            );

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                sut.DispatchAsync("test", (string)null!, (object)null!, null, CancellationToken.None)
            );
        }
    }
}