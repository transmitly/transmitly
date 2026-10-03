using eShop.ServiceDefaults.Communications;
using Transmitly;
using Transmitly.Model.Configuration;

namespace eShop.Communications.API.OrderCreated;

public static class OrderCreatedPipeline
{
    /// <summary>
    /// Registers the <c>OrderCreated</c> pipeline: Catalog enrichment, then the first channel
    /// that can reach the buyer, in order: email, SMS, push.
    /// </summary>
    /// <param name="webAppUrl">The storefront address used for the "view your order" link, when known.</param>
    /// <param name="providers">The provider each channel uses.</param>
    public static CommunicationsClientBuilder AddOrderCreatedPipeline(
        this CommunicationsClientBuilder builder,
        Uri? webAppUrl,
        EshopChannelProviders providers)
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
                // The default first-match strategy: each buyer gets one message, on the first
                // channel below that one of their resolved addresses can use.
                pipeline.AddEmail(
                    "orders@eshop.local".AsIdentityAddress("eShop"),
                    email =>
                    {
                        email.AddChannelProviderFilter(providers.Email);
                        email.Subject.AddTemplateResolver(context =>
                            Task.FromResult<string?>(OrderCreatedEmail.Subject(context)));
                        email.TextBody.AddTemplateResolver(context =>
                            Task.FromResult<string?>(OrderCreatedEmail.TextBody(context, webAppUrl)));
                    });

                pipeline.AddSms(
                    providers.SmsFromNumber.AsIdentityAddress("eShop"),
                    sms =>
                    {
                        sms.AddChannelProviderFilter(providers.Sms);

                        // Where Twilio posts status updates. Transmitly adds the routing details
                        // the webhook needs to recognize each update as a Twilio SMS report.
                        sms.Twilio().StatusCallbackUrl = providers.DeliveryReportUrl;

                        sms.Message.AddTemplateResolver(context =>
                            Task.FromResult<string?>(OrderCreatedSms.Message(context, webAppUrl)));
                    });

                pipeline.AddPushNotification(push =>
                {
                    push.AddChannelProviderFilter(providers.Push);
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
