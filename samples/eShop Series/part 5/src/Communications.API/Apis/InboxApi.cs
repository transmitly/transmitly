using System.Security.Claims;
using eShop.Communications.API.DeliveryReports;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eShop.Communications.API.Apis;

public static class InboxApi
{
    private const int PageSize = 50;

    public static IEndpointRouteBuilder MapInboxApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/communications/inbox", GetInboxAsync)
            .RequireAuthorization()
            .WithName("GetInbox");

        return endpoints;
    }

    /// <summary>
    /// The signed-in user's communications, newest first, with their latest delivery status.
    /// </summary>
    private static async Task<Results<Ok<List<InboxItem>>, UnauthorizedHttpResult>> GetInboxAsync(
        ClaimsPrincipal user,
        CommunicationsContext db,
        CancellationToken cancellationToken)
    {
        if (user.FindFirst("sub")?.Value is not { } recipientId)
        {
            return TypedResults.Unauthorized();
        }

        var items = await db.Communications
            .AsNoTracking()
            .Where(communication => communication.RecipientId == recipientId)
            .OrderByDescending(communication => communication.CreatedAt)
            .Take(PageSize)
            .Select(communication => new InboxItem(
                communication.Id,
                communication.PipelineIntent,
                communication.ChannelId,
                communication.Summary,
                communication.Status,
                communication.IsFailure,
                communication.CreatedAt,
                communication.UpdatedAt))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(items);
    }
}

public sealed record InboxItem(
    Guid Id,
    string Intent,
    string? Channel,
    string? Summary,
    string Status,
    bool IsFailure,
    DateTimeOffset SentAt,
    DateTimeOffset UpdatedAt);
