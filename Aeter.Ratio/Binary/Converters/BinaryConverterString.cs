/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Text;

namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterString : IBinaryConverter<String>
    {

        private static readonly Encoding Encoding = Encoding.UTF8;

        public String Convert(byte[] value)
        {
            if (value == null) throw new ArgumentNullException("value");
            return Convert(value, 0, value.Length);
        }

        public String Convert(byte[] value, int startIndex)
        {
            if (value == null) throw new ArgumentNullException("value");
            return Convert(value, startIndex, value.Length - startIndex);
        }

        public String Convert(byte[] value, int startIndex, int length)
        {
            if (value == null) throw new ArgumentNullException("value");
            return Encoding.GetString(value, startIndex, length);
        }

        public byte[] Convert(String value)
        {
            return Encoding.GetBytes(value);
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
            return Convert((String)value);
        }

        public void Convert(String value, byte[] buffer)
        {
            Convert(value, buffer, 0);
        }

        public void Convert(String value, byte[] buffer, int offset)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (value == null || value.Length == 0) return;
            Encoding.GetBytes(value, 0, value.Length, buffer, offset);
        }

        void IBinaryConverter.Convert(object value, byte[] buffer)
        {
            Convert((String)value, buffer, 0);
        }

        void IBinaryConverter.Convert(object value, byte[] buffer, int offset)
        {
            Convert((String)value, buffer, offset);
        }

        public void Convert(String value, BinaryWriteBuffer writeBuffer)
        {
            var length = Encoding.GetByteCount(value);
            var offset = writeBuffer.Advance(length);
            Convert(value, writeBuffer.Buffer, offset);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((String)value, writeBuffer);
        }

    }
}
