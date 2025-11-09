/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterDouble : IBinaryConverter<Double>
    {
        public Double Convert(Span<byte> value)
        {
            return Convert(value, 0, value.Length);
        }

        public Double Convert(Span<byte> value, int startIndex)
        {
            var bits = BinaryPrimitives.ReadInt64LittleEndian(value.Slice(startIndex));
            return BitConverter.Int64BitsToDouble(bits);
        }

        public Double Convert(Span<byte> value, int startIndex, int length)
        {
            return Convert(value, startIndex);
        }

        public byte[] Convert(Double value)
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
            return Convert((Double)value);
        }

        public void Convert(Double value, Span<byte> buffer)
        {
            Convert(value, buffer, 0);
        }

        public void Convert(Double value, Span<byte> buffer, int offset)
        {
            if (buffer.Length < offset + 8)
                throw new BufferOverflowException("The buffer can not contain the value");
            var bits = BitConverter.DoubleToInt64Bits(value);
            BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(offset), bits);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((Double)value, buffer, 0);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer, int offset)
        {
            Convert((Double)value, buffer, offset);
        }

        public void Convert(Double value, BinaryWriteBuffer writeBuffer)
        {
            var bytes = Convert(value);
            writeBuffer.Write(bytes);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Double)value, writeBuffer);
        }

    }
}
