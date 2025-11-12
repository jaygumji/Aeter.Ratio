/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Aeter.Ratio.Binary
{
    /// <summary>
    /// Variable length packing algorithm. Value will range from 1-5 bytes
    /// </summary>
    public static class BinaryV32Packer
    {
        /// <summary>
        /// Makes the value take variable length, it takes as much as it needs ranging from 1-5 bytes
        /// </summary>
        /// <param name="stream">The stream where we write the value to</param>
        /// <param name="nullableValue">The value to pack</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackU(Stream stream, UInt32? nullableValue)
        {
            var length = nullableValue.HasValue ? GetULength(nullableValue.Value) : 1;
            var buffer = new byte[length];
            PackU(buffer, 0, length, nullableValue);
            stream.Write(buffer, 0, length);
        }

        /// <summary>
        /// The value is packed with a loss of 2 bits
        /// </summary>
        /// <param name="writeBuffer">The buffer where we write the value to.</param>
        /// <param name="nullableValue">Value</param>
        /// <exception cref="System.ArgumentOutOfRangeException">value;Must be between 0 and  + ZMaxValue</exception>
        public static void PackU(BinaryWriteBuffer writeBuffer, UInt32? nullableValue)
        {
            var length = nullableValue.HasValue ? GetULength(nullableValue.Value) : 1;
            var position = writeBuffer.Advance(length);
            PackU(writeBuffer.Span, position, length, nullableValue);
        }

        /// <summary>
        /// Makes the value take variable length, it takes as much as it needs ranging from 1-5 bytes
        /// </summary>
        /// <param name="buffer">The buffer where we write the value to</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="nullableValue">The value to pack</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PackU(Span<byte> buffer, int offset, int length, uint? nullableValue)
        {
            if (!nullableValue.HasValue) {
                buffer[offset] = 7;

                return;
            }
            var value = nullableValue.Value;

            var lengthMask = length - 1;

            buffer[offset] = (byte)((byte)(value << 27 >> 24) | lengthMask);

            if (length == 1) return;
            buffer[offset + 1] = (byte)(value << 19 >> 24);

            if (length == 2) return;
            buffer[offset + 2] = (byte)(value << 11 >> 24);

            if (length == 3) return;
            buffer[offset + 3] = (byte)(value << 3 >> 24);

            if (length == 4) return;
            buffer[offset + 4] = (byte)(value >> 29);
        }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.Int32.</returns>
        public static int GetULength(uint value)
        {
            if (value <= 0x1FU) return 1;
            if (value <= 0x1FFFU) return 2;
            if (value <= 0x1FFFFFU) return 3;
            return value <= 0x1FFFFFFFU ? 4 : 5;
        }

        /// <summary>
        /// Unpacks a value packed with the variable pack algorithm
        /// </summary>
        /// <param name="buffer">The stream where we read the value from</param>
        /// <returns>The unpacked value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint? UnpackU(BinaryReadBuffer buffer)
        {
            var b = (UInt32)buffer.ReadByte();
            if (b == 7) return null;

            var length = (byte)(b << 29 >> 29);

            var result = b >> 3;

            if (length == 0) return result;
            b = (UInt32)buffer.ReadByte();
            var part = b << 5;
            result |= part;

            if (length == 1) return result;
            b = (UInt32)buffer.ReadByte();
            part = b << 13;
            result |= part;

            if (length == 2) return result;
            b = (UInt32)buffer.ReadByte();
            part = b << 21;
            result |= part;

            if (length == 3) return result;
            b = (UInt32)buffer.ReadByte();
            part = b << 29;
            return result | part;
        }

        /// <summary>
        /// Unpacks a value packed with the variable pack algorithm
        /// </summary>
        /// <param name="stream">The stream where we read the value from</param>
        /// <returns>The unpacked value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint? UnpackU(Stream stream)
        {
            var b = (UInt32)stream.ReadByte();
            if (b == 7) return null;

            var length = (byte)(b << 29 >> 29);

            var result = b >> 3;

            if (length == 0) return result;
            b = (UInt32)stream.ReadByte();
            var part = b << 5;
            result |= part;

            if (length == 1) return result;
            b = (UInt32)stream.ReadByte();
            part = b << 13;
            result |= part;

            if (length == 2) return result;
            b = (UInt32)stream.ReadByte();
            part = b << 21;
            result |= part;

            if (length == 3) return result;
            b = (UInt32)stream.ReadByte();
            part = b << 29;
            return result | part;
        }

        /// <summary>
        /// Makes the value take variable length, it takes as much as it needs ranging from 1-5 bytes
        /// </summary>
        /// <param name="stream">The stream where we write the value to</param>
        /// <param name="nullableValue">The value to pack</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackS(Stream stream, Int32? nullableValue)
        {
            if (!nullableValue.HasValue) {
                stream.WriteByte(7);
                return;
            }

            var value = nullableValue.Value;
            var isNegative = value < 0;
            var bits = isNegative ? ((UInt32)value) ^ UInt32.MaxValue : ((UInt32)value);
            var signedBit = isNegative ? (byte)8U : (byte)0U;

            byte length;
            if (bits <= 0xFU) length = 0;
            else if (bits <= 0xFFFU) length = 1;
            else if (bits <= 0xFFFFFU) length = 2;
            else if (bits <= 0xFFFFFFFU) length = 3;
            else length = 4;

            var b = (byte)(bits << 28 >> 24);
            b = (byte)(b | length | signedBit);
            stream.WriteByte(b);

            if (length == 0) return;
            b = (byte)(bits << 20 >> 24);
            stream.WriteByte(b);

            if (length == 1) return;
            b = (byte)(bits << 12 >> 24);
            stream.WriteByte(b);

            if (length == 2) return;
            b = (byte)(bits << 4 >> 24);
            stream.WriteByte(b);

            if (length == 3) return;
            b = (byte)(bits >> 28);
            stream.WriteByte(b);
        }

        /// <summary>
        /// Unpacks a value packed with the variable pack algorithm
        /// </summary>
        /// <param name="stream">The stream where we read the value from</param>
        /// <returns>The unpacked value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32? UnpackS(Stream stream)
        {
            var b = (UInt32)stream.ReadByte();
            if (b == 7) return null;

            var length = (byte)(b << 29 >> 29);
            var isNegative = (b << 28 >> 31) == 1;

            var result = b >> 4;

            if (length == 0) return isNegative ? (Int32)(result ^ UInt32.MaxValue) : (Int32)result;
            b = (UInt32)stream.ReadByte();
            var part = b << 4;
            result |= part;

            if (length == 1) return isNegative ? (Int32)(result ^ UInt32.MaxValue) : (Int32)result;
            b = (UInt32)stream.ReadByte();
            part = b << 12;
            result |= part;

            if (length == 2) return isNegative ? (Int32)(result ^ UInt32.MaxValue) : (Int32)result;
            b = (UInt32)stream.ReadByte();
            part = b << 20;
            result |= part;

            if (length == 3) return isNegative ? (Int32)(result ^ UInt32.MaxValue) : (Int32)result;
            b = (UInt32)stream.ReadByte();
            part = b << 28;
            result |= part;

            return isNegative ? (Int32)(result ^ UInt32.MaxValue) : (Int32)result;
        }


    }
}