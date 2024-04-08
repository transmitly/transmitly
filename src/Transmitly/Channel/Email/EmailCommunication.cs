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
	/// <inheritdoc />
	internal sealed class EmailCommunication(IAudienceAddress from, IExtendedProperties extendedProperties) : IEmail
	{
		/// <inheritdoc />
		public string? Subject { get; internal set; }

		/// <inheritdoc />
		public string? HtmlBody { get; internal set; }

		/// <inheritdoc />
		public string? TextBody { get; internal set; }

		/// <inheritdoc />
		public MessagePriority Priority { get; internal set; } = MessagePriority.Normal;

		/// <inheritdoc />
		public TransportPriority TransportPriority { get; internal set; } = TransportPriority.Normal;

		/// <inheritdoc />
		public IAudienceAddress From { get; } = Guard.AgainstNull(from);

		/// <inheritdoc />
		public IAudienceAddress[]? ReplyTo { get; internal set; } = [from];

		/// <inheritdoc />
		public IAudienceAddress[]? To { get; internal set; }

		/// <inheritdoc />
		public IAudienceAddress[]? Cc { get; internal set; }

		/// <inheritdoc />
		public IAudienceAddress[]? Bcc { get; internal set; }

		/// <inheritdoc />
		public IReadOnlyCollection<IAttachment> Attachments { get; internal set; } = [];

		/// <inheritdoc />
		public IExtendedProperties ExtendedProperties { get; } = Guard.AgainstNull(extendedProperties);

		/// <inheritdoc />
		public string? DeliveryReportCallbackUrl { get; set; }

		/// <inheritdoc />
		public Func<IDispatchCommunicationContext, Task<string?>>? DeliveryReportCallbackUrlResolver { get; set; }
	}
}