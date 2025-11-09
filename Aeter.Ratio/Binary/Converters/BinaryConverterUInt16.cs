/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterUInt16 : IBinaryConverter<UInt16>
    {
        public UInt16 Convert(Span<byte> value)
        {
            return Convert(value, 0, value.Length);
        }

        public UInt16 Convert(Span<byte> value, int startIndex)
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(value.Slice(startIndex));
        }

        public UInt16 Convert(Span<byte> value, int startIndex, int length)
        {
            return Convert(value, startIndex);
        }

        public byte[] Convert(UInt16 value)
        {
            return BitConverter.GetBytes(value);
        }

        object IBinaryConverter.Convert(Span<byte> value)
        {
            return Convert(value, 0, value.Length);
        }

        object IBinaryConverter.Convert(Span<byte> value, int startIndex)
        {
            return Convert(value, startIndex, value.Length - startIndex);
        }

        object IBinaryConverter.Convert(Span<byte> value, int startIndex, int length)
        {
            return Convert(value, startIndex, length);
        }

        byte[] IBinaryConverter.Convert(object value)
        {
            return Convert((UInt16)value);
        }

        public void Convert(UInt16 value, Span<byte> buffer)
        {
            Convert(value, buffer, 0);
        }

        public void Convert(UInt16 value, Span<byte> buffer, int offset)
        {
            if (buffer.Length < offset + 2)
                throw new BufferOverflowException("The buffer can not contain the value");
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset), value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((UInt16)value, buffer, 0);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer, int offset)
        {
            Convert((UInt16)value, buffer, offset);
        }

        public void Convert(UInt16 value, BinaryWriteBuffer writeBuffer)
        {
            var bytes = Convert(value);
            writeBuffer.Write(bytes);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((UInt16)value, writeBuffer);
        }

    }
}
