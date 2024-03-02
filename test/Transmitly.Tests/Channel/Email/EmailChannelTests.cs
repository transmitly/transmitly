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

namespace Transmitly.Channel.Email.Tests
{
	[TestClass()]
	public class EmailChannelTests
	{
		[TestMethod()]
		//https://gist.github.com/cjaoude/fd9910626629b53c4d25

		[DataRow("email@subdomain.example.com", true)]
		[DataRow("\"email\"@example.com", true)]
		[DataRow("1234567890@example.com", true)]
		[DataRow("email@example-one.com", true)]
		[DataRow("_______@example.com", true)]
		[DataRow("email@example.name", true)]
		[DataRow("email@example.museum", true)]
		[DataRow("email@example.co.jp", true)]
		[DataRow("email@example.web", true)]
		[DataRow("あいうえお@example.com", true)]
		[DataRow("email+suffix@example.com", true)]
		[DataRow("email.dot.dot+suffix+suffix-@example.com", true)]

		//while valid, these are uncommon and not matched
		[DataRow("email @example.com", false)]
		[DataRow("firstname.lastname @example.com", false)]
		[DataRow("firstname+lastname @example.com", false)]
		[DataRow("email@123.123.123.123", false)]
		[DataRow("email@[123.123.123.123]", false)]
		[DataRow("firstname-lastname @example.co", false)]
		[DataRow("much.”more\\ unusual”@example.com", false)]
		[DataRow("very.unusual.”@”.unusual.com @example.com", false)]
		[DataRow("very.”(),:;<>[]”.VERY.”very@\\ \"very”.unusual@strange.example.com", false)]
		
		//invalid 
		[DataRow("plainaddress", false)]
		[DataRow("#@%^%#$@#$@#.com", false)]
		[DataRow("@example.com", false)]
		[DataRow("Joe Smith <email @example.com>", false)]
		[DataRow("email.example.com", false)]
		[DataRow("email@example @example.com", false)]
		[DataRow(".email @example.com", false)]
		[DataRow("email.@example.com", false)]
		[DataRow("email..email @example.com", false)]
		[DataRow("email@example.com (Joe Smith)", false)]
		[DataRow("email@example", false)]
		[DataRow("email@-example.com", false)]
		[DataRow("email@111.222.333.44444", false)]
		[DataRow("email @example..com", false)]
		[DataRow("Abc..123@example.com", false)]
		public void MatchesEmailAddressesAsExpected(string email, bool expected)
		{
			var channel = new EmailChannel("unit@test.com".AsAudienceAddress());

			var result = channel.SupportsAudienceAddress(email.AsAudienceAddress());
			Assert.AreEqual(expected, result, email);
		}
	}
}