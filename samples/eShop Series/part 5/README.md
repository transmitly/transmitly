# eShop communications series — Part 5

This snapshot builds on Part 4. Communications now records every delivery report
it receives, from the simulator at dispatch or from a provider's webhook later,
in its own database. Buyers can see what was sent to them on a new Messages
page in the web app. The original eShop README is in
[eshop.README.md](eshop.README.md).

The flow is:

```text
Dispatch ──> provider ──> delivery report (Dispatched) ──┐
                │                                        │
                └─ later: webhook ──> delivery report ───┤
                   (Twilio status callback)              v
                                              DeliveryReportQueue
                                                         │
                                                         v
                                              DeliveryReportRecorder
                                                         │
                                                         v
                                                 communicationsdb
                                                         │
                                                         v
                                     GET /api/communications/inbox ──> WebApp Messages
```

Ordering is unchanged from Part 4.

## What changed from Part 4

- `src/Communications.API/DeliveryReports` holds the delivery report pipeline:
  - `CommunicationsContext`, `CommunicationRecord`, and `DeliveryEventRecord`
    store each communication and its delivery history in `communicationsdb`.
  - `DeliveryReportQueue` and `DeliveryReportProcessor` take reports from
    Transmitly's delivery report handler and record them in the background.
  - `DeliveryReportRecorder` matches later reports to their communication by
    the provider's message id, and keeps Twilio's own details where they exist.
  - `DeliveryReportsController` is the provider webhook at
    `POST /api/communications/delivery-reports`, built on
    `Transmitly.Microsoft.AspnetCore.Mvc`.
- The `OrderCreated` pipeline in `src/Communications.API/Program.cs` goes back
  to Transmitly's default first-match strategy: each buyer gets one message, on the
  first of email, SMS, or push that can reach them.
- `src/Communications.API/IdentityServerCustomerIdentityResolver.cs` only keeps
  addresses the buyer has verified, so verification decides the channel.
- `src/Identity.API/UsersSeed.cs` seeds bob with a verified phone number and an
  unverified email address. Alice keeps her verified email address.
- `src/Communications.API/EshopChannelProviders.cs` uses Twilio for SMS when
  Twilio credentials are configured, and the simulator otherwise.
- `src/Communications.API/Apis/InboxApi.cs` exposes
  `GET /api/communications/inbox` to the signed-in user.
- `src/WebApp/Components/Pages/User/Messages.razor` shows the buyer's
  communications and their latest delivery status. Identity adds a
  `communications` scope for the web app.

## Connecting Twilio

SMS stays on the simulator until these settings are present for the
Communications API (user secrets or environment variables):

```json
{
  "Twilio": {
    "AccountSid": "AC...",
    "AuthToken": "...",
    "FromNumber": "+1..."
  },
  "Communications": {
    "DeliveryReportUrl": "https://<public host>/api/communications/delivery-reports"
  }
}
```

`DeliveryReportUrl` must be reachable by Twilio, for example through a tunnel
while developing locally.

## Trying it

Sign in as `alice`, place an order, and open **Messages** from the account menu:
the order confirmation arrived as an email. Sign out, do the same as `bob`, and
it arrived as an SMS. Ordering dispatched the same `OrderCreated` intent both
times.

The seed only creates users that don't exist yet. If your `identitydb` was
seeded by an earlier part, reset it or mark bob's phone as verified and his
email as unverified:

```sql
UPDATE "AspNetUsers"
SET "EmailConfirmed" = false, "PhoneNumberConfirmed" = true
WHERE "UserName" = 'bob';
```

Part 5 uses Transmitly 0.4.2.
