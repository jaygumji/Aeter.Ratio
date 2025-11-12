/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.IO
{
    /// <summary>
    /// Provides typed read operations for binary payloads.
    /// </summary>
    public interface IDataReader
    {
        /// <summary>
        /// Reads a single byte.
        /// </summary>
        Byte ReadByte();
        /// <summary>
        /// Reads a 16-bit signed integer.
        /// </summary>
        Int16 ReadInt16();
        /// <summary>
        /// Reads a 32-bit signed integer.
        /// </summary>
        Int32 ReadInt32();
        /// <summary>
        /// Reads a 64-bit signed integer.
        /// </summary>
        Int64 ReadInt64();
        /// <summary>
        /// Reads a 16-bit unsigned integer.
        /// </summary>
        UInt16 ReadUInt16();
        /// <summary>
        /// Reads a 32-bit unsigned integer.
        /// </summary>
        UInt32 ReadUInt32();
        /// <summary>
        /// Reads a 64-bit unsigned integer.
        /// </summary>
        UInt64 ReadUInt64();
        /// <summary>
        /// Reads a Boolean value.
        /// </summary>
        Boolean ReadBoolean();
        /// <summary>
        /// Reads a 32-bit floating-point value.
        /// </summary>
        Single ReadSingle();
        /// <summary>
        /// Reads a 64-bit floating-point value.
        /// </summary>
        Double ReadDouble();
        /// <summary>
        /// Reads a decimal value.
        /// </summary>
        Decimal ReadDecimal();
        /// <summary>
        /// Reads a <see cref="TimeSpan"/>.
        /// </summary>
        TimeSpan ReadTimeSpan();
        /// <summary>
        /// Reads a <see cref="DateTime"/>.
        /// </summary>
        DateTime ReadDateTime();
        /// <summary>
        /// Reads a length-prefixed UTF-8 string.
        /// </summary>
        String ReadString();
        /// <summary>
        /// Reads a UTF-8 string of the specified length.
        /// </summary>
        /// <param name="length">Number of bytes to read.</param>
        String ReadString(uint length);
        /// <summary>
        /// Reads a <see cref="Guid"/>.
        /// </summary>
        Guid ReadGuid();
        /// <summary>
        /// Reads a length-prefixed blob.
        /// </summary>
        byte[] ReadBlob();
        /// <summary>
        /// Reads a blob of the specified length.
        /// </summary>
        /// <param name="length">Number of bytes to read.</param>
        byte[] ReadBlob(uint length);
    }
}
