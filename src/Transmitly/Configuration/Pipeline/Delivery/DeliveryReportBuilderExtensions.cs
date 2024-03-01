using Transmitly.ChannelProvider;

namespace Transmitly
{
	public static class DeliveryReportBuilderExtensions
	{
		public static CommunicationsClientBuilder AddDeliveryReportHandler(this CommunicationsClientBuilder communicationsClientBuilder, IObserver<DeliveryReport> reportHandler, IReadOnlyCollection<string>? filterEventNames = null, IReadOnlyCollection<string>? channelIds = null, IReadOnlyCollection<string>? channelProviderIds = null)
		{
			return communicationsClientBuilder.DeliveryReport.AddDeliveryReportHandler(reportHandler, filterEventNames, channelIds, channelProviderIds);
		}

		public static CommunicationsClientBuilder AddDeliveryReportHandler(this CommunicationsClientBuilder communicationsClientBuilder, DeliveryReportAsyncHandler reportHandler, IReadOnlyCollection<string>? filterEventNames = null, IReadOnlyCollection<string>? filterChannelIds = null, IReadOnlyCollection<string>? filterChannelProviderIds = null)
		{
			return communicationsClientBuilder.DeliveryReport.AddDeliveryReportHandler(reportHandler, filterEventNames, filterChannelIds, filterChannelProviderIds);
		}
	}
}
