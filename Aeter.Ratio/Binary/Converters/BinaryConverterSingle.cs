/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterSingle : IBinaryConverter<Single>
    {
        public Single Convert(Span<byte> value)
        {
            return Convert(value, 0, value.Length);
        }

        public Single Convert(Span<byte> value, int startIndex)
        {
            var bits = BinaryPrimitives.ReadInt32LittleEndian(value.Slice(startIndex));
            return BitConverter.Int32BitsToSingle(bits);
        }

        public Single Convert(Span<byte> value, int startIndex, int length)
        {
            return Convert(value, startIndex);
        }

        public byte[] Convert(Single value)
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
            return Convert((Single)value);
        }

        public void Convert(Single value, Span<byte> buffer)
        {
            Convert(value, buffer, 0);
        }

        public void Convert(Single value, Span<byte> buffer, int offset)
        {
            if (buffer.Length < offset + 4)
                throw new BufferOverflowException("The buffer can not contain the value");
            var bits = BitConverter.SingleToInt32Bits(value);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset), bits);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((Single)value, buffer, 0);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer, int offset)
        {
            Convert((Single)value, buffer, offset);
        }

        public void Convert(Single value, BinaryWriteBuffer writeBuffer)
        {
            var bytes = Convert(value);
            writeBuffer.Write(bytes);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Single)value, writeBuffer);
        }

    }
}
