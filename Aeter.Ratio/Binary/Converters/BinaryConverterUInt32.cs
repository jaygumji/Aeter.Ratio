/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterUInt32 : IBinaryConverter<UInt32>
    {
        public UInt32 Convert(Span<byte> value)
        {
            return Convert(value, 0, value.Length);
        }

        public UInt32 Convert(Span<byte> value, int startIndex)
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(value.Slice(startIndex));
        }

        public UInt32 Convert(Span<byte> value, int startIndex, int length)
        {
            return Convert(value, startIndex);
        }

        public byte[] Convert(UInt32 value)
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
            return Convert((UInt32)value);
        }

        public void Convert(UInt32 value, Span<byte> buffer)
        {
            Convert(value, buffer, 0);
        }

        public void Convert(UInt32 value, Span<byte> buffer, int offset)
        {
            if (buffer.Length < offset + 4)
                throw new BufferOverflowException("The buffer can not contain the value");
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset), value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((UInt32)value, buffer, 0);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer, int offset)
        {
            Convert((UInt32)value, buffer, offset);
        }

        public void Convert(UInt32 value, BinaryWriteBuffer writeBuffer)
        {
            var bytes = Convert(value);
            writeBuffer.Write(bytes);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((UInt32)value, writeBuffer);
        }

    }
}
