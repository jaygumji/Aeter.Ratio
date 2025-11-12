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

        public String Convert(ReadOnlySpan<byte> value)
        {
            return Encoding.GetString(value);
        }

        public byte[] Convert(String value)
        {
            return Encoding.GetBytes(value);
        }

        public void Convert(String value, Span<byte> buffer)
        {
            if (string.IsNullOrEmpty(value))
                return;

            var length = Encoding.GetByteCount(value);
            if (buffer.Length < length)
                throw new BufferOverflowException("The buffer can not contain the value");
            Encoding.GetBytes(value.AsSpan(), buffer);
        }

        public void Convert(String value, BinaryWriteBuffer writeBuffer)
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
            return Convert((String)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((String)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((String)value, writeBuffer);
        }

    }
}
