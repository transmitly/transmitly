using System.Text.Json;

namespace eShop.Communications.API;

/// <summary>
/// Moves models between JSON and the dictionary shape Transmitly uses for content models.
/// </summary>
internal static class TransactionModelJson
{
    /// <summary>
    /// Converts the JSON model received over the dispatch endpoint into dictionaries, lists, and
    /// primitives. Transmitly exposes dictionary entries to templates and enrichers as fields,
    /// while a raw <see cref="JsonElement"/> would only expose its own members.
    /// </summary>
    public static object? ToModel(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => element.EnumerateObject()
            .ToDictionary(property => property.Name, property => ToModel(property.Value), StringComparer.Ordinal),
        JsonValueKind.Array => element.EnumerateArray().Select(ToModel).ToList(),
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.TryGetInt64(out var number) ? number : element.GetDecimal(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null
    };

    /// <summary>
    /// Reads a typed model back out of Transmitly's dynamic content model.
    /// </summary>
    public static T? Read<T>(object? value) =>
        value is null ? default : JsonSerializer.SerializeToElement(value).Deserialize<T>(JsonSerializerOptions.Web);
}
