using eShop.Communications.API.PushRegistrations;

namespace Microsoft.Extensions.DependencyInjection;

public static class PushRegistrationServiceExtensions
{
    /// <summary>
    /// Registers the push profile enricher with a store that gives every buyer a simulated device.
    /// Transmitly resolves enrichers from the container, so the enricher is registered here too.
    /// </summary>
    public static IServiceCollection AddSimulatedPushRegistrations(this IServiceCollection services)
    {
        services.AddSingleton<IPushRegistrationStore, SimulatedPushRegistrationStore>();
        services.AddSingleton<PushRegistrationProfileEnricher>();
        return services;
    }
}
