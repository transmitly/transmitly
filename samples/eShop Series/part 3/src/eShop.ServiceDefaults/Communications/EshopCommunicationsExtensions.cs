using eShop.ServiceDefaults.Communications;
using Transmitly;

namespace Microsoft.Extensions.DependencyInjection;

public static class EshopCommunicationsExtensions
{
    public static IServiceCollection AddEshopCommunications(this IServiceCollection services)
    {
        services.AddHttpClient<EshopCommunicationsMiddleware>(client =>
        {
            // Aspire service discovery resolves this logical service name.
            client.BaseAddress = new Uri(EshopCommunicationsMiddleware.ServiceBaseAddress);
        });

        services.AddTransmitly<EshopCommunicationsConfigurator>();

        return services;
    }
}
