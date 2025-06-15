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

using Transmitly.Channel.Configuration;
using Transmitly.ChannelProvider;

namespace Transmitly.Delivery;

/// <summary>
/// Base pipeline delivery strategy provider
/// </summary>
public abstract class BasePipelineDeliveryStrategyProvider
{
	/// <summary>
	/// Dispatches the communications(s) through the provided channel channel provider groups.
	/// </summary>
	/// <param name="sendingGroups">Channel/Channel provider groupings.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Collection of dispatch results.</returns>
	public abstract Task<IDispatchCommunicationResult> DispatchAsync(IReadOnlyCollection<RecipientDispatchCommunicationContext> sendingGroups, CancellationToken cancellationToken);

	/// <summary>
	/// Dispatches the communication(s) through the provided channel and with the provided channel provider.
	/// </summary>
	/// <param name="channel">Communication channel.</param>
	/// <param name="provider">Channel provider used for dispatching.</param>
	/// <param name="recipientContext"></param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Collection of dispatch results.</returns>
	protected async Task<IReadOnlyCollection<IDispatchResult?>> DispatchCommunicationAsync(IChannel channel, IChannelProvider provider, RecipientDispatchCommunicationContext recipientContext, CancellationToken cancellationToken)
	{
		var context = recipientContext.Context;

		var filteredProfiles = FilterToASingleIdentityWithSingleSupportedAddress(channel, context);

		var contentModel = new ContentModel(context.TransactionModel, filteredProfiles);
		var dispatchingContext = new DispatchCommunicationContext(contentModel,
			context.ChannelConfiguration,
			filteredProfiles,
			context.TemplateEngine,
			context.DeliveryReportManager,
			context.CultureInfo,
			context.PipelineIntent,
			context.PipelineId,
			context.MessagePriority,
			context.TransportPriority,
			channel.Id,
			provider.Id);


		var communication = await GetChannelCommunicationAsync(channel, dispatchingContext).ConfigureAwait(false);

		var dispatchResult = await DispatchCommunicationInternal(channel, provider, dispatchingContext, dispatchingContext.ContentModel, communication, cancellationToken).ConfigureAwait(false);

		if (dispatchResult.Count == 0 || dispatchResult.All(r => r == null))
			return Array.Empty<IDispatchResult?>();

		return dispatchResult;
	}

	private static List<IPlatformIdentityProfile> FilterToASingleIdentityWithSingleSupportedAddress(IChannel channel, IInternalDispatchCommunicationContext context)
	{
		//The dispatch client is temporarily only allowing a single platform identity per context.
		//Ensure that is the case until it's eventually changed.
		if (context.PlatformIdentities.Count > 1)
			throw new NotSupportedException("Only a single platform identity is currently supported.");

		//Temporarily only use the first supported address for the provided channel
		var singlePlatformProfile = context.PlatformIdentities.FirstOrDefault();
		var identityProfiles = new List<IPlatformIdentityProfile>();
		if (singlePlatformProfile != null)
		{
			var matchingAddress = singlePlatformProfile.Addresses.FirstOrDefault(a => channel.SupportsIdentityAddress(a));
			IPlatformIdentityProfile profile;
			if (matchingAddress != null)
				profile = new PlatformIdentityProfileProxy(singlePlatformProfile, [matchingAddress]);
			else
				profile = new PlatformIdentityProfileProxy(singlePlatformProfile, []);

			identityProfiles.Add(profile);
		}

		return identityProfiles;
	}

	private static async Task<IReadOnlyCollection<IDispatchResult?>> DispatchCommunicationInternal(IChannel channel, IChannelProvider provider, DispatchCommunicationContext context, IContentModel? contentModel, object communication, CancellationToken cancellationToken)
	{
		IReadOnlyCollection<IDispatchResult?>? results = null;
		try
		{
			if (!provider.CommunicationType.IsInstanceOfType(communication))
				return [];

			var client = Guard.AgainstNull(await provider.DispatcherInstance());
			results = await InvokeCommunicationTypedDispatchAsyncOnDispatcher(provider, context, communication, client, cancellationToken).ConfigureAwait(false);

			if (results == null || results.Count == 0)
				return [];

			return results.Where(r => r != null).Select(r => new DispatchResult(r!, provider.Id, channel.Id)
			{
				PipelineIntent = context.PipelineIntent,
				PipelineId = context.PipelineId
			}).ToList();
		}
		catch (Exception ex)
		{
			if (results != null)
			{
				var reports = results.Select(r =>
					new DeliveryReport(
						DeliveryReport.Event.Error(),
						channel.Id,
						provider.Id,
						context.PipelineIntent,
						context.PipelineId,
						r!.ResourceId,
						r.Status,
						communication,
						contentModel,
						r.Exception
					)
				).ToList();
				await context.DeliveryReportManager.DispatchAsync(reports);
			}
			return [new DispatchResult(CommunicationsStatus.ClientError("Channel Provider Exception"), provider.Id, channel.Id) { Exception = ex }];
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
				a.Any(x => x != null && x.Status.IsSuccess())
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