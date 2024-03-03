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

namespace Transmitly.Channel.Push.Tests
{
	[TestClass()]
	public class PushNotificationChannelTests
	{
		[TestMethod()]
		public void SupportsAudienceAddressTypes()
		{
			var tests = new List<(AudienceAddress, bool)> {
				(new AudienceAddress("test", type: AudienceAddress.Types.DeviceToken()), true),
				(new AudienceAddress("test", type: AudienceAddress.Types.Topic()), true),
				(new AudienceAddress("fe595523a0c2965f9eabff921555df48-80df133c-5aab-4db4-bd03-b04331181664", type:AudienceAddress.Types.DeviceToken()), true),
				(new AudienceAddress("test", type: "other"), false),
				(new AudienceAddress("test"), false),
				(new AudienceAddress("fe595523a0c2965f9eabff921555df48-80df133c-5aab-4db4-bd03-b04331181664"), false)
			};

			foreach (var test in tests)
			{
				var channel = new PushNotificationChannel([]);
				var result = channel.SupportsAudienceAddress(test.Item1);

				Assert.AreEqual(test.Item2, result, test.Item1.Value + ":" + test.Item1.Type);
			}
		}
	}
}