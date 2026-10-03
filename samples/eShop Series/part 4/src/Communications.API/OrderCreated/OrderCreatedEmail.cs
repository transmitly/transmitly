using System.Globalization;
using System.Text;
using Transmitly;

namespace eShop.Communications.API.OrderCreated;

/// <summary>
/// Code-defined email content for <c>OrderCreated</c>, rendered from the enriched content model.
/// </summary>
internal static class OrderCreatedEmail
{
    // eShop prices are in US dollars.
    private static readonly CultureInfo PriceCulture = CultureInfo.GetCultureInfo("en-US");

    public static string Subject(IDispatchCommunicationContext context) =>
        OrderCreatedContentModel.From(context.ContentModel) is { } order
            ? $"Thanks for your order #{order.OrderId}"
            : "Thanks for your order!";

    public static string TextBody(IDispatchCommunicationContext context, Uri? webAppUrl)
    {
        if (OrderCreatedContentModel.From(context.ContentModel) is not { } order)
        {
            // Without the enriched model, fall back to the bare Part 2 message.
            return "We've received your order and we'll let you know when it ships.";
        }

        var text = new StringBuilder();
        text.AppendLine($"Hi {order.Recipient.FirstName ?? "there"},");
        text.AppendLine();
        text.AppendLine("We've received your order. We'll let you know when it ships.");
        text.AppendLine();

        foreach (var item in order.Items)
        {
            text.AppendLine($"{item.Quantity} × {item.ProductName}");

            if (!string.IsNullOrWhiteSpace(item.Brand))
            {
                text.AppendLine($"    {item.Brand}");
            }

            var price = item.PurchasedUnitPrice.ToString("C", PriceCulture);
            text.AppendLine(item.Quantity > 1 ? $"    {price} each" : $"    {price}");
            text.AppendLine();
        }

        text.AppendLine($"Order total: {order.Total.ToString("C", PriceCulture)}");

        if (OrderLinks.Orders(webAppUrl) is { } ordersLink)
        {
            text.AppendLine();
            text.AppendLine("View your order:");
            text.AppendLine(ordersLink);
        }

        return text.ToString();
    }
}
