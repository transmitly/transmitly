# eShop communications series — Part 3

This snapshot builds on Part 2. Instead of dispatching to the buyer's email
address, Ordering now dispatches the buyer's identity reference, and the
Communications service resolves it through the service that owns it. The
original eShop README is in [eshop.README.md](eshop.README.md).

The flow is:

```text
Ordering -> Transmitly forwarding middleware -> Communications API
         -> resolve Buyer identity (Identity API)
         -> enrich with product data (Catalog API)
         -> OrderCreated pipeline -> Email -> Simulation provider
```

## What changed from Part 2

- `src/Ordering.API/Application/DomainEventHandlers/SendOrderCreatedCommunicationWhenOrderStartedDomainEventHandler.cs`
  dispatches `IdentityReference(CommunicationIdentityTypes.Buyer, userId)` with
  an `OrderCreatedModel` projected from the order it just created. Ordering no
  longer reads the email claim.
- `src/eShop.ServiceDefaults/Communications/OrderCreatedModel.cs` is the
  transactional model Ordering supplies: order id, date, items, and total.
- `src/Communications.API/OrderCreated` holds the pipeline.
  `CatalogContentModelEnricher` batch-loads the ordered products from Catalog
  and builds `OrderCreatedContentModel`. `OrderCreatedEmail` renders the order
  summary from it. If Catalog is unavailable, the email is sent without product
  details.
- `src/Communications.API/Apis/CommunicationsApi.cs` converts the dispatched JSON
  model into dictionaries so templates and enrichers can read its fields.
- `src/eShop.ServiceDefaults/Communications/CommunicationIdentityTypes.cs`
  shares the `Buyer` identity type between the services that dispatch it and
  the resolver that handles it.
- `src/Communications.API/IdentityServerCustomerIdentityResolver.cs` obtains a
  client-credentials token and calls Identity API's
  `POST /api/identities/resolve` endpoint. The response contains the customer's
  profile data and all available email and phone addresses. Transmitly selects
  the addresses the configured channel needs.
- `src/Identity.API/Apis/IdentityApi.cs` exposes that endpoint to trusted
  services. `Config.cs` adds the `communications` client and the
  `IdentityServerApi` scope.

Place an order in the web app. After Ordering commits the order, the
`OrderCreated` dispatch reaches the Communications API, the buyer is resolved
through Identity API, the products are enriched from Catalog, and a simulated
order-summary email shows up in the `communications-api` logs in the Aspire
dashboard.
