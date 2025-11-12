/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterDouble : IBinaryConverter<Double>
    {
        private const int Size = sizeof(long);

        public Double Convert(ReadOnlySpan<byte> value)
        {
            var bits = BinaryPrimitives.ReadInt64LittleEndian(value);
            return BitConverter.Int64BitsToDouble(bits);
        }

        public byte[] Convert(Double value)
        {
            return BitConverter.GetBytes(value);
        }

        public void Convert(Double value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            var bits = BitConverter.DoubleToInt64Bits(value);
            BinaryPrimitives.WriteInt64LittleEndian(buffer, bits);
        }

        public void Convert(Double value, BinaryWriteBuffer writeBuffer)
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
            return Convert((Double)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((Double)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Double)value, writeBuffer);
        }

    }
}
