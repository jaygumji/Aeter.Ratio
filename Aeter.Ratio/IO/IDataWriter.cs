/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using Aeter.Ratio.Binary.Information;

namespace Aeter.Ratio.IO
{
    /// <summary>
    /// Provides typed write operations for binary payloads.
    /// </summary>
    public interface IDataWriter
    {
        /// <summary>Writes a single byte.</summary>
        void Write(Byte value);
        /// <summary>Writes a 16-bit signed integer.</summary>
        void Write(Int16 value);
        /// <summary>Writes a 32-bit signed integer.</summary>
        void Write(Int32 value);
        /// <summary>Writes a 64-bit signed integer.</summary>
        void Write(Int64 value);
        /// <summary>Writes a 16-bit unsigned integer.</summary>
        void Write(UInt16 value);
        /// <summary>Writes a 32-bit unsigned integer.</summary>
        void Write(UInt32 value);
        /// <summary>Writes a 64-bit unsigned integer.</summary>
        void Write(UInt64 value);
        /// <summary>Writes a Boolean value.</summary>
        void Write(Boolean value);
        /// <summary>Writes a 32-bit floating-point value.</summary>
        void Write(Single value);
        /// <summary>Writes a 64-bit floating-point value.</summary>
        void Write(Double value);
        /// <summary>Writes a decimal value.</summary>
        void Write(Decimal value);
        /// <summary>Writes a <see cref="TimeSpan"/>.</summary>
        void Write(TimeSpan value);
        /// <summary>Writes a <see cref="DateTime"/>.</summary>
        void Write(DateTime value);
        /// <summary>Writes a string.</summary>
        void Write(String value);
        /// <summary>Writes a <see cref="Guid"/>.</summary>
        void Write(Guid value);
        /// <summary>Writes a binary blob as-is.</summary>
        void Write(byte[] value);
    }
}
