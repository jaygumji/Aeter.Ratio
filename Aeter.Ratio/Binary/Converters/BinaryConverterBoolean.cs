/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterBoolean : IBinaryConverter<Boolean>
    {
        private const int Size = sizeof(byte);

        public Boolean Convert(ReadOnlySpan<byte> value)
        {
            if (value.Length < Size)
                throw new ArgumentException("The span does not contain enough data.", nameof(value));
            return value[0] != 0;
        }

        public byte[] Convert(Boolean value)
        {
            return new[] { value ? (byte)1 : (byte)0 };
        }

        public void Convert(Boolean value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            buffer[0] = value ? (byte)1 : (byte)0;
        }

        public void Convert(Boolean value, BinaryWriteBuffer writeBuffer)
        {
            writeBuffer.WriteByte(value ? (byte)1 : (byte)0);
        }

        object IBinaryConverter.Convert(ReadOnlySpan<byte> value)
        {
            return Convert(value);
        }

        byte[] IBinaryConverter.Convert(object value)
        {
            return Convert((Boolean)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((Boolean)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Boolean)value, writeBuffer);
        }
    }
}
