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

namespace Transmitly.Logging;

internal static class LogEvents
{
	/// <summary>
	/// Event emitted when logging is enabled for the communications client builder.
	/// </summary>
	internal const string LoggingEnabled = "logging.enabled";

	/// <summary>
	/// Event emitted when a dispatch operation starts.
	/// </summary>
	internal const string DispatchStarted = "dispatch.started";

	/// <summary>
	/// Event emitted when a dispatch operation completes.
	/// </summary>
	internal const string DispatchCompleted = "dispatch.completed";

	/// <summary>
	/// Event emitted when a dispatch operation is cancelled.
	/// </summary>
	internal const string DispatchCancelled = "dispatch.cancelled";

	/// <summary>
	/// Event emitted when a dispatch operation fails.
	/// </summary>
	internal const string DispatchFailed = "dispatch.failed";

	/// <summary>
	/// Event emitted when pipeline lookup begins.
	/// </summary>
	internal const string PipelineLookupStarted = "pipeline.lookup.started";

	/// <summary>
	/// Event emitted when pipeline lookup finishes.
	/// </summary>
	internal const string PipelineLookupCompleted = "pipeline.lookup.completed";

	/// <summary>
	/// Event emitted when pipeline lookup returns no matches.
	/// </summary>
	internal const string PipelineLookupEmpty = "pipeline.lookup.empty";

	/// <summary>
	/// Event emitted when platform identity resolution begins.
	/// </summary>
	internal const string IdentityResolutionStarted = "identity.resolve.started";

	/// <summary>
	/// Event emitted when platform identity resolution completes.
	/// </summary>
	internal const string IdentityResolutionCompleted = "identity.resolve.completed";

	/// <summary>
	/// Event emitted when platform identity profile enrichment begins.
	/// </summary>
	internal const string IdentityProfileEnrichmentStarted = "identity.profile.enrich.started";

	/// <summary>
	/// Event emitted when platform identity profile enrichment completes.
	/// </summary>
	internal const string IdentityProfileEnrichmentCompleted = "identity.profile.enrich.completed";

	/// <summary>
	/// Event emitted when recipient dispatch contexts are created.
	/// </summary>
	internal const string RecipientContextsCreated = "recipient.contexts.created";

	/// <summary>
	/// Event emitted when content enrichment begins.
	/// </summary>
	internal const string ContentEnrichmentStarted = "content.enrich.started";

	/// <summary>
	/// Event emitted when content enrichment completes.
	/// </summary>
	internal const string ContentEnrichmentCompleted = "content.enrich.completed";

	/// <summary>
	/// Event emitted when channel rendering begins.
	/// </summary>
	internal const string ChannelRenderingStarted = "channel.render.started";

	/// <summary>
	/// Event emitted when channel rendering completes.
	/// </summary>
	internal const string ChannelRenderingCompleted = "channel.render.completed";

	/// <summary>
	/// Event emitted when channel rendering fails.
	/// </summary>
	internal const string ChannelRenderingFailed = "channel.render.failed";

	/// <summary>
	/// Event emitted when a pipeline delivery strategy begins dispatching.
	/// </summary>
	internal const string StrategyDispatchStarted = "strategy.dispatch.started";

	/// <summary>
	/// Event emitted when a pipeline delivery strategy finishes dispatching.
	/// </summary>
	internal const string StrategyDispatchCompleted = "strategy.dispatch.completed";

	/// <summary>
	/// Event emitted when provider dispatch begins.
	/// </summary>
	internal const string ProviderDispatchStarted = "provider.dispatch.started";

	/// <summary>
	/// Event emitted when provider dispatch completes.
	/// </summary>
	internal const string ProviderDispatchCompleted = "provider.dispatch.completed";

	/// <summary>
	/// Event emitted when provider dispatch fails.
	/// </summary>
	internal const string ProviderDispatchFailed = "provider.dispatch.failed";

	/// <summary>
	/// Event emitted when delivery reports are dispatched.
	/// </summary>
	internal const string DeliveryReportDispatch = "deliveryreport.dispatch";

	/// <summary>
	/// Event emitted when dispatch middleware is invoked.
	/// </summary>
	internal const string MiddlewareInvoked = "middleware.invoked";
}
