using Microsoft.Extensions.DependencyInjection;
using Transmitly;

namespace eShop.ServiceDefaults.Communications;

public sealed class EshopCommunicationsConfigurator(
    EshopCommunicationsMiddleware forwardingMiddleware) : ICommunicationsClientConfigurator
{
    public void ConfigureClient(CommunicationsClientBuilder builder)
    {
        builder.AddClientMiddleware(forwardingMiddleware);
    }
}
