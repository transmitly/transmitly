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
	/// Client for dispatching communications.
	/// </summary>
	public interface ICommunicationsClient
	{
		/// <summary>
		/// Dispatches the communications for the provided communication intent id and optional pipeline name.
		/// </summary>
		/// <param name="communicationIntentId">Id of the intent of the pipeline.</param>
		/// <param name="pipelineName">Name of the pipeline.
		/// <para>Must be unique per <paramref name="communicationIntentId"/></para></param>
		/// <param name="platformIdentities">Potential recipients of communications.</param>
		/// <param name="transactionalModel">Model for the communications</param>
		/// <param name="channelPreferences">Ids of channels preferences to use with channel and channel provider decisioning.</param>
		/// <param name="cultureInfo">Culture ISO.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Dispatch results</returns>
		Task<IDispatchCommunicationResult> DispatchAsync(string communicationIntentId, IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities, ITransactionModel transactionalModel, IReadOnlyCollection<string> channelPreferences, string? pipelineName = null, string? cultureInfo = null, CancellationToken cancellationToken = default);
		/// <summary>
		/// Dispatches the communications for the provided communication intent id and optional pipeline name.
		/// </summary>
		/// <param name="communicationIntentId">Id of the intent of the pipeline.</param>
		/// <param name="pipelineName">Name of the pipeline.
		/// <para>Must be unique per <paramref name="communicationIntentId"/></para></param>
		/// <param name="identityReferences">Potential recipients of communications.</param>
		/// <param name="transactionalModel">Model for the communications</param>
		/// <param name="channelPreferences">Ids of channels preferences to use with channel and channel provider decisioning.</param>
		/// <param name="cultureInfo">Culture ISO.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Dispatch results</returns>
		Task<IDispatchCommunicationResult> DispatchAsync(string communicationIntentId, IReadOnlyCollection<IPlatformIdentityReference> identityReferences, ITransactionModel transactionalModel, IReadOnlyCollection<string> channelPreferences, string? pipelineName = null, string? cultureInfo = null, CancellationToken cancellationToken = default);
		/// <summary>
		/// Delivers a single delivery report.
		/// </summary>
		/// <param name="report">The delivery report to deliver.</param>
		void DeliverReport(DeliveryReport report);
		/// <summary>
		/// Delivers multiple delivery reports.
		/// </summary>
		/// <param name="reports">The collection of delivery reports to deliver.</param>
		void DeliverReports(IReadOnlyCollection<DeliveryReport> reports);
	}
}