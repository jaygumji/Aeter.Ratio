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

        public String Convert(Span<byte> value)
        {
            return Convert(value, 0, value.Length);
        }

        public String Convert(Span<byte> value, int startIndex)
        {
            return Convert(value, startIndex, value.Length - startIndex);
        }

        public String Convert(Span<byte> value, int startIndex, int length)
        {
            return Encoding.GetString(value.Slice(startIndex, length));
        }

        public byte[] Convert(String value)
        {
            return Encoding.GetBytes(value);
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
            return Convert((String)value);
        }

        public void Convert(String value, Span<byte> buffer)
        {
            Convert(value, buffer, 0);
        }

        public void Convert(String value, Span<byte> buffer, int offset)
        {
            if (value == null || value.Length == 0) return;
            var length = Encoding.GetByteCount(value);
            if (buffer.Length < offset + length)
                throw new BufferOverflowException("The buffer can not contain the value");
            Encoding.GetBytes(value.AsSpan(), buffer.Slice(offset));
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((String)value, buffer, 0);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer, int offset)
        {
            Convert((String)value, buffer, offset);
        }

        public void Convert(String value, BinaryWriteBuffer writeBuffer)
        {
            var bytes = Convert(value);
            writeBuffer.Write(bytes);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((String)value, writeBuffer);
        }

    }
}
