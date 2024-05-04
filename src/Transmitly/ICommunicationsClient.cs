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

namespace Transmitly
{
	/// <summary>
	/// Client for dispatching communications
	/// </summary>
	public interface ICommunicationsClient : ISenderVerificationCommunicationsClient
	{
		/// <summary>
		/// Dispatches the communications for the provided pipeline name.
		/// </summary>
		/// <param name="pipelineName">Name of the pipeline.</param>
		/// <param name="audiences">Potential recipients of communications.</param>
		/// <param name="contentModel">Model for the communications</param>
		/// <param name="allowedChannels">Ids of channels that are allowed to be used with this dispatch.</param>
		/// <param name="cultureInfo">Culture ISO.</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Dispatch results</returns>
		Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IAudience> audiences, IContentModel contentModel, IReadOnlyCollection<string> allowedChannels, string? cultureInfo = null, CancellationToken cancellationToken = default);
		Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IAudienceAddress> audienceAddresses, IContentModel contentModel, IReadOnlyCollection<string> allowedChannels, string? cultureInfo = null, CancellationToken cancellationToken = default);
		Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, string audienceAddress, IContentModel contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default);
		Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, string audienceAddress, object contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default);
		Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<string> audienceAddresses, object contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default);
		Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IAudienceAddress> audienceAddresses, IContentModel contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default);
		Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IAudience> audiences, IContentModel contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default);

		void DeliverReport(DeliveryReport report);
		void DeliverReports(IReadOnlyCollection<DeliveryReport> reports);
	}
}