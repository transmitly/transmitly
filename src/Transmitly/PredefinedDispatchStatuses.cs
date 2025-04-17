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
	public static class PredefinedDispatchStatuses
	{
		internal static readonly DispatchResultStatus PipelineNotFound = DispatchResultStatus.ClientError("Pipeline Not Found", 4);
		internal static readonly DispatchResultStatus ChannelFiltersNotAllowed = DispatchResultStatus.ClientError("Channel Filters Not Allowed", 5);
		internal static readonly DispatchResultStatus DispatchChannelFilterMismatch = DispatchResultStatus.ClientError("Dispatch Request Channel Filter Mismatch", 5);
		internal static readonly DispatchResultStatus Unknown = DispatchResultStatus.ClientError("Unknown", 999);
	}
}