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

using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Transmitly
{
	/// <summary>
	/// Extends a <see cref="ICommunicationsClient"/>.
	/// </summary>
	[DebuggerStepThrough]
	public static class CommunicationsClientExtensions
	{
		/// <summary>
		/// Dispatches the communications for the provided pipeline name.
		/// </summary>
		/// <param name="client">Communications client.</param>
		/// <param name="pipelineName">Name of the pipeline.</param>
		/// <param name="platformIdentities">Potential recipients of communications.</param>
		/// <param name="transactionalModel">Model for the communications</param>
		/// <param name="cultureInfo">Culture ISO.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Dispatch results</returns>
		public static Task<IDispatchCommunicationResult> DispatchAsync(this ICommunicationsClient client, string pipelineName, IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities, ITransactionModel transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return client.DispatchAsync(pipelineName, platformIdentities, transactionalModel, [], cultureInfo, cancellationToken);
		}

		/// <summary>
		/// Dispatches the communications for the provided pipeline name.
		/// </summary>
		/// <param name="client">Communications client.</param>		
		/// <param name="pipelineName">Name of the pipeline.</param>
		/// <param name="identityAddress">Potential recipients of communications.</param>
		/// <param name="transactionalModel">Model for the communications.</param>
		/// <param name="cultureInfo">Culture ISO.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns></returns>
		public static Task<IDispatchCommunicationResult> DispatchAsync(this ICommunicationsClient client, string pipelineName, string identityAddress, object transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			var platformIdentities = WrapAsPlatformIdentityProfileCollection(identityAddress);
			return client.DispatchAsync(pipelineName, platformIdentities, TransactionModel.Create(transactionalModel), [], cultureInfo, cancellationToken);
		}

		/// <summary>
		/// Dispatches the communications for the provided pipeline name.
		/// </summary>
		/// <param name="client">Communications client.</param>
		/// <param name="pipelineName">Name of the pipeline.</param>
		/// <param name="identityAddress">Potential recipient of communications.</param>
		/// <param name="transactionalModel">Model for the communications</param>
		/// <param name="cultureInfo">Culture ISO.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Dispatch results</returns>
		public static Task<IDispatchCommunicationResult> DispatchAsync(this ICommunicationsClient client, string pipelineName, string identityAddress, ITransactionModel transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			var platformIdentities = WrapAsPlatformIdentityProfileCollection(identityAddress);
			return client.DispatchAsync(pipelineName, platformIdentities, transactionalModel, [], cultureInfo, cancellationToken);
		}

		/// <summary>
		/// Dispatches the communications for the provided pipeline name.
		/// </summary>
		/// <param name="client">Communications client.</param>
		/// <param name="pipelineName">Name of the pipeline.</param>
		/// <param name="identityAddresses">Potential recipients of communications.</param>
		/// <param name="transactionalModel">Model for the communications</param>
		/// <param name="cultureInfo">Culture ISO.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Dispatch results</returns>
		public static Task<IDispatchCommunicationResult> DispatchAsync(this ICommunicationsClient client, string pipelineName, IReadOnlyCollection<string> identityAddresses, object transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			Guard.AgainstNull(identityAddresses);
			var platformIdentities = identityAddresses.Select(x => (IPlatformIdentityProfile)WrapAsPlatformIdentityProfileCollection(x)).ToList().AsReadOnly();
			return client.DispatchAsync(pipelineName, platformIdentities, TransactionModel.Create(transactionalModel), [], cultureInfo, cancellationToken);
		}

		/// <summary>
		/// Dispatches the communications for the provided pipeline name.
		/// </summary>
		/// <param name="client">Communications client.</param>
		/// <param name="pipelineName">Name of the pipeline.</param>
		/// <param name="identityAddresses">Potential recipients of communications.</param>
		/// <param name="transactionalModel">Model for the communications</param>
		/// <param name="cultureInfo">Culture ISO.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Dispatch results</returns>
		public static Task<IDispatchCommunicationResult> DispatchAsync(this ICommunicationsClient client, string pipelineName, IReadOnlyCollection<IIdentityAddress> identityAddresses, ITransactionModel transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			Guard.AgainstNull(identityAddresses);
			var platformIdentities = identityAddresses.Select(x => (IPlatformIdentityProfile)new PlatformIdentityProfile(null, null, [x])).ToList().AsReadOnly();
			return client.DispatchAsync(pipelineName, platformIdentities, transactionalModel, [], cultureInfo, cancellationToken);
		}
		/// <summary>
		/// Dispatches the communications for the provided pipeline name.
		/// </summary>
		/// <param name="client">Communications client.</param>
		/// <param name="pipelineName">Name of the pipeline.</param>
		/// <param name="identityAddresses">Potential recipients of communications.</param>
		/// <param name="transactionalModel">Model for the communications</param>
		/// <param name="channelPreferences">Ids of channels preferences to use with channel and channel provider decisioning.</param>
		/// <param name="cultureInfo">Culture ISO.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Dispatch results</returns>
		public static Task<IDispatchCommunicationResult> DispatchAsync(this ICommunicationsClient client, string pipelineName, IReadOnlyCollection<IIdentityAddress> identityAddresses, ITransactionModel transactionalModel, IReadOnlyCollection<string> channelPreferences, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return client.DispatchAsync(pipelineName, [identityAddresses.AsPlatformIdentity()], transactionalModel, channelPreferences, cultureInfo, cancellationToken);
		}

		/// <summary>
		/// Dispatches the communications for the provided pipeline name.
		/// </summary>
		/// <param name="client">Communications client.</param>
		/// <param name="pipelineName">Name of the pipeline.</param>
		/// <param name="identityReferences">Potential recipients of communications.</param>
		/// <param name="transactionalModel">Model for the communications</param>
		/// <param name="cultureInfo">Culture ISO.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Dispatch results</returns>
		public static Task<IDispatchCommunicationResult> DispatchAsync(this ICommunicationsClient client, string pipelineName, IReadOnlyCollection<IPlatformIdentityReference> identityReferences, ITransactionModel transactionalModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return client.DispatchAsync(pipelineName, identityReferences, transactionalModel, [], cultureInfo, cancellationToken);
		}


		private static IReadOnlyCollection<IPlatformIdentityProfile> WrapAsPlatformIdentityProfileCollection(string identityAddress)
		{
			Guard.AgainstNullOrWhiteSpace(identityAddress);
			return new ReadOnlyCollection<IPlatformIdentityProfile>([new PlatformIdentityProfile(null, null, [identityAddress.AsIdentityAddress()])]);

		}
	}
}