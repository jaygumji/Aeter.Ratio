/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterSByte : IBinaryConverter<SByte>
    {
        private const int Size = sizeof(SByte);

        public SByte Convert(ReadOnlySpan<byte> value)
        {
            if (value.Length < Size)
                throw new ArgumentException("The span does not contain enough data.", nameof(value));
            unchecked
            {
                return (sbyte)value[0];
            }
        }

        public byte[] Convert(SByte value)
        {
            return new[] { unchecked((byte)value) };
        }

        public void Convert(SByte value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            unchecked
            {
                buffer[0] = (byte)value;
            }
        }

        public void Convert(SByte value, BinaryWriteBuffer writeBuffer)
        {
            writeBuffer.WriteByte(unchecked((byte)value));
        }

        object IBinaryConverter.Convert(ReadOnlySpan<byte> value)
        {
            return Convert(value);
        }

        byte[] IBinaryConverter.Convert(object value)
        {
            return Convert((SByte)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((SByte)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((SByte)value, writeBuffer);
        }

    }
}
