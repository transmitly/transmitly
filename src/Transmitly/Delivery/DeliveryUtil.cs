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

namespace Transmitly.Delivery
{
	public static class DeliveryUtil
	{
		public const string ResourceIdKey = "_tlyrid";
		public const string PipelineNameKey = "_tlypn";
		public const string ChannelIdKey = "_tlycid";
		public const string ChannelProviderIdKey = "_tlycpid";

		public static Uri AddPipelineContext(this Uri url, string resourceId, string pipelineName, string channel, string channelProvider)
		{
			return Guard.AgainstNull(url)
				.AddParameter(ResourceIdKey, resourceId)
				.AddParameter(PipelineNameKey, pipelineName)
				.AddParameter(ChannelIdKey, channel)
				.AddParameter(ChannelProviderIdKey, channelProvider);
		}

		//Source=https://stackoverflow.com/a/19679135
		public static Uri AddParameter(this Uri url, string paramName, string paramValue)
		{
			var uriBuilder = new UriBuilder(url);
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);
			query[paramName] = paramValue;
			uriBuilder.Query = query.ToString();

			return uriBuilder.Uri;
		}
	}
}
