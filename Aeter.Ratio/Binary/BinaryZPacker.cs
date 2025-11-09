/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Aeter.Ratio.Binary
{

    public static class BinarySVNPacker
    {
        public static void Pack(Stream stream, Int32? value)
        {

        }
    }

    public static class BinarySVPacker
    {
        public static void Pack(Stream stream, Int32 value)
        {
            
        }
    }

    /// <summary>
    /// Classification BinaryZPacker.
    /// </summary>
    public static class BinaryZPacker
    {

        internal const byte Null = byte.MinValue;
        internal const byte VariabelLength = byte.MaxValue;

        // 0011 1111 1111 1111 1111 1111 1111 1111
        private const UInt32 ZMaxValue = 0x3FFFFFFF;

        /// <summary>
        /// The value is packed with a loss of 2 bits
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Packed value in binary format</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Pack(UInt32 value)
        {
            using (var stream = new MemoryStream(4)) {
                Pack(stream, value);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// The value is packed with a loss of 2 bits
        /// </summary>
        /// <param name="stream">The stream where we write the value to</param>
        /// <param name="value">Value</param>
        public static void Pack(Stream stream, UInt32 value)
        {
            if (value > ZMaxValue)
                throw new ArgumentOutOfRangeException("value", value, "Must be between 0 and " + ZMaxValue);

            var length = GetLength(value);
            var buffer = new byte[length];
            Pack(buffer, 0, length, value);
            stream.Write(buffer, 0, length);
        }

        /// <summary>
        /// The value is packed with a loss of 2 bits
        /// </summary>
        /// <param name="writeBuffer">The buffer where we write the value to.</param>
        /// <param name="value">Value</param>
        /// <exception cref="System.ArgumentOutOfRangeException">value;Must be between 0 and  + ZMaxValue</exception>
        public static void Pack(BinaryWriteBuffer writeBuffer, UInt32 value)
        {
            if (value > ZMaxValue)
                throw new ArgumentOutOfRangeException("value", value, "Must be between 0 and " + ZMaxValue);

            var length = GetLength(value);
            var position = writeBuffer.Advance(length);
            Pack(writeBuffer.Span, position, length, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Pack(Span<byte> buffer, int offset, int length, uint value)
        {
            if (value > ZMaxValue)
                throw new ArgumentOutOfRangeException("value", value, "Must be between 0 and " + ZMaxValue);

            var lengthMask = length - 1;

            buffer[offset] = (byte)((byte)(value << 26 >> 24) | lengthMask);

            if (length == 1) return;
            buffer[offset+1] = (byte)(value << 18 >> 24);

            if (length == 2) return;
            buffer[offset+2] = (byte)(value << 10 >> 24);

            if (length == 3) return;
            buffer[offset+3] = (byte)(value << 2 >> 24);

            return;
        }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.Int32.</returns>
        public static int GetLength(UInt32 value)
        {
            if (value <= 0x3F) return 1;
            if (value <= 0x3FFF) return 2;
            return value <= 0x3FFFFF ? 3 : 4;
        }

        /// <summary>
        /// Unpacks the value which was packed with <see cref="PackZ"/>
        /// </summary>
        /// <param name="buffer">The buffer containing the value</param>
        /// <param name="offset">Offset in the buffer where the value begins</param>
        /// <returns>The unpacked value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Unpack(byte[] buffer, int offset)
        {
            using (var stream = new MemoryStream(buffer)) {
                if (offset > 0)
                    stream.Seek(offset, SeekOrigin.Begin);

                return Unpack(stream);
            }
        }

        /// <summary>
        /// Unpacks the value which was packed with <see cref="Pack"/>
        /// </summary>
        /// <param name="stream">The stream containing the value</param>
        /// <returns>The unpacked value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Unpack(Stream stream)
        {
            var b = (UInt32)stream.ReadByte();
            var length = (byte)(b << 30 >> 30);

            var result = b >> 2;

            if (length == 0) return result;
            b = (UInt32)stream.ReadByte();
            var part = b << 6;
            result |= part;

            if (length == 1) return result;
            b = (UInt32)stream.ReadByte();
            part = b << 14;
            result |= part;

            if (length == 2) return result;
            b = (UInt32)stream.ReadByte();
            part = b << 22;
            return result | part;
        }
    }
}
