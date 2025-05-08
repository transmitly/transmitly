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
	public sealed record CommunicationsStatus
	{
		public const int InfoMin = 1000, InfoMax = 1999;
		public const int SuccessMin = 2000, SuccessMax = 2999;
		public const int ClientErrMin = 4000, ClientErrMax = 4999;
		public const int ServerErrMin = 5000, ServerErrMax = 5999;

		internal const string TransmitlyCallerId = "Transmitly";
		private string OwnerId { get; }
		public int Code { get; }
		public string Detail { get; }

		internal CommunicationsStatus(string ownerId, string reasonPhrase, int code = 0)
		{
			OwnerId = ownerId;
			Code = code;
			Detail = reasonPhrase;
		}

		internal static CommunicationsStatus Info(string reasonPhrase, int subCode = 0)
		  => Create(TransmitlyCallerId, InfoMin, InfoMax, subCode, reasonPhrase, true);

		internal static CommunicationsStatus Success(string reasonPhrase, int subCode = 0)
		  => Create(TransmitlyCallerId, SuccessMin, SuccessMax, subCode, reasonPhrase, true);

		internal static CommunicationsStatus ClientError(string reasonPhrase, int subCode = 0)
		  => Create(TransmitlyCallerId, ClientErrMin, ClientErrMax, subCode, reasonPhrase, true);

		internal static CommunicationsStatus ServerError(string reasonPhrase, int subCode = 0)
		  => Create(TransmitlyCallerId, ServerErrMin, ServerErrMax, subCode, reasonPhrase, true);

		public static CommunicationsStatus Info(string callerId, string reasonPhrase, int subCode = 0)
		  => Create(TransmitlyCallerId, InfoMin, InfoMax, subCode, reasonPhrase);

		public static CommunicationsStatus Success(string callerId, string reasonPhrase, int subCode = 0)
		  => Create(callerId, SuccessMin, SuccessMax, subCode, reasonPhrase);

		public static CommunicationsStatus ClientError(string callerId, string reasonPhrase, int subCode = 0)
		  => Create(callerId, ClientErrMin, ClientErrMax, subCode, reasonPhrase);

		public static CommunicationsStatus ServerError(string callerId, string reasonPhrase, int subCode = 0)
		  => Create(callerId, ServerErrMin, ServerErrMax, subCode, reasonPhrase);

		private static CommunicationsStatus Create(
			string ownerId,
			int min,
			int max,
			int subCode,
			string reasonPhrase,
			bool internalUsage = false)
		{
			int code = min + subCode;
			if (subCode < 0 || code > max)
				throw new ArgumentOutOfRangeException(
					nameof(subCode),
					$"Subcode must be between 0 and {max - min} for owner '{ownerId}'.");

			if (!internalUsage && ownerId.IndexOf(TransmitlyCallerId, StringComparison.InvariantCultureIgnoreCase) > -1)
				throw new ArgumentException(
					$"OwnerId '{ownerId}' cannot contain '{TransmitlyCallerId}' as it is reserved for Transmitly's internal use.",
					nameof(ownerId));

			var status = new CommunicationsStatus(ownerId, reasonPhrase, code);
			return status;
		}

		public override string ToString() => $"{OwnerId}:{Code} {Detail}";
	}
}