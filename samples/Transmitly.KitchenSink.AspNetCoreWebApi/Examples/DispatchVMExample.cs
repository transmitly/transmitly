﻿// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
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
using Swashbuckle.AspNetCore.Filters;
using Transmitly.Channel.Push;
using Transmitly.KitchenSink.AspNetCoreWebApi.Controllers;

namespace Transmitly.KitchenSink.AspNetCoreWebApi
{
	public class DispatchVMExample : IExamplesProvider<DispatchVM>
	{
		public DispatchVM GetExamples()
		{
			return new DispatchVM
			{
				PipelineName = "first-pipeline",
				AllowedChannelIds = null,
				TransationModel = new DispatchTransactionModel
				{
					Model = new
					{
						firstName = "Mit",
						lastName = "Ly",
						date = DateTime.UtcNow,
						amount = 100.11,
						currency = "$"
					}
				},
				Culture = null,
				Recipients = [
					 new(){
						 Addresses = [
							new("example@domain.com") { Display="Example Display" },
							new("+18885551234"),
							new("fe595523a0c2965f9eabff921555df48-80df133c-5aab-4db4-bd03-b04331181664") { Type=IdentityAddress.Types.DeviceToken() }
						 ]
					 }
				]

			};
		}
	}
}