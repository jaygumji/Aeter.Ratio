/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterInt64 : IBinaryConverter<Int64>
    {
        private const int Size = sizeof(Int64);

        public Int64 Convert(ReadOnlySpan<byte> value)
        {
            return BinaryPrimitives.ReadInt64LittleEndian(value);
        }

        public byte[] Convert(Int64 value)
        {
            return BitConverter.GetBytes(value);
        }

        public void Convert(Int64 value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
        }

        public void Convert(Int64 value, BinaryWriteBuffer writeBuffer)
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
            return Convert((Int64)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((Int64)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Int64)value, writeBuffer);
        }

    }
}
