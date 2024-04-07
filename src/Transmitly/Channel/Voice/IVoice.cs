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

namespace Transmitly
{
	public interface IVoice
	{
		IAudienceAddress? From { get; }
		IAudienceAddress[]? To { get; }
		IVoiceType? VoiceType { get; }
		string Message { get; }
		MachineDetection MachineDetection { get; }
		TransportPriority TransportPriority { get; }
		IExtendedProperties ExtendedProperties { get; }
		/// <summary>
		/// The URL to call for status updates for the dispatched communication.
		/// </summary>
		string? DeliveryReportCallbackUrl { get; set; }
		/// <summary>
		/// A resolver that will return The URL to call for status updates for the dispatched communication.
		/// </summary>
		Func<IDispatchCommunicationContext, Task<string?>>? DeliveryReportCallbackUrlResolver { get; set; }
	}
}
