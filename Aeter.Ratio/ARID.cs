/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Text;

namespace Aeter.Ratio
{
    /// <summary>
    /// Represents an Aeter Ratio Identifier (ARID). Ensures the value is non-empty and limited to a-z, 0-9 or '-'.
    /// </summary>
    public sealed class ARID : IEquatable<ARID>
    {
        private const int MaxLength = 10;
        private readonly string _value;

        public ARID(string value)
        {
            _value = NormalizeAndValidate(value);
        }

        public int Length => _value.Length;

        public static implicit operator string(ARID id) => id._value;

        public static explicit operator ARID(string value) => new ARID(value);

        public static ARID ReadFrom(ReadOnlySpan<byte> source)
        {
            if (source.Length == 0) {
                throw new ArgumentException("Source span cannot be empty.", nameof(source));
            }

            var value = Encoding.UTF8.GetString(source);
            return new ARID(value);
        }

        public override string ToString() => _value;

        public override bool Equals(object? obj)
            => obj is ARID other && Equals(other);

        public bool Equals(ARID? other)
            => other is not null && string.Equals(_value, other._value, StringComparison.Ordinal);

        public override int GetHashCode()
            => StringComparer.Ordinal.GetHashCode(_value);

        public static bool operator ==(ARID? left, ARID? right)
            => Equals(left, right);

        public static bool operator !=(ARID? left, ARID? right)
            => !Equals(left, right);

        public void WriteTo(Span<char> destination)
        {
            if (destination.Length < _value.Length) {
                throw new ArgumentException("Destination span is too small.", nameof(destination));
            }

            _value.AsSpan().CopyTo(destination);
        }

        public int WriteTo(Span<byte> destination)
        {
            var required = Encoding.UTF8.GetByteCount(_value);
            if (destination.Length < required) {
                throw new ArgumentException("Destination span is too small.", nameof(destination));
            }

            Encoding.UTF8.GetBytes(_value, destination);
            return required;
        }

        private static string NormalizeAndValidate(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) {
                throw new ArgumentException("ARID value cannot be null or empty.", nameof(value));
            }

            var trimmed = value.Trim();
            if (trimmed.Length > MaxLength) {
                throw new ArgumentException($"ARID value must not exceed {MaxLength} characters.", nameof(value));
            }

            foreach (var ch in trimmed) {
                if (!IsAllowed(ch)) {
                    throw new ArgumentException($"ARID value '{value}' contains invalid character '{ch}'.", nameof(value));
                }
            }

            return trimmed;
        }

        private static bool IsAllowed(char ch)
            => (ch >= 'a' && ch <= 'z') ||
               (ch >= '0' && ch <= '9') ||
               ch == '-';
    }
}
