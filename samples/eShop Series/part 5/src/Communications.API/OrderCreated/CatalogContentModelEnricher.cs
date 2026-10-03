using System.Text.Json;
using eShop.ServiceDefaults.Communications;
using Transmitly;
using Transmitly.Model.Configuration;

namespace eShop.Communications.API.OrderCreated;

/// <summary>
/// Builds the <see cref="OrderCreatedContentModel"/> from Ordering's transactional model,
/// the resolved recipient, and product presentation data from Catalog.
/// </summary>
public sealed class CatalogContentModelEnricher(
    HttpClient httpClient,
    ILogger<CatalogContentModelEnricher> logger) : IContentModelEnricher
{
    // Transmitly keeps the dispatched transactional model under this key in the content model.
    private const string TransactionModelKey = "trx";

    public async Task<IContentModel?> EnrichAsync(
        IDispatchCommunicationContext context,
        IContentModel currentModel,
        CancellationToken cancellationToken = default)
    {
        var order = ReadTransactionModel(currentModel);
        if (order is null)
        {
            logger.LogWarning("The {Intent} dispatch did not include an order model; content was not enriched.", context.PipelineIntent);
            return null;
        }

        var productIds = order.Items
            .Select(x => x.ProductId)
            .Distinct()
            .ToArray();

        var catalog = await GetCatalogAsync(productIds, cancellationToken);

        var items = order.Items
            .Select(item =>
            {
                var product = catalog.Items.GetValueOrDefault(item.ProductId);
                return new OrderCreatedContentItem(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    // The order keeps what the customer paid. Catalog's current price means something else.
                    item.UnitPrice,
                    product is null ? null : catalog.Brands.GetValueOrDefault(product.CatalogBrandId),
                    product is null ? null : catalog.Types.GetValueOrDefault(product.CatalogTypeId),
                    product?.Description,
                    product is null ? null : $"api/catalog/items/{product.Id}/pic");
            })
            .ToArray();

        var content = new OrderCreatedContentModel(
            new RecipientModel(GetFirstName(context.PlatformIdentities)),
            order.OrderId,
            order.OrderDate,
            order.Total,
            items);

        // Transmitly merges this into its dynamic content model, so templates see each
        // property of OrderCreatedContentModel as a top-level field.
        return new EnrichedContentModel(content, currentModel);
    }

    private static OrderCreatedModel? ReadTransactionModel(IContentModel currentModel)
    {
        if (currentModel.Model is not IDictionary<string, object?> model
            || !model.TryGetValue(TransactionModelKey, out var transactionModel)
            || transactionModel is null)
        {
            return null;
        }

        return TransactionModelJson.Read<OrderCreatedModel>(transactionModel);
    }

    private static string? GetFirstName(IReadOnlyCollection<IPlatformIdentityProfile> recipients) =>
        recipients.FirstOrDefault() is CustomerIdentityProfile profile
            ? profile.FirstName ?? profile.DisplayName
            : null;

    private async Task<CatalogData> GetCatalogAsync(int[] productIds, CancellationToken cancellationToken)
    {
        if (productIds.Length == 0)
        {
            return CatalogData.Empty;
        }

        try
        {
            // One batch call for the products, plus Catalog's brand and type lists for their names.
            var itemsTask = httpClient.GetFromJsonAsync<CatalogItem[]>(
                $"api/catalog/items/by?ids={string.Join("&ids=", productIds)}", cancellationToken);
            var brandsTask = httpClient.GetFromJsonAsync<CatalogBrand[]>("api/catalog/catalogbrands", cancellationToken);
            var typesTask = httpClient.GetFromJsonAsync<CatalogType[]>("api/catalog/catalogtypes", cancellationToken);

            await Task.WhenAll(itemsTask, brandsTask, typesTask);

            return new CatalogData(
                (await itemsTask ?? []).ToDictionary(item => item.Id),
                (await brandsTask ?? []).ToDictionary(brand => brand.Id, brand => brand.Brand),
                (await typesTask ?? []).ToDictionary(type => type.Id, type => type.Type));
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            // Catalog adds context; it doesn't define the order. Render without it rather than fail.
            logger.LogWarning(ex, "Catalog data was unavailable; the order communication is rendered without product details.");
            return CatalogData.Empty;
        }
    }

    private sealed record CatalogData(
        IReadOnlyDictionary<int, CatalogItem> Items,
        IReadOnlyDictionary<int, string> Brands,
        IReadOnlyDictionary<int, string> Types)
    {
        public static CatalogData Empty { get; } = new(
            new Dictionary<int, CatalogItem>(),
            new Dictionary<int, string>(),
            new Dictionary<int, string>());
    }

    private sealed record CatalogItem(int Id, string Name, string? Description, int CatalogTypeId, int CatalogBrandId);

    private sealed record CatalogBrand(int Id, string Brand);

    private sealed record CatalogType(int Id, string Type);
}
