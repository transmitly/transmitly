﻿// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
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
            var contentModel = new ContentModel(context.ContentModel, filteredRecipients);

            var internalContext = new DispatchCommunicationContext(context, channel, provider)
            {
                PlatformIdentities = filteredRecipients,
                ContentModel = contentModel
            };

            var communication = await GetChannelCommunicationAsync(channel, internalContext).ConfigureAwait(false);

            IReadOnlyCollection<IDispatchResult?>? results = null;
            try
            {
                if (!provider.CommunicationType.IsInstanceOfType(communication))
                    return [];

                var client = Guard.AgainstNull(await provider.ClientInstance());
                results = await InvokeCommunicationTypedDispatchAsyncOnClient(provider, internalContext, communication, client, cancellationToken).ConfigureAwait(false);

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

        private static async Task<IReadOnlyCollection<IDispatchResult?>> InvokeCommunicationTypedDispatchAsyncOnClient(IChannelProvider provider, IDispatchCommunicationContext internalContext, object communication, IChannelProviderClient client, CancellationToken cancellationToken)
        {
            var method = typeof(IChannelProviderClient<>).MakeGenericType(provider.CommunicationType).GetMethod(nameof(IChannelProviderClient.DispatchAsync));
            Guard.AgainstNull(method);

            var comm = method.Invoke(client, [communication, internalContext, cancellationToken]);
            Guard.AgainstNull(comm);

            return await ((Task<IReadOnlyCollection<IDispatchResult?>>)comm).ConfigureAwait(false);
        }

        private static ReadOnlyCollection<PlatformIdentityRecord> FilterRecipientAddresses(IChannel channel, IReadOnlyCollection<IPlatformIdentity> platformIdentities)
        {
            return platformIdentities.Select(x =>
               new PlatformIdentityRecord(
                   x.Id,
                   x.Type,
                   x.Addresses.Where(a =>
                           channel.SupportsIdentityAddress(a)
                       )
                   )
               ).ToList().AsReadOnly();
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