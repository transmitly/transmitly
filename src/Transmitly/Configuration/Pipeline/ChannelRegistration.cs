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

using Transmitly.Channel.Configuration;

namespace Transmitly.Pipeline.Configuration
{
	internal sealed class ChannelRegistration(IChannel channel) : IChannelRegistration
	{
		public IChannel Channel { get; } = Guard.AgainstNull(channel);
		public string? ToAddress { get; internal set; }
		public string? ToAddressPurpose { get; internal set; }
		public IReadOnlyCollection<string> FilterChannelProviderIds { get; internal set; } = [];

		public string? BlindCopyAddress { get; internal set; }

		public string? BlindCopyAddressPurpose { get; internal set; }

		public string? CopyAddress { get; internal set; }

		public string? CopyAddressPurpose { get; internal set; }

		public bool CompleteOnDispatch { get; internal set; }
	}
}