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

namespace Transmitly.Verification.Configuration
{
	internal sealed class DefaultSenderVerificationCodeProvider : ISenderVerificationCodeGenerator
	{
		private readonly byte[] hashBytes = Guid.NewGuid().ToByteArray();

		public string GenerateCode(int length = 6)
		{
			// Use a bitwise operation to get a representative binary code from the hash
			// Refer section 5.4 at https://www.rfc-editor.org/rfc/rfc4226#page-7            
			int offset = hashBytes[19] & 0xf;
			int binaryCode = (hashBytes[offset] & 0x7f) << 24
				| (hashBytes[offset + 1] & 0xff) << 16
				| (hashBytes[offset + 2] & 0xff) << 8
				| (hashBytes[offset + 3] & 0xff);

			// Generate the OTP using the binary code. As per RFC 4426 [link above] "Implementations MUST extract a 6-digit code at a minimum 
			// and possibly 7 and 8-digit code"
			int otp = binaryCode % (int)Math.Pow(10, length); // where 6 is the password length

			return otp.ToString().PadLeft(length, '0');
		}
	}
}
