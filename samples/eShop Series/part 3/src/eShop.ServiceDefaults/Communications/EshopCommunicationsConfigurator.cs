using Microsoft.Extensions.DependencyInjection;
using Transmitly;

namespace eShop.ServiceDefaults.Communications;

public sealed class EshopCommunicationsConfigurator(
    EshopCommunicationsMiddleware forwardingMiddleware,
    IEnumerable<ICommunicationClientMiddleware> serviceMiddleware) : ICommunicationsClientConfigurator
{
    public void ConfigureClient(CommunicationsClientBuilder builder)
    {
        builder.AddClientMiddleware(forwardingMiddleware);

        // Middleware added later wraps the forwarding client, so a service can
        // shape dispatches (for example, hold them until a transaction commits)
        // before they leave for the Communications service.
        foreach (var middleware in serviceMiddleware)
        {
            builder.AddClientMiddleware(middleware);
        }
    }
}
