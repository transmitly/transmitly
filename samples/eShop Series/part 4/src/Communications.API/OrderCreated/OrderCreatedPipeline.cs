using eShop.ServiceDefaults.Communications;
using Transmitly;
using Transmitly.Model.Configuration;

namespace eShop.Communications.API.OrderCreated;

public static class OrderCreatedPipeline
{
    /// <summary>
    /// Registers the <c>OrderCreated</c> pipeline: Catalog enrichment, then an email, an SMS,
    /// and a push notification rendered from the same content.
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
                // Send on every channel the recipient can be reached on.
                pipeline.UseAnyMatchPipelineDeliveryStrategy();

                pipeline.AddEmail(
                    "orders@eshop.local".AsIdentityAddress("eShop"),
                    email =>
                    {
                        email.AddChannelProviderFilter(EshopChannelProviders.Email);
                        email.Subject.AddTemplateResolver(context =>
                            Task.FromResult<string?>(OrderCreatedEmail.Subject(context)));
                        email.TextBody.AddTemplateResolver(context =>
                            Task.FromResult<string?>(OrderCreatedEmail.TextBody(context, webAppUrl)));
                    });

                pipeline.AddSms(
                    "+15555550100".AsIdentityAddress("eShop"),
                    sms =>
                    {
                        sms.AddChannelProviderFilter(EshopChannelProviders.Sms);
                        sms.Message.AddTemplateResolver(context =>
                            Task.FromResult<string?>(OrderCreatedSms.Message(context, webAppUrl)));
                    });

                pipeline.AddPushNotification(push =>
                {
                    push.AddChannelProviderFilter(EshopChannelProviders.Push);
                    push.Title.AddTemplateResolver(context =>
                        Task.FromResult<string?>(OrderCreatedPush.Title(context)));
                    push.Body.AddTemplateResolver(context =>
                        Task.FromResult<string?>(OrderCreatedPush.Body(context)));
                    push.AddData(OrderCreatedPush.ActionKey, OrderCreatedPush.OpenOrderAction);
                    push.AddDataIfNotNull(OrderCreatedPush.OrderIdKey, context =>
                        Task.FromResult(OrderCreatedPush.OrderId(context)));
                });
            });
    }
}
