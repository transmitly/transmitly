namespace eShop.WebApp.Services;

/// <summary>
/// Reads the signed-in buyer's communications from the Communications service.
/// </summary>
public class InboxService(HttpClient httpClient)
{
    private readonly string remoteServiceBaseUrl = "/api/communications/inbox";

    public Task<InboxMessage[]> GetMessages()
    {
        return httpClient.GetFromJsonAsync<InboxMessage[]>(remoteServiceBaseUrl)!;
    }
}

public record InboxMessage(
    Guid Id,
    string Intent,
    string? Channel,
    string? Summary,
    string Status,
    bool IsFailure,
    DateTimeOffset SentAt,
    DateTimeOffset UpdatedAt);
