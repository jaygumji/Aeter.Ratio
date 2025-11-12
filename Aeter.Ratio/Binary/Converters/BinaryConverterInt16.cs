/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterInt16 : IBinaryConverter<Int16>
    {
        private const int Size = sizeof(Int16);

        public Int16 Convert(ReadOnlySpan<byte> value)
        {
            return BinaryPrimitives.ReadInt16LittleEndian(value);
        }

        public byte[] Convert(Int16 value)
        {
            return BitConverter.GetBytes(value);
        }

        public void Convert(Int16 value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
        }

        public void Convert(Int16 value, BinaryWriteBuffer writeBuffer)
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
            return Convert((Int16)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((Int16)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Int16)value, writeBuffer);
        }

    }
}
