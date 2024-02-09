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

namespace Transmitly.Channel.Email
{
	internal sealed class EmailCommunication(IAudienceAddress from) : IEmail
	{
		public EmailCommunication(string fromValue, string? fromDisplay = null) : this(new AudienceAddress(fromValue, fromDisplay))
		{

		}

		public string? Subject { get; internal set; }
		public string? HtmlBody { get; internal set; }
		public string? TextBody { get; internal set; }

		public MessagePriority Priority { get; internal set; } = MessagePriority.Normal;

		public TransportPriority TransportPriority { get; internal set; } = TransportPriority.Normal;

		public IAudienceAddress From { get; } = Guard.AgainstNull(from);

		public IAudienceAddress[]? ReplyTo { get; internal set; } = [from];

		public IAudienceAddress[]? To { get; internal set; }

		public IAudienceAddress[]? Cc { get; internal set; }

		public IAudienceAddress[]? Bcc { get; internal set; }

		public IReadOnlyCollection<IAttachment> Attachments { get; internal set; } = [];


	}
}