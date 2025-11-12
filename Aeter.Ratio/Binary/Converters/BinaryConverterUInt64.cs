/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterUInt64 : IBinaryConverter<UInt64>
    {
        private const int Size = sizeof(UInt64);

        public UInt64 Convert(ReadOnlySpan<byte> value)
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(value);
        }

        public byte[] Convert(UInt64 value)
        {
            return BitConverter.GetBytes(value);
        }

        public void Convert(UInt64 value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
        }

        public void Convert(UInt64 value, BinaryWriteBuffer writeBuffer)
        {
            var bytes = Convert(value);
            writeBuffer.Write(bytes);
        }

        object IBinaryConverter.Convert(ReadOnlySpan<byte> value)
        {
            return Convert(value);
        }

        byte[] IBinaryConverter.Convert(object value)
        {
            return Convert((UInt64)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((UInt64)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((UInt64)value, writeBuffer);
        }

    }
}
