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

    public static Task<string?> Title(IDispatchCommunicationContext context) =>
        Task.FromResult<string?>("Thanks for your order");

    public static Task<string?> Body(IDispatchCommunicationContext context) =>
        Task.FromResult<string?>(
            OrderCreatedContentModel.From(context.ContentModel) is { } order
                ? $"Order #{order.OrderId} has been received."
                : "Your order has been received.");

    public static Task<string?> OrderId(IDispatchCommunicationContext context) =>
        Task.FromResult(OrderCreatedContentModel.From(context.ContentModel)?.OrderId.ToString());
}
