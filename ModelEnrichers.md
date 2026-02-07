Model enrichers let you inspect the current dispatch context and mutate or replace models before a communication is rendered.

There are two enrichment stages:
- **Transaction model enrichers** run once per pipeline (after persona filtering) and can enrich shared, pipeline-level data.
- **Content model enrichers** run per recipient and per channel and can tailor content just before rendering.

DI integration is supported with `Transmitly.Microsoft.DependencyInjection`.

---

## How it works

### Transaction model enrichers
- Run once per pipeline after persona filtering and before any per-recipient content model is created.
- Do **not** use scopes.
- Provide shared data for all recipients/channels in the pipeline.

### Content model enrichers
- Run in two scopes:
  - **PerRecipient**: Runs once per recipient before channel selection.
  - **PerChannel** (default): Runs for each channel dispatch and can use `ChannelId` and `ChannelProviderId`.

Both transaction and content enrichers run **in order** by:
1. `Order` ascending (lowest first).
2. Registration order for enrichers with the same order (or `Order = null`).

If an enricher returns **null**, the current model is preserved and the chain continues.
If an enricher returns a **model** and `ContinueOnEnrichedModel = false`, the chain stops.

---

## Registering enrichers

```csharp
var client = new CommunicationsClientBuilder()
    .AddTransactionModelEnricher<TenantConfigEnricher>(options =>
    {
        options.Order = 10;
    })
    .AddContentModelEnricher<PerRecipientEnricher>(options =>
    {
        options.Scope = ContentModelEnricherScope.PerRecipient;
        options.Order = 20;
    })
    .AddContentModelEnricher<PerChannelEnricher>(options =>
    {
        options.Scope = ContentModelEnricherScope.PerChannel; // default
        options.Order = 30;
        options.Predicate = ctx => ctx.ChannelId == Id.Channel.Email();
    })
    .BuildClient();
```

### Transaction model registration options

| Option | Default | Notes |
|---|---|---|
| `Order` | `null` | Lower executes earlier. `null` runs after ordered items and preserves registration order. |
| `Predicate` | `null` | If provided and returns false, the enricher is skipped. |
| `ContinueOnEnrichedModel` | `true` | When false, the chain stops after this enricher returns a model. |

### Content model registration options

| Option | Default | Notes |
|---|---|---|
| `Scope` | `PerChannel` | `PerRecipient` or `PerChannel`. |
| `Order` | `null` | Lower executes earlier. `null` runs after ordered items and preserves registration order. |
| `Predicate` | `null` | If provided and returns false, the enricher is skipped. |
| `ContinueOnEnrichedModel` | `true` | When false, the chain stops after this enricher returns a model. |

---

## Writing a transaction model enricher

```csharp
public sealed class AddTenantConfigEnricher : ITransactionModelEnricher
{
    public Task<ITransactionModel?> EnrichAsync(
        IDispatchCommunicationContext context,
        ITransactionModel currentModel,
        CancellationToken cancellationToken = default)
    {
        if (currentModel.Model is IDictionary<string, object?> bag)
        {
            bag["tenant"] = new
            {
                id = context.PipelineIntent,
                plan = "pro"
            };
        }

        return Task.FromResult<ITransactionModel?>(currentModel);
    }
}
```

---

## Writing a content model enricher

```csharp
public sealed class AddRecipientBrandingEnricher : IContentModelEnricher
{
    public Task<IContentModel?> EnrichAsync(
        IDispatchCommunicationContext context,
        IContentModel currentModel,
        CancellationToken cancellationToken = default)
    {
        if (currentModel.Model is IDictionary<string, object?> bag)
        {
            bag["branding"] = new
            {
                tenant = context.PipelineIntent,
                logo = "https://cdn.example.com/logo.png"
            };
        }

        return Task.FromResult<IContentModel?>(currentModel);
    }
}
```

---

## Replace the model

```csharp
public sealed class ReplaceContentModelEnricher : IContentModelEnricher
{
    public Task<IContentModel?> EnrichAsync(
        IDispatchCommunicationContext context,
        IContentModel currentModel,
        CancellationToken cancellationToken = default)
    {
        var newModel = new ContentModel(
            TransactionModel.Create(new { Marker = "replaced" }),
            context.PlatformIdentities);

        return Task.FromResult<IContentModel?>(newModel);
    }
}
```

```csharp
public sealed class ReplaceTransactionModelEnricher : ITransactionModelEnricher
{
    public Task<ITransactionModel?> EnrichAsync(
        IDispatchCommunicationContext context,
        ITransactionModel currentModel,
        CancellationToken cancellationToken = default)
    {
        var newModel = TransactionModel.Create(new { Marker = "replaced" });
        return Task.FromResult<ITransactionModel?>(newModel);
    }
}
```

---

## Scope guidance

**Transaction model** is best for:
- Tenant-level or pipeline-level enrichment.
- External data you want reused across recipients/channels.

**PerRecipient content model** is best for:
- Pulling user profile data.
- Shared, recipient-level enrichment.

**PerChannel content model** is best for:
- Channel-specific data or formatting.
- Conditional changes based on `ChannelId` or `ChannelProviderId`.

Notes:
- PerRecipient runs **before** channel selection, so `ChannelId` is null.
- PerChannel runs **after** channel selection, right before communication rendering.

---

## Performance tips

- Keep enrichers fast and avoid blocking I/O.
- Use predicates to avoid unnecessary work.
- Use transaction enrichers for shared data and keep per-channel content enrichers light.

---

## Behavior summary

- Enrichers are optional; nothing runs unless registered.
- Order is deterministic.
- Returning `null` keeps the current model.
- `ContinueOnEnrichedModel = false` stops the chain after an enricher returns a model.
