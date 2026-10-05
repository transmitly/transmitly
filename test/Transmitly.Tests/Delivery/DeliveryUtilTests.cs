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

using Transmitly.Delivery;

namespace Transmitly.Tests.Delivery
{
	[TestClass]
	public class DeliveryUtilTests
	{
		[TestMethod]
		public void AddPipelineContextAddsResourceIdWhenKnown()
		{
			var url = new Uri("https://example.com/delivery-reports")
				.AddPipelineContext("resource-1", "OrderCreated", "pipeline-1", "Sms", "Twilio");

			var query = ParseQuery(url);
			Assert.AreEqual("resource-1", Value(query, DeliveryUtil.ResourceIdKey));
			Assert.AreEqual("OrderCreated", Value(query, DeliveryUtil.PipelineIntentKey));
			Assert.AreEqual("pipeline-1", Value(query, DeliveryUtil.PipelineIdKey));
			Assert.AreEqual("Sms", Value(query, DeliveryUtil.ChannelIdKey));
			Assert.AreEqual("Twilio", Value(query, DeliveryUtil.ChannelProviderIdKey));
			Assert.IsFalse(string.IsNullOrWhiteSpace(Value(query, DeliveryUtil.EventIdKey)));
		}

		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		public void AddPipelineContextOmitsResourceIdWhenNotYetKnown(string? resourceId)
		{
			// Providers such as Twilio assign the resource id after dispatch, so the callback
			// URL is built before one exists.
			var url = new Uri("https://example.com/delivery-reports")
				.AddPipelineContext(resourceId, "OrderCreated", null, "Sms", "Twilio");

			var query = ParseQuery(url);
			Assert.IsFalse(query.ContainsKey(DeliveryUtil.ResourceIdKey));
			Assert.AreEqual("OrderCreated", Value(query, DeliveryUtil.PipelineIntentKey));
			Assert.AreEqual("Sms", Value(query, DeliveryUtil.ChannelIdKey));
			Assert.AreEqual("Twilio", Value(query, DeliveryUtil.ChannelProviderIdKey));
		}

		[TestMethod]
		public void AddPipelineContextRequiresChannelContext()
		{
			var url = new Uri("https://example.com/delivery-reports");

			Assert.ThrowsExactly<ArgumentNullException>(() => url.AddPipelineContext(null, "OrderCreated", null, null, "Twilio"));
			Assert.ThrowsExactly<ArgumentNullException>(() => url.AddPipelineContext(null, "OrderCreated", null, "Sms", null));
		}

		private static string? Value(Dictionary<string, string> query, string key) =>
			query.TryGetValue(key, out var value) ? value : null;

		private static Dictionary<string, string> ParseQuery(Uri url) =>
			url.Query.TrimStart('?')
				.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(pair => pair.Split(new[] { '=' }, 2))
				.ToDictionary(pair => Uri.UnescapeDataString(pair[0]), pair => pair.Length > 1 ? Uri.UnescapeDataString(pair[1].Replace('+', ' ')) : string.Empty);
	}
}
