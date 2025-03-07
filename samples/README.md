# Samples

This directory contains sample projects to help demonstrates how to use Transmitly in various scenarios.

## [Kitchen Sink](https://github.com/transmitly/transmitly/tree/main/samples/Transmitly.KitchenSink.AspNetCoreWebApi)
Aims to use as many features of Transmitly as possible. 

Take a look at the [appsettings.json]() for configuration options. If you change nothing, all communications will be logged to the console of the app.

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
Demonstrates how you can completely extend the default Transmitly behavior by showcasing an notifications service that other services might call. 

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
 
## [Transmitly.ChannelProvider.Logger](https://github.com/transmitly/transmitly/tree/main/samples/Transmitly.ChannelProvider.Logger)
An example channel provider that handles all channels. It's only purpose is to log communications dispatched with Transmitly.
