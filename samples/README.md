# Samples

This directory contains sample projects to help demonstrates features of Transmitly in various scenarios.

## [Hello Transmitly](https://github.com/transmitly/transmitly/tree/main/samples/HelloTransmitly)
The smallest possible example of using Transmitly

## [Kitchen Sink](https://github.com/transmitly/transmitly/tree/main/samples/Transmitly.KitchenSink.AspNetCoreWebApi)
Demonstrates as many features of Transmitly as possible in a single project. 

### Features
* Channel - Available Providers
  * Email - Smtp, Twilio, Infobip, SendGrid
  * Push - Firebase
  * SMS - Twilio, Infobip
  * Logger - All
* Channel Delivery Reports
* Template Engine Support - Fluid
* Channel Provider Restrictions 
	* [Restrict channels to a certain channel provider](https://github.com/transmitly/transmitly/blob/694ce5bc2a8ce261a3a52be2518d06835179d2eb/samples/Transmitly.KitchenSink.AspNetCoreWebApi/Program.cs#L157-L161)
* Channel Provider specific functionality
  * Twilio 
	* Get Voice Message Route
	* Delivery Reports - See twilio/messageNeeded
  * SendGrid
	* [Use SendGrid TemplateIds](https://github.com/transmitly/transmitly/blob/694ce5bc2a8ce261a3a52be2518d06835179d2eb/samples/Transmitly.KitchenSink.AspNetCoreWebApi/Program.cs#L155C1-L155C7)

## [Microservices](https://github.com/transmitly/transmitly/tree/main/samples/Microservices)
Demonstrates how you can completely extend the default Transmitly behavior by showcasing an notifications service that other services call. 

### Features
* Communications Client extensibility
* Communication Composition
  * [Resolving Platform Identities](https://github.com/transmitly/transmitly/blob/main/samples/Microservices/Tandely.Notifications.Service/CustomerRepository.cs)    
* Templating
    * Fluid Template Engine
    * Remote Template Loading
    * Embedded Templates
    * Static Templates
* Delivery Strategy Modifier 
* ["From" address resolution](https://github.com/transmitly/transmitly/blob/9a7942313df0fe532e7ad365301b251d964b9e12/samples/Microservices/Tandely.Notifications.Service/Program.cs#L92-L96) (multi-tenant 'from' addresses)
* [Persona filters](https://github.com/transmitly/transmitly/blob/9a7942313df0fe532e7ad365301b251d964b9e12/samples/Microservices/Tandely.Notifications.Service/Program.cs#L84C5-L84C81) - Filter communications based on properties of the identity
* Channel - Available Providers
  * Email - Smtp, Twilio, Infobip, SendGrid
  * Push - Firebase
  * SMS - Twilio, Infobip
  * Logger - All
* Delivery Reports

## [eShop Series](https://github.com/transmitly/transmitly/tree/main/samples/eShop%20Series)
Applies Transmitly to Microsoft's [.NET eShop](https://github.com/dotnet/eShop) reference application over a five-part article series. Ordering dispatches a single `OrderCreated` intent, and a Communications service grows it from one simulated email into email, SMS, and push, with delivery tracking and an inbox for buyers. Each part is a complete, runnable copy of eShop that builds on the part before it.

### Features
* Communications Client extensibility
  * Forwarding dispatches to a central Communications service
  * Holding dispatches until a database transaction commits
* Communication Composition
  * Resolving Platform Identities from an identity service
  * Platform identity profile enrichers
  * Content model enrichers
* Channels - Email, SMS, Push
* Channel Provider Restrictions
* Delivery Strategies - first match and any match
* Delivery Reports
  * Recording delivery history
  * Provider webhooks with Transmitly.Microsoft.AspnetCore.Mvc (Twilio)
* Simulation provider and composition tests
