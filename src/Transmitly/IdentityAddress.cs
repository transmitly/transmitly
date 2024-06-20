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
    /// Represents an identity address.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="IdentityAddress"/> class.
    /// </remarks>
    [DebuggerStepThrough]
    public sealed class IdentityAddress : IIdentityAddress, IEquatable<IdentityAddress>
    {
        private const string ValueKey = "value";
        private const string DisplayKey = "display";
        
        /// <param name="addressParts"></param>
        /// <param name="attributes"></param>        
        /// <param name="type">Optional type for this address. See: <see cref="Transmitly.Channel.Push.IdentityAddressPushNotificationExtensions"/> for example of extending types.</param>
        public IdentityAddress(IDictionary<string, string?> addressParts, IDictionary<string, string?> attributes, string? type = null)
        {
            AddressParts = Guard.AgainstNull(addressParts);
            Attributes = Guard.AgainstNull(attributes);

            if(!AddressParts.TryGetValue(ValueKey, out var value))
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentNullException(ValueKey);
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
            Value = value!;
            
            Type = type;
        }

        public IdentityAddress(string value, string? display = null, IDictionary<string, string?>? attributes = null, string? type = null)
            : this(new Dictionary<string, string?>() { { ValueKey, Guard.AgainstNullOrWhiteSpace(value) }, { DisplayKey, display } }, attributes ?? new Dictionary<string, string?>(), type)
        {
        }

        /// <summary>
        /// Types of identity addresses defined by channels. See: <see cref="Transmitly.Channel.Push.IdentityAddressPushNotificationExtensions"/> for example of extending types.
        /// </summary>
        public readonly static IIdentityAddressType? Types;

        /// <summary>
        /// Gets or sets the value of the identity address.
        /// </summary>
        public string Value
        {
            get => AddressParts[ValueKey]!;
            set
            {
                Guard.AgainstNullOrWhiteSpace(value);

                if (!AddressParts.ContainsKey(ValueKey))
                    AddressParts.Add(ValueKey, value);
                else
                    AddressParts[ValueKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the display value of the identity address.
        /// </summary>
        public string? Display
        {
            get
            {
                if (AddressParts.TryGetValue(DisplayKey, out var display))
                    return display;
                return default;
            }
            set
            {
                if (!AddressParts.ContainsKey(DisplayKey))
                    AddressParts.Add(DisplayKey, value);
                else
                    AddressParts[DisplayKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of identity address. See: <see cref="Transmitly.Channel.Push.IdentityAddressPushNotificationExtensions"/> for example of extending types.
        /// </summary>
        public string? Type { get; set; }

        public IDictionary<string, string?> AddressParts { get; set; }

        public IDictionary<string, string?> Attributes { get; set; }

        /// <summary>
        /// Implicitly converts a string to an <see cref="IdentityAddress"/>.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <returns>The converted <see cref="IdentityAddress"/>.</returns>
        public static implicit operator IdentityAddress(string value) => new(value);

        /// <summary>
        /// Implicitly converts an <see cref="IdentityAddress"/> to a string.
        /// </summary>
        /// <param name="address">The <see cref="IdentityAddress"/> to convert.</param>
        /// <returns>The converted string.</returns>
        public static implicit operator string?(IdentityAddress address) => address?.Value;

        /// <summary>
        /// Determines whether the two specified operands are equal.
        /// </summary>
        /// <param name="address">The left hand operand in the equation.</param>
        /// <param name="other">The right hand operand in the equation.</param>
        /// <returns>True if equal, false if not.</returns>
        public static bool operator ==(IdentityAddress address, IdentityAddress other)
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
        public static bool operator !=(IdentityAddress left, IdentityAddress right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IdentityAddress"/> is equal to the specified identity address.
        /// </summary>
        /// <param name="other">The comparand identity address.</param>
        /// <returns>true if the objects are equal, false if they're not.</returns>
        public bool Equals(IdentityAddress? other)
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
        /// Gets a value indicating whether this <see cref="IdentityAddress"/> is equal to the specified object.
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

            return Equals((IdentityAddress)obj);
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