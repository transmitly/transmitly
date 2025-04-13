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

namespace Transmitly.Channel.Configuration
{
	/// <summary>
	/// Channel specific configuration.
	/// </summary>
	public interface IChannelConfiguration
	{
		/// <summary>
		/// Gets the list of address purposes to use for the dispatching recipients. 
		/// <para>To set collection see: <see cref="AddRecipientAddressPurpose(string[])"/></para>
		/// </summary>
		IReadOnlyCollection<string>? RecipientAddressPurposes { get; }
		/// <summary>
		/// Gets the list of address purposes to use for the dispatched blind copy recipients.
		/// <para>To set collection see: <see cref="AddBlindCopyRecipientAddressPurpose(string[])"/></para>
		/// </summary>
		IReadOnlyCollection<string>? BlindCopyRecipientPurposes { get; }
		/// <summary>
		/// Gets the list of address purposes to use for the dispatched copy recipients.
		/// <para>To set collection see: <see cref="AddCopyRecipientAddressPurpose(string[])"/></para>
		/// </summary>
		IReadOnlyCollection<string>? CopyRecipientPurposes { get; }
		/// <summary>
		/// Gets the list of channel providers that are allowed to be used for dispatching.
		/// <para>To set collection see: <see cref="AddChannelProviderFilter(string[])"/></para>
		/// </summary>
		IReadOnlyCollection<string>? ChannelProviderFilter { get; }
		/// <summary>
		/// A resolver that will return The URL to call for status updates for the dispatched communication.
		/// <para>To set see: <see cref="AddDeliveryReportCallbackUrlResolver(Func{IDispatchCommunicationContext, Task{string?}})"/></para>
		/// </summary>
		Func<IDispatchCommunicationContext, Task<string?>>? DeliveryReportCallbackUrlResolver { get; }
		/// <summary>
		/// Sets the list of address purposes to use for the dispatching recipients.
		/// </summary>
		/// <param name="purposes">An identity addresses' purpose. (<see cref="IIdentityAddress.Purposes"/>)</param>
		/// <returns></returns>
		IChannelConfiguration AddRecipientAddressPurpose(params string[] purposes);
		/// <summary>
		/// Sets the list of address purposes to use for the dispatching recipients.
		/// </summary>
		/// <param name="purposes">An identity addresses' purpose. (<see cref="IIdentityAddress.Purposes"/>)</param>
		/// <returns></returns>
		IChannelConfiguration AddBlindCopyRecipientAddressPurpose(params string[] purposes);
		/// <summary>
		/// Sets the list of address purposes to use for the dispatching recipients.
		/// </summary>
		/// <param name="purposes">An identity addresses' purpose. (<see cref="IIdentityAddress.Purposes"/>)</param>
		/// <returns></returns>
		IChannelConfiguration AddCopyRecipientAddressPurpose(params string[] purposes);
		/// <summary>
		/// Adds an optional resolver that will return the URL to call for status updates for the dispatched communication if supported by the channel provider.
		/// </summary>
		/// <param name="callbackResolver">The func to resolve the callback URL.</param>
		/// <returns></returns>
		IChannelConfiguration AddDeliveryReportCallbackUrlResolver(Func<IDispatchCommunicationContext, Task<string?>> callbackResolver);
		/// <summary>
		/// Sets the list of channel provider <see cref="Id.ChannelProvider">ids</see> that are allowed to be used for this channel.
		/// </summary>
		/// <param name="channelProviderIds">A list of allowed channel provider <see cref="Id.ChannelProvider">Id's</see>.</param>
		/// <returns></returns>
		IChannelConfiguration AddChannelProviderFilter(params string[] channelProviderIds);
	}
}