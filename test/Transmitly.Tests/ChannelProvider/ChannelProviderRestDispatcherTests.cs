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

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using Transmitly.Tests;

namespace Transmitly.ChannelProvider.Tests;

[TestClass]
public class ChannelProviderRestDispatcherTests
{
	/// <summary>
	/// <see cref="ChannelProviderRestDispatcher{TCommunication}"/> used to create a new HttpClient, with its own
	/// connection pool, for every instance and never dispose it. Every dispatch opened another socket that stayed open,
	/// which exhausts sockets under load.
	/// </summary>
	[TestMethod]
	public async Task DispatchingRepeatedlyShouldReuseOneConnectionInsteadOfOpeningOnePerDispatch()
	{
		const int DispatchCount = 5;
		using var server = new LoopbackHttpServer();

		var client = new CommunicationsClientBuilder()
			.ChannelProvider.Add<LoopbackRestDispatcher, UnitTestCommunication>("loopback-provider", new LoopbackOptions(server.Url), "unit-test-channel")
			.AddPipeline("test-pipeline", options => options.AddChannel(new UnitTestChannel("c0-from")))
			.BuildClient();

		var instancesBefore = LoopbackRestDispatcher.InstanceCount;
		for (var i = 0; i < DispatchCount; i++)
		{
			var result = await client.DispatchAsync("test-pipeline", "unit-test-address-0", new { });
			Assert.IsTrue(result.IsSuccessful);
		}

		Assert.AreEqual(DispatchCount, LoopbackRestDispatcher.InstanceCount - instancesBefore, "Precondition: the non-DI path creates a dispatcher per dispatch.");
		Assert.AreEqual(1, server.ConnectionCount, "Each dispatch opened its own connection; dispatcher instances are not sharing a connection pool.");
	}

	[TestMethod]
	public async Task DisposingDispatcherShouldNotCloseConnectionsSharedWithOtherDispatchers()
	{
		using var server = new LoopbackHttpServer();

		var disposed = new TestRestDispatcher("first-token");
		_ = disposed.Client;
		disposed.Dispose();

		using var other = new TestRestDispatcher("second-token");
		using var response = await other.Client.GetAsync(server.Url);

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
	}

	[TestMethod]
	public async Task CookieSetForOneDispatcherShouldNotBeSentByAnother()
	{
		using var server = new LoopbackHttpServer();
		using var first = new TestRestDispatcher("first-token");
		using var second = new TestRestDispatcher("second-token");

		using (var firstResponse = await first.Client.GetAsync(server.Url)) { }
		using (var secondResponse = await second.Client.GetAsync(server.Url)) { }

		var secondRequest = server.Requests.Last();
		StringAssert.Contains(secondRequest, "Bearer second-token", "Precondition: the last request came from the second dispatcher.");
		Assert.IsFalse(secondRequest.Contains("\r\nCookie:"), "The cookie set on the first dispatcher's response was sent by the second dispatcher.");
	}

	[TestMethod]
	public void EachDispatcherShouldGetItsOwnHttpClientSoHeadersDoNotLeak()
	{
		using var first = new TestRestDispatcher("first-token");
		using var second = new TestRestDispatcher("second-token");

		Assert.AreNotSame(first.Client, second.Client);
		Assert.AreEqual("first-token", first.Client.DefaultRequestHeaders.Authorization?.Parameter);
		Assert.AreEqual("second-token", second.Client.DefaultRequestHeaders.Authorization?.Parameter);
	}

	[TestMethod]
	public void ShouldConfigureAndUseProvidedHttpClient()
	{
		var provided = new HttpClient();
		using var dispatcher = new TestRestDispatcher("token", provided);

		Assert.AreSame(provided, dispatcher.Client);
		Assert.AreEqual("token", provided.DefaultRequestHeaders.Authorization?.Parameter);
	}

	private sealed class LoopbackOptions(string url)
	{
		public string Url { get; } = url;
	}

	private sealed class LoopbackRestDispatcher : ChannelProviderRestDispatcher<UnitTestCommunication>
	{
		private static int _instanceCount;
		private readonly LoopbackOptions _options;

		public LoopbackRestDispatcher(LoopbackOptions options)
		{
			_options = options;
			Interlocked.Increment(ref _instanceCount);
		}

		public static int InstanceCount => Volatile.Read(ref _instanceCount);

		protected override async Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(HttpClient restClient, UnitTestCommunication communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken)
		{
			using var response = await restClient.GetAsync(_options.Url, cancellationToken).ConfigureAwait(false);
			return [new DispatchResult(response.IsSuccessStatusCode ? CommunicationsStatus.Success("Dispatched") : CommunicationsStatus.ClientError("Failed"))];
		}
	}

	private sealed class TestCommunication;

	private sealed class TestRestDispatcher(string token, HttpClient? httpClient = null) : ChannelProviderRestDispatcher<TestCommunication>(httpClient)
	{
		public HttpClient Client => HttpClient;

		protected override void ConfigureHttpClient(HttpClient httpClient)
		{
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			base.ConfigureHttpClient(httpClient);
		}

		protected override Task<IReadOnlyCollection<IDispatchResult?>> DispatchAsync(HttpClient restClient, TestCommunication communication, IDispatchCommunicationContext communicationContext, CancellationToken cancellationToken) =>
			Task.FromResult<IReadOnlyCollection<IDispatchResult?>>([]);
	}

	/// <summary>
	/// Minimal keep-alive HTTP server on a loopback port: answers 200 (setting a cookie) to every request,
	/// records each request's head and counts accepted connections.
	/// </summary>
	private sealed class LoopbackHttpServer : IDisposable
	{
		private static readonly byte[] OkResponse = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nSet-Cookie: session=loopback; Path=/\r\nContent-Length: 0\r\n\r\n");
		private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
		private readonly System.Collections.Concurrent.ConcurrentQueue<string> _requests = new();
		private int _connectionCount;

		public LoopbackHttpServer()
		{
			_listener.Start();
			Url = $"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/";
			_ = AcceptConnectionsAsync();
		}

		public string Url { get; }

		public int ConnectionCount => Volatile.Read(ref _connectionCount);

		public IReadOnlyCollection<string> Requests => _requests.ToArray();

		public void Dispose() => _listener.Stop();

		private async Task AcceptConnectionsAsync()
		{
			try
			{
				while (true)
				{
					var connection = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
					Interlocked.Increment(ref _connectionCount);
					_ = RespondToEveryRequestAsync(connection);
				}
			}
			catch (Exception)
			{
				// listener stopped
			}
		}

		private async Task RespondToEveryRequestAsync(TcpClient connection)
		{
			using (connection)
			{
				var stream = connection.GetStream();
				var buffer = new byte[4096];
				var pending = string.Empty;
				try
				{
					int read;
					while ((read = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
					{
						pending += Encoding.ASCII.GetString(buffer, 0, read);
						int end;
						while ((end = pending.IndexOf("\r\n\r\n", StringComparison.Ordinal)) >= 0)
						{
							_requests.Enqueue(pending.Substring(0, end));
							pending = pending.Substring(end + 4);
							await stream.WriteAsync(OkResponse, 0, OkResponse.Length).ConfigureAwait(false);
						}
					}
				}
				catch (Exception)
				{
					// client closed the connection
				}
			}
		}
	}
}
