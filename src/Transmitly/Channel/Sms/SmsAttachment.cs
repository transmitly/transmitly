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

using Transmitly.Exceptions;

namespace Transmitly.Channel.Sms
{
	internal class SmsAttachment : ISmsAttachment
	{
		public SmsAttachment(Resource resource)
		{
			Name = Guard.AgainstNullOrWhiteSpace(resource.Name);
			ContentType = Guard.AgainstNullOrWhiteSpace(resource.ContentType);
			ContentStream = Guard.AgainstNull(resource.ContentStream);
			if (!ContentStream.CanRead)
				throw new CommunicationsException($"Attachment ContentStream is not readable");
		}

		public string? Name { get; set; }

		public string? ContentType { get; set; }

		public Stream? ContentStream { get; set; }
	}
}
