using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transmitly;

namespace eShop.Communications.API.DeliveryReports;

/// <summary>
/// The webhook channel providers call with delivery updates. Each provider's request adaptor
/// recognizes its own requests and turns them into Transmitly delivery reports, which reach
/// the same handler as the reports raised at dispatch.
/// </summary>
[AllowAnonymous]
[Route("api/communications/delivery-reports")]
public sealed class DeliveryReportsController(ICommunicationsClient communicationsClient)
    : ChannelProviderDeliveryReportController(communicationsClient);
