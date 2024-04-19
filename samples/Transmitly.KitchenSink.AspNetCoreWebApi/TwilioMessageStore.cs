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
using System.Collections.Concurrent;
using Transmitly.Exceptions;

namespace Transmitly.KitchenSink.AspNetCoreWebApi
{
	/// <summary>
	/// Temporarily stores twilio specific messages.
	/// </summary>
	/// <remarks>
	/// Typically, you'd store your twilio specific messages in some kind of 
	/// cache or data store (redis, in memory, database)
	/// </remarks>
	public static class TwilioMessageStore
	{
		static readonly ConcurrentDictionary<string, string> _messages = new();

		public static void StoreMessage(string key, string message)
		{
			_messages.TryAdd(key, message);
		}

		public static string GetMessage(string key)
		{
			if (_messages.TryRemove(key, out var value))
			{
				return value;
			}

			throw new CommunicationsException("Message not found.");
		}

	}
}