namespace eShop.ServiceDefaults.Communications;

/// <summary>
/// The transactional model Ordering supplies with an <see cref="CommunicationIntents.OrderCreated"/> dispatch.
/// It carries the order facts Ordering owns. Communications enriches it before rendering.
/// </summary>
public sealed record OrderCreatedModel(
    int OrderId,
    DateTime OrderDate,
    IReadOnlyCollection<OrderCreatedItem> Items,
    decimal Total);

public sealed record OrderCreatedItem(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
