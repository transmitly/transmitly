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

using System.Web;

namespace Transmitly.Delivery;

public static class DeliveryUtil
{
	public const string ResourceIdKey = "tlyr";
	public const string PipelineIntentKey = "tlyp";
	public const string ChannelIdKey = "tlyc";
	public const string ChannelProviderIdKey = "tlycp";
	public const string EventIdKey = "tlye";
	public const string PipelineIdKey = "tlypid";

	/// <summary>
	/// Adds the pipeline context a channel provider's delivery report adaptor needs to recognize a callback.
	/// </summary>
	/// <param name="url">The delivery report callback URL.</param>
	/// <param name="resourceId">The provider's id for the communication. Often unknown at dispatch, when the provider assigns it; omitted when empty.</param>
	/// <param name="pipelineIntent">Intent of the pipeline.</param>
	/// <param name="pipelineId">Id of the pipeline.</param>
	/// <param name="channel">Id of the channel.</param>
	/// <param name="channelProvider">Id of the channel provider.</param>
	/// <returns>The callback URL with the pipeline context added.</returns>
	public static Uri AddPipelineContext(this Uri url, string? resourceId, string pipelineIntent, string? pipelineId, string? channel, string? channelProvider)
	{
		Guard.AgainstNullOrWhiteSpace(channel);
		Guard.AgainstNullOrWhiteSpace(channelProvider);
		Guard.AgainstNullOrWhiteSpace(pipelineIntent);

		return Guard.AgainstNull(url)
			.AddParameter(ResourceIdKey, resourceId)
			.AddParameter(PipelineIntentKey, pipelineIntent)
			.AddParameter(PipelineIdKey, pipelineId ?? string.Empty)
			.AddParameter(ChannelIdKey, channel)
			.AddParameter(ChannelProviderIdKey, channelProvider)
			.AddParameter(EventIdKey, Guid.NewGuid().ToString("N"));
	}

	//Source=https://stackoverflow.com/a/19679135
	public static Uri AddParameter(this Uri url, string paramName, string? paramValue)
	{
		if (string.IsNullOrEmpty(paramValue))
			return url;

		var uriBuilder = new UriBuilder(url);
		var query = HttpUtility.ParseQueryString(uriBuilder.Query);
		query[paramName] = paramValue;
		uriBuilder.Query = query.ToString();

		return uriBuilder.Uri;
	}
}
