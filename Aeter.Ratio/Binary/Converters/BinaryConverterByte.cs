/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterByte : IBinaryConverter<Byte>
    {
        private const int Size = sizeof(Byte);

        public Byte Convert(ReadOnlySpan<byte> value)
        {
            if (value.Length < Size)
                throw new ArgumentException("The span does not contain enough data.", nameof(value));
            return value[0];
        }

        public byte[] Convert(Byte value)
        {
            return new[] { value };
        }

        public void Convert(Byte value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            buffer[0] = value;
        }

        public void Convert(Byte value, BinaryWriteBuffer writeBuffer)
        {
            writeBuffer.WriteByte(value);
        }

        object IBinaryConverter.Convert(ReadOnlySpan<byte> value)
        {
            return Convert(value);
        }

        byte[] IBinaryConverter.Convert(object value)
        {
            return Convert((Byte)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((Byte)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Byte)value, writeBuffer);
        }

    }
}
