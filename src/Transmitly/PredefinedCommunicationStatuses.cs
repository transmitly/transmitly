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
	public static class PredefinedCommunicationStatuses
	{
		internal static readonly CommunicationsStatus PipelineNotFound = CommunicationsStatus.ClientError("Pipeline Not Found", 4);
		internal static readonly CommunicationsStatus DispatchRequirementsNotAllowed = CommunicationsStatus.ClientError("Pipeline Does Not Allow Channel Filters", 5);
		internal static readonly CommunicationsStatus DispatchChannelFilterMismatch = CommunicationsStatus.ClientError("Dispatch Request Channel Filter Mismatch", 6);
		internal static readonly CommunicationsStatus Unknown = CommunicationsStatus.ClientError("Unknown", 999);

	}
}