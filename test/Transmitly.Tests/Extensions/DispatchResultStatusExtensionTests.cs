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

namespace Transmitly.Tests
{
	[TestClass]
	public class DispatchResultStatusExtensionsTests
	{
		[TestMethod]
		public void IsSuccessReturnsTrueForSuccessCode()
		{
			var status = new CommunicationsStatus ("test", "test", 2000);

			var result = status.IsSuccess();

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void IsSuccessReturnsTrueForInfoCode()
		{
			var status = new CommunicationsStatus ("test", "test", 1500);

			var result = status.IsSuccess();

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void IsSuccessReturnsFalseForClientErrorCode()
		{
			var status = new CommunicationsStatus ("test", "test", 4000);

			var result = status.IsSuccess();

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void IsClientErrorReturnsTrueForClientErrorCode()
		{
			var status = new CommunicationsStatus ("test", "test", 4500);

			var result = status.IsClientError();

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void IsClientErrorReturnsFalseForServerErrorCode()
		{
			var status = new CommunicationsStatus ("test", "test", 5000);

			var result = status.IsClientError();

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void IsServerErrorReturnsTrueForServerErrorCode()
		{
			var status = new CommunicationsStatus ("test", "test", 5500);

			var result = status.IsServerError();

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void IsServerErrorReturnsFalseForClientErrorCode()
		{
			var status = new CommunicationsStatus ("test", "test", 4500);

			var result = status.IsServerError();

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void IsFailureReturnsTrueForClientErrorCode()
		{
			var status = new CommunicationsStatus("test", "test", 4500);

			var result = status.IsFailure();

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void IsFailureReturnsTrueForServerErrorCode()
		{
			var status = new CommunicationsStatus("test", "test", 5500);

			var result = status.IsFailure();

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void IsFailureReturnsFalseForSuccessCode()
		{
			var status = new CommunicationsStatus("test","test", 2000);

			var result = status.IsFailure();

			Assert.IsFalse(result);
		}
	}
}