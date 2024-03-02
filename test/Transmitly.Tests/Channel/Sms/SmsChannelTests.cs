using Microsoft.VisualStudio.TestTools.UnitTesting;
using Transmitly.Channel.Sms;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transmitly.Channel.Sms.Tests
{
	[TestClass()]
	public class SmsChannelTests
	{
		[TestMethod()]
		[DataRow("+14155552671", true)]
		[DataRow("+442071838750", true)]
		[DataRow("+551155256325", true)]
		[DataRow("551155256325", false)]
		[DataRow("51155256325", false)]
		[DataRow("(511)55256325", false)]
		[DataRow("511-552-56325", false)]
		[DataRow("+1 511-552-56325", false)]
		[DataRow("+1 51155256325", false)]
		[DataRow("+37060112345", true)]
		[DataRow("+64010", true)]//NZ service number
		[DataRow("+1231234567890", true)]
		[DataRow("+2902124", true)]
		public void SupportsAudienceAddressTest(string value, bool expected)
		{
			var sms = new SmsChannel();
			var result = sms.SupportsAudienceAddress(value.AsAudienceAddress());
			Assert.AreEqual(expected, result, value);
		}
	}
}