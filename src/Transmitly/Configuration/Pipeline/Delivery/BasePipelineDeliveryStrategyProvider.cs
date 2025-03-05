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

using System.Dynamic;
using System.Reflection;
using Transmitly.Channel.Configuration;
using Transmitly.ChannelProvider;

namespace Transmitly.Delivery
{
	/// <summary>
	/// Base pipeline delivery strategy provider
	/// </summary>
	public abstract class BasePipelineDeliveryStrategyProvider
	{
		/// <summary>
		/// List of <see cref="DispatchStatus"/> that will evaluate to a successful result.
		/// </summary>
		protected virtual IReadOnlyCollection<DispatchStatus> SuccessfulStatuses { get; } = [DispatchStatus.Delivered, DispatchStatus.Dispatched, DispatchStatus.Pending];

		/// <summary>
		/// Dispatches the communications(s) through the provided channel channel provider groups.
		/// </summary>
		/// <param name="sendingGroups">Channel/Channel provider groupings.</param>
		/// <param name="context">Context of the dispatch operation.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Collection of dispatch results.</returns>
		public abstract Task<IDispatchCommunicationResult> DispatchAsync(IReadOnlyCollection<ChannelChannelProviderGroup> sendingGroups, IDispatchCommunicationContext context, CancellationToken cancellationToken);

		/// <summary>
		/// Dispatches the communication(s) through the provided channel and with the provided channel provider.
		/// </summary>
		/// <param name="channel">Communication channel.</param>
		/// <param name="provider">Channel provider used for dispatching.</param>
		/// <param name="context">Context of the dispatch operation.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Collection of dispatch results.</returns>
		protected async Task<IReadOnlyCollection<IDispatchResult?>> DispatchCommunicationAsync(IChannel channel, IChannelProvider provider, IDispatchCommunicationContext context, CancellationToken cancellationToken)
		{
			var filteredRecipients = FilterRecipientAddresses(channel, context.PlatformIdentities);
			var allResults = new List<IDispatchResult?>();
			foreach (var recipient in filteredRecipients)
			{
				var contentModel = new ContentModel(context.ContentModel, filteredRecipients);

				var internalContext = new DispatchCommunicationContext(context, channel, provider)
				{
					PlatformIdentities = [recipient],
					ContentModel = contentModel
				};

				var communication = await GetChannelCommunicationAsync(channel, internalContext).ConfigureAwait(false);

				var dispatchResult = await DispatchCommunicationInternal(channel, provider, context, contentModel, internalContext, communication, cancellationToken).ConfigureAwait(false);

				allResults.AddRange(dispatchResult);
			}

			if (allResults.Count == 0 || allResults.All(r => r == null))
				return Array.Empty<IDispatchResult?>();

			return allResults;
		}

		private static async Task<IReadOnlyCollection<IDispatchResult?>> DispatchCommunicationInternal(IChannel channel, IChannelProvider provider, IDispatchCommunicationContext context, ContentModel contentModel, DispatchCommunicationContext internalContext, object communication, CancellationToken cancellationToken)
		{
			IReadOnlyCollection<IDispatchResult?>? results = null;
			try
			{
				if (!provider.CommunicationType.IsInstanceOfType(communication))
					return [];

				var client = Guard.AgainstNull(await provider.DispatcherInstance());
				results = await InvokeCommunicationTypedDispatchAsyncOnDispatcher(provider, internalContext, communication, client, cancellationToken).ConfigureAwait(false);

				if (results == null || results.Count == 0)
					return [];

				return results.Where(r => r != null).Select(r => new DispatchResult(r!, provider.Id, channel.Id)).ToList();
			}
			catch (Exception ex)
			{
				if (results != null)
				{
					var reports = results.Select(r =>
						new DeliveryReport(
							DeliveryReport.Event.Error(),
							internalContext.ChannelId,
							internalContext.ChannelProviderId,
							context.PipelineName,
							r!.ResourceId,
							r.DispatchStatus,
							communication,
							contentModel,
							r.Exception
						)
					).ToList();
					context.DeliveryReportManager.DispatchReports(reports);
				}
				return [new DispatchResult(DispatchStatus.Exception, provider.Id, channel.Id) { Exception = ex }];
			}
		}

		/// <summary>
		/// Gets if the pipeline dispatch results are considered successful.
		/// </summary>
		/// <param name="allResults">Results to evaluate.</param>
		/// <returns>Whether the pipeline is considered successful.</returns>
		protected virtual bool IsPipelineSuccessful(IReadOnlyCollection<IDispatchResult?> allResults)
		{
			return allResults
				.GroupBy(g => g?.ChannelId)
				.All(a =>
					a.Any(x => x != null && SuccessfulStatuses.Contains(x.DispatchStatus))
				);
		}

		/// <summary>
		/// Executes the DispatchAsync for the matching communication type of the channel provider dispatcher.
		/// </summary>
		/// <param name="provider">Channel provider.</param>
		/// <param name="internalContext">Internal dispatch context.</param>
		/// <param name="communication">The communication to dispatch.</param>
		/// <param name="dispatcher">The channel provider dispatcher.</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>Collection of dispatch results.</returns>
		private static async Task<IReadOnlyCollection<IDispatchResult?>> InvokeCommunicationTypedDispatchAsyncOnDispatcher(IChannelProvider provider, IDispatchCommunicationContext internalContext, object communication, IChannelProviderDispatcher dispatcher, CancellationToken cancellationToken)
		{
			var method = typeof(IChannelProviderDispatcher<>).MakeGenericType(provider.CommunicationType).GetMethod(nameof(IChannelProviderDispatcher.DispatchAsync));
			Guard.AgainstNull(method);

			var comm = method.Invoke(dispatcher, [communication, internalContext, cancellationToken]);
			Guard.AgainstNull(comm);

			return await ((Task<IReadOnlyCollection<IDispatchResult?>>)comm).ConfigureAwait(false);
		}

		/// <summary>
		/// Filters the available platform identities to those supported by the provided channel.
		/// </summary>
		/// <param name="channel">The channel to use for filtering.</param>
		/// <param name="platformIdentities">Collection of identities to filter.</param>
		/// <returns>Filtered collection of <see cref="PlatformIdentityRecord"/>.</returns>
		protected virtual IReadOnlyCollection<IPlatformIdentity> FilterRecipientAddresses(IChannel channel, IReadOnlyCollection<IPlatformIdentity> platformIdentities)
		{
			return platformIdentities.Select(x =>
				FilterPlatformIdentityAddresses(x, x.Addresses.Where(a => channel.SupportsIdentityAddress(a)))
			).ToList().AsReadOnly();
		}

		/// <summary>
		/// Since the platform Identity does not allow updating the addresses we need to wrap the provided 
		/// platform identity so we don't lose the underlying properties.
		/// </summary>
		/// <param name="source">Source Platform Identity.</param>
		/// <param name="filteredAddresses">Collection of address that have been filtered.</param>
		/// <returns>Wrapped platform identity</returns>
		private static WrappedPlatformIdentity FilterPlatformIdentityAddresses(IPlatformIdentity source, IEnumerable<IIdentityAddress> filteredAddresses)
		{
			var expando = new ExpandoObject();
			var dict = (IDictionary<string, object>)expando!;

			foreach (var prop in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var value = prop.GetValue(source);
				dict[prop.Name] = value!;
			}

			dict[nameof(IPlatformIdentity.Addresses)] = filteredAddresses.ToList().AsReadOnly();

			return new WrappedPlatformIdentity(expando);
		}

		/// <summary>
		/// Gets the communication as rendered by the provided channel.
		/// </summary>
		/// <param name="channel">Channel to render the communication with.</param>
		/// <param name="context">Context of the dispatch operation.</param>
		/// <returns>Channel communication</returns>
		protected virtual async Task<object> GetChannelCommunicationAsync(IChannel channel, IDispatchCommunicationContext context)
		{
			return await channel.GenerateCommunicationAsync(context);
		}

		
	}
}