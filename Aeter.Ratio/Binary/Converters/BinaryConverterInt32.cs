/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterInt32 : IBinaryConverter<Int32>
    {
        private const int Size = sizeof(Int32);

        public Int32 Convert(ReadOnlySpan<byte> value)
        {
            return BinaryPrimitives.ReadInt32LittleEndian(value);
        }

        public byte[] Convert(Int32 value)
        {
            return BitConverter.GetBytes(value);
        }

        public void Convert(Int32 value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        }

        public void Convert(Int32 value, BinaryWriteBuffer writeBuffer)
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
            return Convert((Int32)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((Int32)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Int32)value, writeBuffer);
        }

    }
}
