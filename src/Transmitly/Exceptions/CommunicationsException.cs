﻿// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
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

using System.Runtime.Serialization;

namespace Transmitly.Exceptions;

/// <summary>
/// A general transmitly communication exception
/// </summary>
[Serializable]
public class CommunicationsException : Exception
{
	public CommunicationsException(string message) : base(message)
	{
	}

	public CommunicationsException(string message, Exception innerException) : base(message, innerException)
	{
	}
	public CommunicationsException()
	{

	}
#if NET8_0_OR_GREATER
	//https://aka.ms/dotnet-warnings/SYSLIB0051
	[Obsolete("https://aka.ms/dotnet-warnings/SYSLIB0051", DiagnosticId = "SYSLIB0051")]
#endif
	protected CommunicationsException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
}
