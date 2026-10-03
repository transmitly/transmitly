using Transmitly;

namespace eShop.Communications.API.OrderCreated;

/// <summary>
/// The model the <c>OrderCreated</c> templates render. It combines Ordering's transactional
/// facts, the resolved recipient from Identity, and product presentation data from Catalog.
/// </summary>
public sealed record OrderCreatedContentModel(
    RecipientModel Recipient,
    int OrderId,
    DateTime OrderDate,
    decimal Total,
    IReadOnlyCollection<OrderCreatedContentItem> Items)
{
    /// <summary>
    /// Reads the enriched model from the content model a template receives. Returns
    /// <see langword="null"/> when <see cref="CatalogContentModelEnricher"/> hasn't run.
    /// </summary>
    public static OrderCreatedContentModel? From(IContentModel? contentModel)
    {
        // Ordering's transactional model has no recipient, so its presence marks the enriched model.
        if (contentModel?.Model is not IDictionary<string, object?> values
            || !values.ContainsKey(nameof(Recipient)))
        {
            return null;
        }

        string[] fields = [nameof(Recipient), nameof(OrderId), nameof(OrderDate), nameof(Total), nameof(Items)];
        return TransactionModelJson.Read<OrderCreatedContentModel>(
            fields.Where(values.ContainsKey).ToDictionary(field => field, field => values[field]));
    }
}

public sealed record RecipientModel(
    string? FirstName);

/// <param name="PurchasedUnitPrice">The price on the order. Catalog's current price is deliberately not used.</param>
/// <param name="ProductImage">Catalog's picture route for the product, relative to the Catalog API.</param>
public sealed record OrderCreatedContentItem(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal PurchasedUnitPrice,
    string? Brand,
    string? ProductType,
    string? Description,
    string? ProductImage);

/// <summary>
/// Wraps an enriched model so it replaces the current content model for the rest of the pipeline.
/// </summary>
internal sealed class EnrichedContentModel(object model, IContentModel previous) : IContentModel
{
    public object Model { get; } = model;
    public IReadOnlyList<Resource> Resources { get; } = previous.Resources;
    public IReadOnlyList<LinkedResource> LinkedResources { get; } = previous.LinkedResources;
}
