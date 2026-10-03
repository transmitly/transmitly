using eShop.ServiceDefaults.Communications;
using Transmitly;
using Transmitly.Model.Configuration;

namespace eShop.Communications.API.OrderCreated;

public static class OrderCreatedPipeline
{
    /// <summary>
    /// Registers the <c>OrderCreated</c> pipeline: Catalog enrichment followed by the order summary email.
    /// </summary>
    /// <param name="webAppUrl">The storefront address used for the "view your order" link, when known.</param>
    public static CommunicationsClientBuilder AddOrderCreatedPipeline(
        this CommunicationsClientBuilder builder,
        Uri? webAppUrl)
    {
        return builder
            .AddContentModelEnricher<CatalogContentModelEnricher>(options =>
            {
                // Enrich once per recipient so every channel shares the same content.
                options.Scope = ContentModelEnricherScope.PerRecipient;
                options.Predicate = context => context.PipelineIntent == CommunicationIntents.OrderCreated;
            })
            .AddPipeline(CommunicationIntents.OrderCreated, pipeline =>
            {
                pipeline.AddEmail(
                    "orders@eshop.local".AsIdentityAddress("eShop"),
                    email =>
                    {
                        email.Subject.AddTemplateResolver(context =>
                            Task.FromResult<string?>(OrderCreatedEmail.Subject(context)));
                        email.TextBody.AddTemplateResolver(context =>
                            Task.FromResult<string?>(OrderCreatedEmail.TextBody(context, webAppUrl)));
                    });
            });
    }
}
