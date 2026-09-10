using eShop.ServiceDefaults.Communications;
using Microsoft.AspNetCore.Http.HttpResults;
using Transmitly;

namespace eShop.Communications.API.Apis;

public static class CommunicationsApi
{
    public static IEndpointRouteBuilder MapCommunicationsApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/communications/dispatch", DispatchAsync)
            .WithName("DispatchCommunication");

        return endpoints;
    }

    private static async Task<Results<Ok<CommunicationDispatchResponse>, BadRequest<CommunicationDispatchResponse>>> DispatchAsync(
        CommunicationDispatchRequest request,
        ICommunicationsClient communicationsClient,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Intent) || request.Recipients.Count == 0)
        {
            return TypedResults.BadRequest(new CommunicationDispatchResponse(
                false,
                0,
                request.CorrelationId));
        }

        var model = TransactionModel.Create(request.Model);
        var references = request.Recipients
            .Where(recipient =>
                !string.IsNullOrWhiteSpace(recipient.Id))
            .Select(recipient =>
                (IPlatformIdentityReference)new IdentityReference(recipient.Type ?? "User", recipient.Id!))
            .ToArray();

        if (references.Length != request.Recipients.Count)
        {
            return TypedResults.BadRequest(new CommunicationDispatchResponse(
                false,
                0,
                request.CorrelationId));
        }

        var result = await communicationsClient.DispatchAsync(
            request.Intent,
            references,
            model,
            request.AllowedChannels,
            request.PipelineId,
            request.Culture,
            cancellationToken);

        var response = new CommunicationDispatchResponse(
            result.IsSuccessful,
            result.Results.Count,
            request.CorrelationId);

        return result.IsSuccessful
            ? TypedResults.Ok(response)
            : TypedResults.BadRequest(response);
    }
}
