/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterSingle : IBinaryConverter<Single>
    {
        private const int Size = sizeof(int);

        public Single Convert(ReadOnlySpan<byte> value)
        {
            var bits = BinaryPrimitives.ReadInt32LittleEndian(value);
            return BitConverter.Int32BitsToSingle(bits);
        }

        public byte[] Convert(Single value)
        {
            return BitConverter.GetBytes(value);
        }

        public void Convert(Single value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            var bits = BitConverter.SingleToInt32Bits(value);
            BinaryPrimitives.WriteInt32LittleEndian(buffer, bits);
        }

        public void Convert(Single value, BinaryWriteBuffer writeBuffer)
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
            return Convert((Single)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((Single)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Single)value, writeBuffer);
        }

    }
}
