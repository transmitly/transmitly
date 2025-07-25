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

namespace Transmitly.Channel.Sms;

/// <inheritdoc cref="ISms"/>
internal class SmsCommunication(IExtendedProperties extendedProperties) : ISms
{
	public IPlatformIdentityAddress[]? To { get; set; }

	public string? Message { get; set; }

	public MessagePriority Priority { get; set; }

	public TransportPriority TransportPriority { get; set; }

	public IReadOnlyCollection<ISmsAttachment> Attachments { get; set; } = [];

	public IPlatformIdentityAddress? From { get; set; }

	public IExtendedProperties ExtendedProperties { get; } = Guard.AgainstNull(extendedProperties);

	public Func<IDispatchCommunicationContext, Task<string?>>? DeliveryReportCallbackUrlResolver { get; set; }
}
