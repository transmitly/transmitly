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

namespace Transmitly.Tests;

[TestClass]
public class DispatchMiddlewareTests
{
        private sealed class RecordingMiddleware(string name, List<string> order) : IDispatchMiddleware
        {
                public async Task<IReadOnlyCollection<IDispatchResult?>> InvokeAsync(DispatchMiddlewareContext ctx, Func<DispatchMiddlewareContext, Task<IReadOnlyCollection<IDispatchResult?>>> next)
                {
                        order.Add(name);
                        return await next(ctx).ConfigureAwait(false);
                }
        }

        private sealed class ShortCircuitMiddleware : IDispatchMiddleware
        {
                public bool Invoked { get; private set; }
                public Task<IReadOnlyCollection<IDispatchResult?>> InvokeAsync(DispatchMiddlewareContext ctx, Func<DispatchMiddlewareContext, Task<IReadOnlyCollection<IDispatchResult?>>> next)
                {
                        Invoked = true;
                        IReadOnlyCollection<IDispatchResult?> results = [new DispatchResult(CommunicationsStatus.Success("short"))];
                        return Task.FromResult(results);
                }
        }

        private sealed class FlagDispatcher : IChannelProviderDispatcher<object>
        {
                public static bool WasCalled;
                public FlagDispatcher() => WasCalled = false;
                public Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(object communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
                {
                        WasCalled = true;
                        return Task.FromResult<IReadOnlyCollection<IDispatchResult?>>([new DispatchResult(CommunicationsStatus.Success("flag"))]);
                }
        }

        private sealed class AfterMiddleware : IDispatchMiddleware
        {
                public bool Invoked { get; private set; }
                public async Task<IReadOnlyCollection<IDispatchResult?>> InvokeAsync(DispatchMiddlewareContext ctx, Func<DispatchMiddlewareContext, Task<IReadOnlyCollection<IDispatchResult?>>> next)
                {
                        Invoked = true;
                        return await next(ctx).ConfigureAwait(false);
                }
        }

        [TestMethod]
        public async Task MiddlewaresExecuteInRegistrationOrder()
        {
                var order = new List<string>();
                var client = new CommunicationsClientBuilder()
                        .AddDispatchMiddleware(new RecordingMiddleware("mw1", order))
                        .AddDispatchMiddleware(new RecordingMiddleware("mw2", order))
                        .ChannelProvider.Add<SuccessChannelProviderDispatcher, object>("test")
                        .AddPipeline("test", options =>
                        {
                                options.AddChannel(new UnitTestChannel("unit-test-address").HandlesAddressStartsWith("unit-test"));
                        })
                        .BuildClient();

                await client.DispatchAsync("test", "unit-test-address-to", new { });

                CollectionAssert.AreEqual(["mw1", "mw2"], order);
        }

        [TestMethod]
        public async Task MiddlewareCanShortCircuitDispatch()
        {
                var shortCircuit = new ShortCircuitMiddleware();
                var after = new AfterMiddleware();
                var client = new CommunicationsClientBuilder()
                        .AddDispatchMiddleware(shortCircuit)
                        .AddDispatchMiddleware(after)
                        .ChannelProvider.Add<FlagDispatcher, object>("test")
                        .AddPipeline("test", options =>
                        {
                                options.AddChannel(new UnitTestChannel("unit-test-address").HandlesAddressStartsWith("unit-test"));
                        })
                        .BuildClient();

                var result = await client.DispatchAsync("test", "unit-test-address-to", new { });

                Assert.IsTrue(shortCircuit.Invoked);
                Assert.IsFalse(after.Invoked);
                Assert.IsFalse(FlagDispatcher.WasCalled);
                Assert.AreEqual("short", result.Results.First()?.ResourceId);
        }
}

