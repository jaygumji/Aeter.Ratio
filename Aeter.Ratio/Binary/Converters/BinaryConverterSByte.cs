/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterSByte : IBinaryConverter<SByte>
    {
        public SByte Convert(byte[] value)
        {
            if (value == null) throw new ArgumentNullException("value");
            return Convert(value, 0, value.Length);
        }

        public SByte Convert(byte[] value, int startIndex)
        {
            if (value == null) throw new ArgumentNullException("value");
            return (SByte) value[startIndex];
        }

        public SByte Convert(byte[] value, int startIndex, int length)
        {
            return Convert(value, startIndex);
        }

        public byte[] Convert(SByte value)
        {
            return new byte[] { (byte)value };
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
            return Convert((SByte)value);
        }

        public void Convert(SByte value, byte[] buffer)
        {
            Convert(value, buffer, 0);
        }

        public void Convert(SByte value, byte[] buffer, int offset)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            var b = (byte) value;
            buffer[offset] = b;
        }

        void IBinaryConverter.Convert(object value, byte[] buffer)
        {
            Convert((SByte)value, buffer, 0);
        }

        void IBinaryConverter.Convert(object value, byte[] buffer, int offset)
        {
            Convert((SByte)value, buffer, offset);
        }

        public void Convert(SByte value, BinaryWriteBuffer writeBuffer)
        {
            var offset = writeBuffer.Advance(1);
            Convert(value, writeBuffer.Buffer, offset);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((SByte)value, writeBuffer);
        }

    }
}
