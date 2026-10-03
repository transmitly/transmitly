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
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var correlationId = request.Metadata.CorrelationId;
        var logger = loggerFactory.CreateLogger(typeof(CommunicationsApi));
        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["DispatchSource"] = request.Metadata.Source,
            ["DispatchedAt"] = request.Metadata.DispatchedAt
        });

        if (string.IsNullOrWhiteSpace(request.Intent) || request.Recipients.Count == 0)
        {
            return TypedResults.BadRequest(new CommunicationDispatchResponse(false, 0, correlationId));
        }

        logger.LogInformation(
            "Dispatching {Intent} from {DispatchSource} to {RecipientCount} recipient(s)",
            request.Intent,
            request.Metadata.Source,
            request.Recipients.Count);

        var model = TransactionModel.Create(request.Model);
        IDispatchCommunicationResult result;

        if (request.Recipients.All(recipient => recipient.Addresses.Count > 0))
        {
            // The originating service already knows where to send the communication.
            IReadOnlyCollection<IPlatformIdentityProfile> profiles =
                [.. request.Recipients.Select(ToPlatformIdentityProfile)];

            result = await communicationsClient.DispatchAsync(
                request.Intent,
                profiles,
                model,
                request.AllowedChannels,
                request.PipelineId,
                request.Culture,
                cancellationToken);
        }
        else if (request.Recipients.All(recipient =>
            recipient.Addresses.Count == 0 && !string.IsNullOrWhiteSpace(recipient.Id)))
        {
            // Identity references are resolved by the platform identity resolvers configured in Program.cs.
            IReadOnlyCollection<IPlatformIdentityReference> references =
                [.. request.Recipients.Select(recipient =>
                    (IPlatformIdentityReference)new IdentityReference(recipient.Type ?? "User", recipient.Id!))];

            result = await communicationsClient.DispatchAsync(
                request.Intent,
                references,
                model,
                request.AllowedChannels,
                request.PipelineId,
                request.Culture,
                cancellationToken);
        }
        else
        {
            // Every recipient must either carry addresses or be an identity reference.
            return TypedResults.BadRequest(new CommunicationDispatchResponse(false, 0, correlationId));
        }

        var response = new CommunicationDispatchResponse(
            result.IsSuccessful,
            result.Results.Count,
            correlationId);

        return result.IsSuccessful
            ? TypedResults.Ok(response)
            : TypedResults.BadRequest(response);
    }

    private static IPlatformIdentityProfile ToPlatformIdentityProfile(CommunicationRecipient recipient) =>
        new PlatformIdentityProfile(
            recipient.Id,
            recipient.Type,
            [.. recipient.Addresses.Select(address =>
                (IPlatformIdentityAddress)new PlatformIdentityAddress(
                    address.Value,
                    address.Display,
                    type: address.Type))]);
}
