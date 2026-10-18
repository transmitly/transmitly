# eShop Series
<img alt="robot waving with a shopping bag" src="robot.svg" width="200" height="200" align="right"/>

Applies Transmitly to Microsoft's [.NET eShop](https://github.com/dotnet/eShop) reference application, one article at a time.

eShop doesn't send customer communications. Over five parts, the series adds an order confirmation and grows it into email, SMS, and push, with delivery tracking and an inbox for buyers. Ordering dispatches one `OrderCreated` intent, and a Communications service decides everything else. From Part 3 on, Ordering's call doesn't change at all.

Each part folder is a complete, runnable copy of eShop and builds on the part before it, so comparing two neighboring folders shows exactly what an article changed.
<br/>
<br/>

## Parts

| Part | Article | What the code adds |
| --- | --- | --- |
| [Part 1](part%201) | [Where should communications live?](part%201/article.md) | Unmodified eShop, the starting point for the series. |
| [Part 2](part%202) | [Sending Email Without Choosing Email](part%202/article.md) | A Communications service. Ordering dispatches `OrderCreated` through `ICommunicationsClient`, middleware forwards it, and the dispatch waits for the order to commit. |
| [Part 3](part%203) | [Composing the Message Where the Data Lives](part%203/article.md) | Ordering dispatches the buyer's identity instead of an email address. Communications resolves the buyer through Identity, Catalog enriches the order with product details, and the email renders from a purpose-built content model. |
| [Part 4](part%204) | [One Intent, Many Channels and Providers](part%204/article.md) | SMS and push from the same intent, push tokens added by a profile enricher, and a simulated provider per channel. |
| [Part 5](part%205) | [After the Dispatch - Handling Delivery Events](part%205/article.md) | Delivery reports recorded in Postgres, a Twilio webhook, and a Messages inbox in the web app. Each buyer gets one message on the first channel that reaches them. |

Parts 2 to 5 each have their own `README.md` describing what changed, with eShop's original README kept beside it as `eshop.README.md`. Part 1 is unmodified eShop, so its `README.md` is eShop's own.

### Features
* Communications Client extensibility
  * [Forwarding dispatches to a Communications service](part%205/src/eShop.ServiceDefaults/Communications/EshopCommunicationsMiddleware.cs)
  * [Holding dispatches until a database transaction commits](part%205/src/Ordering.API/Infrastructure/Communications/TransactionalCommunicationsMiddleware.cs)
* Communication Composition
  * [Resolving Platform Identities](part%205/src/Communications.API/IdentityServerCustomerIdentityResolver.cs) from eShop's Identity service
  * [Platform identity profile enrichers](part%205/src/Communications.API/PushRegistrations/PushRegistrationProfileEnricher.cs) - push device tokens
  * [Content model enrichers](part%205/src/Communications.API/OrderCreated/CatalogContentModelEnricher.cs) - product details from Catalog
* Templating
  * Code-defined templates per channel
* Channels - Email, SMS, Push
* [Channel Provider Restrictions](part%205/src/Communications.API/Program.cs) - pin each channel to one provider
* Delivery Strategies - first match and any match
* Delivery Reports
  * Recording delivery history in a Communications database
  * [Provider webhooks](part%205/src/Communications.API/DeliveryReports/DeliveryReportsController.cs) with `Transmitly.Microsoft.AspnetCore.Mvc`
  * Twilio-specific delivery details
* Simulation provider and composition tests, no provider accounts required

## Running a part

The prerequisites are the same as eShop's: a .NET 10 SDK, the [Aspire CLI](https://aspire.dev/get-started/install-cli/), and a running container runtime such as Docker Desktop. See any part's `eshop.README.md` for details.

From a part's folder:

```console
cd "part 5"
aspire run
```

Or open `Transmitly-Samples.sln` and pick one of the `eShop Part 1` to `eShop Part 5` launch profiles.

From Part 2 on, sign in with one of eShop's seeded users, place an order, and the simulated communications show up in the `communications-api` logs in the Aspire dashboard. In Part 5 they also appear on the **Messages** page in the web app.
