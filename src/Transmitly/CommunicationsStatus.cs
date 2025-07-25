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

namespace Transmitly;

/// <summary>
/// CommunicationsStatus represents the status of a communication operation.
/// </summary>
public sealed record CommunicationsStatus
{
	public const int InfoMin = 1000, InfoMax = 1999;
	public const int SuccessMin = 2000, SuccessMax = 2999;
	public const int ClientErrMin = 4000, ClientErrMax = 4999;
	public const int ServerErrMin = 5000, ServerErrMax = 5999;

	internal const string TransmitlyCallerId = "Transmitly";
	private string OwnerId { get; }
	public int Code { get; }
	public string Type { get; }
	public string? Detail { get; }

	internal CommunicationsStatus(string ownerId, string type, int code = 0, string? detail = null)
	{
		OwnerId = ownerId;
		Code = code;
		Type = type;
		Detail = detail;
	}

	internal static CommunicationsStatus Info(string reasonPhrase, int subCode = 0, string? detail = null)
	  => Create(TransmitlyCallerId, InfoMin, InfoMax, subCode, reasonPhrase, detail, true);

	internal static CommunicationsStatus Success(string reasonPhrase, int subCode = 0, string? detail = null)
	  => Create(TransmitlyCallerId, SuccessMin, SuccessMax, subCode, reasonPhrase, detail, true);

	internal static CommunicationsStatus ClientError(string reasonPhrase, int subCode = 0, string? detail = null)
	  => Create(TransmitlyCallerId, ClientErrMin, ClientErrMax, subCode, reasonPhrase, detail, true);

	internal static CommunicationsStatus ServerError(string reasonPhrase, int subCode = 0, string? detail = null)
	  => Create(TransmitlyCallerId, ServerErrMin, ServerErrMax, subCode, reasonPhrase, detail, true);

	/// <summary>
	/// Creates a new Info typed CommunicationsStatus with the specified parameters.
	/// </summary>
	/// <param name="callerId"></param>
	/// <param name="reasonPhrase"></param>
	/// <param name="subCode"></param>
	/// <param name="detail"></param>
	/// <returns></returns>
	public static CommunicationsStatus Info(string callerId, string reasonPhrase, int subCode = 0, string? detail = null)
	  => Create(callerId, InfoMin, InfoMax, subCode, reasonPhrase, detail);

	/// <summary>
	/// Creates a new Success typed CommunicationsStatus with the specified parameters.
	/// </summary>
	/// <param name="callerId"></param>
	/// <param name="reasonPhrase"></param>
	/// <param name="subCode"></param>
	/// <param name="detail"></param>
	/// <returns></returns>
	public static CommunicationsStatus Success(string callerId, string reasonPhrase, int subCode = 0, string? detail = null)
	  => Create(callerId, SuccessMin, SuccessMax, subCode, reasonPhrase, detail);

	/// <summary>
	/// Creates a new ClientError typed CommunicationsStatus with the specified parameters.
	/// </summary>
	/// <param name="callerId"></param>
	/// <param name="reasonPhrase"></param>
	/// <param name="subCode"></param>
	/// <param name="detail"></param>
	/// <returns></returns>
	public static CommunicationsStatus ClientError(string callerId, string reasonPhrase, int subCode = 0, string? detail = null)
	  => Create(callerId, ClientErrMin, ClientErrMax, subCode, reasonPhrase, detail);

	/// <summary>
	/// Creates a new ServerError typed CommunicationsStatus with the specified parameters.
	/// </summary>
	/// <param name="callerId"></param>
	/// <param name="reasonPhrase"></param>
	/// <param name="subCode"></param>
	/// <param name="detail"></param>
	/// <returns></returns>
	public static CommunicationsStatus ServerError(string callerId, string reasonPhrase, int subCode = 0, string? detail = null)
	  => Create(callerId, ServerErrMin, ServerErrMax, subCode, reasonPhrase, detail);

	private static CommunicationsStatus Create(
		string ownerId,
		int min,
		int max,
		int subCode,
		string type,
		string? detail = null,
		bool internalUsage = false
		)
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

		var status = new CommunicationsStatus(ownerId, type, code, detail);
		return status;
	}

	public override string ToString() => $"{OwnerId}:{Type} ({Code}) {Detail}".Trim();
}