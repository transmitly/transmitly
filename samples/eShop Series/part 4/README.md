# eShop communications series — Part 4

This snapshot builds on Part 3. The `OrderCreated` pipeline now sends an email,
an SMS, and a push notification from the same enriched content, each through
its own provider. Every provider is Transmitly's simulator, so nothing leaves
the machine. The original eShop README is in [eshop.README.md](eshop.README.md).

The flow is:

```text
Ordering -> Transmitly forwarding middleware -> Communications API
         -> resolve Buyer identity (Identity API)
         -> add push devices (push registration store)
         -> enrich with product data (Catalog API)
         -> OrderCreated pipeline -> Email -> Simulation.Email
                                  -> SMS   -> Simulation.Sms
                                  -> Push  -> Simulation.Push
```

Ordering is unchanged from Part 3.

## What changed from Part 3

- `src/Communications.API/OrderCreated/OrderCreatedPipeline.cs` adds SMS and
  push channels and uses the any-match delivery strategy, so the buyer is
  reached on every channel they have an address for. Each channel is pinned to
  one provider.
- `src/Communications.API/OrderCreated/OrderCreatedSms.cs` and
  `OrderCreatedPush.cs` render the shorter channel content.
- `src/Communications.API/EshopChannelProviders.cs` registers one simulator per
  channel and names the provider each channel uses. Connecting a real provider
  (SMTP, Twilio, Firebase) means changing that file.
- `src/Communications.API/PushRegistrations` holds push device tokens, which are
  communications data. `PushRegistrationProfileEnricher` adds them to the buyer's
  profile. eShop's apps don't register for push, so
  `SimulatedPushRegistrationStore` gives every buyer one simulated device.
  `CustomerIdentityProfile` becomes a class with `AddAddress` so the enricher
  can add those tokens.
- `OrderCreatedEmail` and `OrderCreatedSms` share the "view your order" link
  through `OrderLinks`.

Place an order in the web app. The `communications-api` logs in the Aspire
dashboard show three simulated deliveries, one per channel, each tagged with
its provider.
