using Transmitly;

namespace eShop.Communications.API.OrderCreated;

/// <summary>
/// Code-defined SMS content for <c>OrderCreated</c>: short, with a link back to the order.
/// </summary>
internal static class OrderCreatedSms
{
    public static Task<string?> Message(IDispatchCommunicationContext context, Uri? webAppUrl)
    {
        var order = OrderCreatedContentModel.From(context.ContentModel);
        var message = order is null
            ? "We've received your eShop order."
            : $"We've received your eShop order #{order.OrderId}.";

        return Task.FromResult<string?>(OrderLinks.Orders(webAppUrl) is { } ordersLink
            ? $"{message}\nView your order: {ordersLink}"
            : message);
    }
}
