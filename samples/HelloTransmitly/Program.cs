// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
//  
//  Licensed under the Apache License, Version 2.0 (the "License")
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using Transmitly;

var communicationsClient = new CommunicationsClientBuilder()
	.AddSimulationSupport()
	.AddPipeline("WelcomeKit", pipeline =>
	{
		pipeline.AddEmail(
			"welcome@example.com".AsIdentityAddress("Welcome"),
			email =>
			{
				email.Subject.AddStringTemplate("Welcome!");
				email.TextBody.AddStringTemplate(
					"Thanks for trying Transmitly.");
			});
	})
	.BuildClient();

const string intent = "WelcomeKit";
const string recipient = "developer@example.com";

Console.WriteLine("Hello Transmitly!");
Console.WriteLine(new string('-', 50));
Row("Pipeline Intent", intent);
Row("Recipient", recipient);
Console.WriteLine();
Console.WriteLine("Press any key to dispatch the communication...");
Console.ReadKey(intercept: true);

// Your application expresses the intent for dispatching.
var result = await communicationsClient.DispatchAsync(
	intent,
	recipient,
	new { });

foreach (var dispatch in result.Results)
{
	if (dispatch is null)
		continue;

	Console.WriteLine();
	Row("Channel", dispatch.ChannelId);
	Row("Channel Provider", dispatch.ChannelProviderId);
	Row("Status", $"{dispatch.Status.Code} {dispatch.Status.Type}");
	Row("Resource Id", dispatch.ResourceId);
	if (dispatch.Exception is not null)
		Row("Error", dispatch.Exception.Message);
}

Console.WriteLine(new string('-', 50));
Console.WriteLine(
	result.IsSuccessful
		? "Communication dispatched successfully."
		: "Communication failed.");

Console.WriteLine();
Console.WriteLine("Tip: add a delivery report handler to see the composed email (subject, body, ...):");
Console.WriteLine("  https://github.com/transmitly/transmitly/wiki/Delivery-Reports");

Console.WriteLine();
Console.WriteLine("Press any key to exit.");
Console.ReadKey(intercept: true);

static void Row(string label, string? value) =>
	Console.WriteLine($"  {label,-18}: {value ?? "-"}");
