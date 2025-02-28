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

namespace Tandely.Notifications.Client
{
    /// <summary>
    /// The status of a communication pipeline instance
    /// </summary>
    public enum PipelineInstanceStatus
    {
        /// <summary>
        /// The orignating command has yet to be processed for pipeline resolution
        /// </summary>
        PendingResolution,
        /// <summary>
        /// In the process of initiating the pipeline instance
        /// </summary>
        Initiating,
        /// <summary>
        /// Indicates the pipeline is accumulating additional conditional
        /// trigger data requirements before continuing forward in the pipeline.
        /// </summary>
        AccumulatingTriggerData,
        /// <summary>
        /// Indicates the pipeline has been triggered and is ready to begin dispatching.
        /// </summary>
        TriggeredForDispatch,
        /// <summary>
        /// Indicates the pipeline is configured to compose a set number of communication samples
        /// which require manual review before continuing with the pipeline dispatching process.
        /// </summary>
        DispatchPendingSampleReview,
        /// <summary>
        /// Indicates the pipeline is in the process of dispatching the communication.
        /// </summary>
        Dispatching,
        /// <summary>
        /// Indicates the pipeline has successfully dispatched the communication.
        /// </summary>
        Dispatched,
        /// <summary>
        /// Indicates the pipeline has failed to dispatch the communication.
        /// </summary>
        Failed
    }
}