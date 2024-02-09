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

using System.Diagnostics;

namespace Transmitly
{
	/// <summary>
	/// Represents an audience address.
	/// </summary>
	/// <remarks>
	/// Initializes a new instance of the <see cref="AudienceAddress"/> class.
	/// </remarks>
	/// <param name="value">The value of the audience address.</param>
	/// <param name="display">The display value of the audience address.</param>
	[DebuggerStepThrough]
	public sealed class AudienceAddress(string value, string? display = null, string? type = null) : IAudienceAddress, IEquatable<AudienceAddress>
	{
		/// <summary>
		/// Types of audience addresses defined by channels. See: <see cref="Transmitly.Channel.Push.AudienceAddressPushNotificationExtensions"/> for example of extending types.
		/// </summary>
		public readonly static IAudienceAddressType? Types;

		/// <summary>
		/// Gets or sets the value of the audience address.
		/// </summary>
		public string Value { get; set; } = Guard.AgainstNullOrWhiteSpace(value);

		/// <summary>
		/// Gets or sets the display value of the audience address.
		/// </summary>
		public string? Display { get; set; } = display;

		public string? Type { get; set; } = type;

		/// <summary>
		/// Implicitly converts a string to an <see cref="AudienceAddress"/>.
		/// </summary>
		/// <param name="value">The string value to convert.</param>
		/// <returns>The converted <see cref="AudienceAddress"/>.</returns>
		public static implicit operator AudienceAddress(string value) => new(value);

		/// <summary>
		/// Implicitly converts an <see cref="AudienceAddress"/> to a string.
		/// </summary>
		/// <param name="address">The <see cref="AudienceAddress"/> to convert.</param>
		/// <returns>The converted string.</returns>
		public static implicit operator string?(AudienceAddress address) => address?.Value;

		/// <summary>
		/// Determines whether the two specified operands are equal.
		/// </summary>
		/// <param name="left">The left hand operand in the equation.</param>
		/// <param name="right">The right hand operand in the equation.</param>
		/// <returns>True if equal, false if not.</returns>
		public static bool operator ==(AudienceAddress address, AudienceAddress other)
		{
			if (address is null && other is null)
			{
				return true;
			}
			return address?.Equals(other) ?? false;
		}

		/// <summary>
		/// Determines whether the two specified operands are not equal.
		/// </summary>
		/// <param name="left">The left hand operand in the equation.</param>
		/// <param name="right">The right hand operand in the equation.</param>
		/// <returns>True if the two operands are not equal, and false if they are.</returns>
		public static bool operator !=(AudienceAddress left, AudienceAddress right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="AudienceAddress"/> is equal to the specified AudienceAddress.
		/// </summary>
		/// <param name="other">The comparand audience address.</param>
		/// <returns>true if the objects are equal, false if they're not.</returns>
		public bool Equals(AudienceAddress? other)
		{
			if (other is null)
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="AudienceAddress"/> is equal to the specified object.
		/// </summary>
		/// <param name="obj">The comparand object.</param>
		/// <returns>true if the objects are equal, false if they're not.</returns>
		public override bool Equals(object? obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != GetType())
			{
				return false;
			}

			return Equals((AudienceAddress)obj);
		}

		/// <summary>
		/// Gets a hash code representing this object.
		/// </summary>
		/// <returns>A hash code representing this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (Value != null ? Value.ToLower().GetHashCode() : 0);
			}
		}
	}
}