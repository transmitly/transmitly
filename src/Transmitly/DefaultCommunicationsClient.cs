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
using Transmitly.ChannelProvider.Configuration;
using Transmitly.Delivery;
using Transmitly.Exceptions;
using Transmitly.Pipeline.Configuration;
using Transmitly.Template.Configuration;

namespace Transmitly
{
	public sealed class DefaultCommunicationsClient(
		IPipelineFactory pipelineRegistrations,
		IChannelProviderFactory channelProviderRegistrations,
		ITemplateEngineFactory templateEngineRegistrations,
		IDeliveryReportReporter deliveryReportHandler//,
													 //IAudienceResolverRegistrationStore audienceResolvers
		) : ICommunicationsClient
	{
		private readonly IPipelineFactory _pipelineRegistrations = Guard.AgainstNull(pipelineRegistrations);
		private readonly IChannelProviderFactory _channelProviderRegistrations = Guard.AgainstNull(channelProviderRegistrations);
		private readonly ITemplateEngineFactory _templateEngineRegistrations = Guard.AgainstNull(templateEngineRegistrations);
		private readonly IDeliveryReportReporter _deliveryReportProvider = Guard.AgainstNull(deliveryReportHandler);

		//private readonly IAudienceResolverRegistrationStore _audienceResolvers = Guard.AgainstNull(audienceResolvers);

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IAudienceAddress> audienceAddresses, IContentModel contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			Guard.AgainstNull(audienceAddresses);
			//Audience concepts are not available for v1. Eventually this will serve as a way to identify legacy calls to send transactional emails
			var audience = new AudienceRecord[] {
				new(audienceAddresses) {
					Type = "Unknown:" + pipelineName,
					Id = string.Join(";", audienceAddresses.Select(x => x.Value))
				}
			};
			return await DispatchAsync(pipelineName, audience, contentModel, cultureInfo, cancellationToken).ConfigureAwait(false);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, string audienceAddress, object contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			Guard.AgainstNullOrWhiteSpace(audienceAddress);

			return await DispatchAsync(pipelineName, [audienceAddress], contentModel, cultureInfo, cancellationToken).ConfigureAwait(false);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<string> audienceAddresses, object contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			Guard.AgainstNull(audienceAddresses);

			return await DispatchAsync(pipelineName, audienceAddresses.Select(x => (IAudienceAddress)new AudienceAddress(x)).ToList(), ContentModel.Create(contentModel), cultureInfo, cancellationToken).ConfigureAwait(false);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, string audienceAddress, IContentModel contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{

			return await DispatchAsync(pipelineName, new AudienceAddress[] { new(audienceAddress) }, contentModel, cultureInfo, cancellationToken).ConfigureAwait(false);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IAudience> audiences, IContentModel contentModel, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return await DispatchAsync(pipelineName, audiences, contentModel, [], cultureInfo, cancellationToken).ConfigureAwait(false);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IAudienceAddress> audienceAddresses, IContentModel contentModel, IReadOnlyCollection<string> allowedChannels, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			return await DispatchAsync(pipelineName, [audienceAddresses.AsAudience()], contentModel, allowedChannels, cultureInfo, cancellationToken).ConfigureAwait(false);
		}

		public async Task<IDispatchCommunicationResult> DispatchAsync(string pipelineName, IReadOnlyCollection<IAudience> audiences, IContentModel contentModel, IReadOnlyCollection<string> allowedChannels, string? cultureInfo = null, CancellationToken cancellationToken = default)
		{
			Guard.AgainstNullOrWhiteSpace(pipelineName);
			Guard.AgainstNull(contentModel);
			Guard.AgainstNull(audiences);
			var culture = GuardCulture.AgainstNull(cultureInfo);

			var pipeline = await _pipelineRegistrations.GetAsync(pipelineName).ConfigureAwait(false) ??
				throw new CommunicationsException($"A communication pipeline named, '{pipelineName}', has not been registered.");

			var pipelineConfiguration = pipeline.ChannelConfiguration;

			var deliveryStrategy = pipeline.ChannelConfiguration.PipelineDeliveryStrategyProvider;

			var context = new DispatchCommunicationContext(
				contentModel,
				pipelineConfiguration,
				audiences,
				_templateEngineRegistrations.Get(),
				_deliveryReportProvider,
				culture,
				pipelineName,
				pipeline.MessagePriority,
				pipeline.TransportPriority);


			var channelProviders = await _channelProviderRegistrations.GetAllAsync().ConfigureAwait(false);
			var groups = await CreateChannelChannelProviderGroupsAsync(_channelProviderRegistrations, pipelineConfiguration.Channels, channelProviders, allowedChannels, audiences);
			if (groups.Count == 0)
				return new DispatchCommunicationResult([], true);

			var result = await deliveryStrategy.DispatchAsync(groups, context, cancellationToken).ConfigureAwait(false);

			return result;
		}

		private static async Task<IReadOnlyCollection<ChannelChannelProviderGroup>> CreateChannelChannelProviderGroupsAsync(
			IChannelProviderFactory channelProviderFactory,
			IReadOnlyCollection<IChannel> configuredChannels,
			IReadOnlyCollection<IChannelProviderRegistration> allowedChannelProviders,
			IReadOnlyCollection<string> allowedChannels,
			IReadOnlyCollection<IAudience> audiences)
		{
			return (await Task.WhenAll(configuredChannels.Select(c =>
			{
				var audienceAddresses = audiences.SelectMany(m => m.Addresses);
				var channelProviders = allowedChannelProviders.Where(x =>
							(allowedChannels.Count == 0 || allowedChannels.Any(a => c.Id == a)) &&
							(!c.AllowedChannelProviderIds.Any() || c.AllowedChannelProviderIds.Contains(x.Id)) &&
							x.SupportsChannel(c.Id) &&
							(x.CommunicationType == typeof(object) || c.CommunicationType == x.CommunicationType) &&
							audienceAddresses.Any(a => c.SupportsAudienceAddress(a))
						).Select(m => new ChannelProviderEntity(m, async () => await channelProviderFactory.ResolveClientAsync(m)))
						.ToList();

				return Task.FromResult(new ChannelChannelProviderGroup(
						c,
						channelProviders
					));

			})))
			.Where(x => x.ChannelProviderClients.Count != 0)
			.ToList();
		}

		public void DeliverReport(DeliveryReport report)
		{
			Guard.AgainstNull(report);
			_deliveryReportProvider.DispatchReport(report);
		}

		public void DeliverReports(IReadOnlyCollection<DeliveryReport> reports)
		{
			Guard.AgainstNull(reports);
			foreach (var report in reports)
				_deliveryReportProvider.DispatchReport(report);
		}
	}
}