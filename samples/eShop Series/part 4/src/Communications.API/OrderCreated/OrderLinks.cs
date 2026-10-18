namespace eShop.Communications.API.OrderCreated;

internal static class OrderLinks
{
    // eShop has no page per order, so links go to the buyer's order list.
    public static string? Orders(Uri? webAppUrl) =>
        webAppUrl is null ? null : new Uri(webAppUrl, "user/orders").ToString();
}
