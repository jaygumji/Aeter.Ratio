/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterByte : IBinaryConverter<Byte>
    {
        public Byte Convert(byte[] value)
        {
            if (value == null) throw new ArgumentNullException("value");
            return Convert(value, 0, value.Length);
        }

        public Byte Convert(byte[] value, int startIndex)
        {
            if (value == null) throw new ArgumentNullException("value");
            return value[startIndex];
        }

        public Byte Convert(byte[] value, int startIndex, int length)
        {
            return Convert(value, startIndex);
        }

        public byte[] Convert(Byte value)
        {
            return new byte[] { value };
        }

        object IBinaryConverter.Convert(byte[] value)
        {
            if (value == null) throw new ArgumentNullException("value");
            return Convert(value, 0, value.Length);
        }

        object IBinaryConverter.Convert(byte[] value, int startIndex)
        {
            if (value == null) throw new ArgumentNullException("value");
            return Convert(value, startIndex, value.Length - startIndex);
        }

        object IBinaryConverter.Convert(byte[] value, int startIndex, int length)
        {
            return Convert(value, startIndex, length);
        }

        byte[] IBinaryConverter.Convert(object value)
        {
            return Convert((byte)value);
        }

        public void Convert(Byte value, byte[] buffer)
        {
            Convert(value, buffer, 0);
        }

        public void Convert(Byte value, byte[] buffer, int offset)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.Length < offset + 1)
                throw new BufferOverflowException("The buffer can not contain the value");
            buffer[offset] = value;
        }

        void IBinaryConverter.Convert(object value, byte[] buffer)
        {
            Convert((Byte)value, buffer, 0);
        }

        void IBinaryConverter.Convert(object value, byte[] buffer, int offset)
        {
            Convert((Byte)value, buffer, offset);
        }

        public void Convert(Byte value, BinaryWriteBuffer writeBuffer)
        {
            var offset = writeBuffer.Advance(1);
            Convert(value, writeBuffer.Buffer, offset);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Byte)value, writeBuffer);
        }

    }
}
