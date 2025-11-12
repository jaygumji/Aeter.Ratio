/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterUInt16 : IBinaryConverter<UInt16>
    {
        private const int Size = sizeof(UInt16);

        public UInt16 Convert(ReadOnlySpan<byte> value)
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(value);
        }

        public byte[] Convert(UInt16 value)
        {
            return BitConverter.GetBytes(value);
        }

        public void Convert(UInt16 value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        }

        public void Convert(UInt16 value, BinaryWriteBuffer writeBuffer)
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
            return Convert((UInt16)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((UInt16)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((UInt16)value, writeBuffer);
        }

    }
}
