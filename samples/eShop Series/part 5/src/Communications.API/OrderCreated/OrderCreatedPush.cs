using Transmitly;

namespace eShop.Communications.API.OrderCreated;

/// <summary>
/// Code-defined push content for <c>OrderCreated</c>: a brief alert plus the data an app
/// needs to open the order.
/// </summary>
internal static class OrderCreatedPush
{
    public const string ActionKey = "action";
    public const string OpenOrderAction = "open-order";
    public const string OrderIdKey = "orderId";

    public static string Title(IDispatchCommunicationContext context) => "Thanks for your order";

    public static string Body(IDispatchCommunicationContext context) =>
        OrderCreatedContentModel.From(context.ContentModel) is { } order
            ? $"Order #{order.OrderId} has been received."
            : "Your order has been received.";

    public static string? OrderId(IDispatchCommunicationContext context) =>
        OrderCreatedContentModel.From(context.ContentModel)?.OrderId.ToString();
}
