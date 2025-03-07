# Kitchen Sink
<img src="https://github.com/user-attachments/assets/91261a97-cfac-46af-b2c3-c2b977a73107" style="width:200px; height:150px;max-width:100%" align="right"/>

Aims to demonstrate as many features of Transmitly as possible in a single project. 

Take a look at the [appsettings.json](https://github.com/transmitly/transmitly/blob/main/samples/Transmitly.KitchenSink.AspNetCoreWebApi/appsettings.json) for configuration options. If you change nothing, all communications will be logged to the console of the app.
<br/><br/>
<br/>

## 


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


### Routes
* dispatch/otp - Simulates sending a OTP to a user via Email or Push.
* dispatch - Generic dispatch route to allow you to customize dispatching communications.
* channel/provider/update - When exposed publicly can be used with channel providers like Twilio and SendGrid to receive delivery reports.
* twilio/messageNeeded - When exposed publicly can be used with Twilio to provide message content for Voice messages.
  *	Requires [uncommenting code](https://github.com/transmitly/transmitly/blob/694ce5bc2a8ce261a3a52be2518d06835179d2eb/samples/Transmitly.KitchenSink.AspNetCoreWebApi/Program.cs#L182-L187) and the Twilio channel provider to be configured correctly.
