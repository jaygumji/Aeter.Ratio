/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Linq;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterGuid : IBinaryConverter<Guid>
    {
        public Guid Convert(byte[] value)
        {
            if (value == null) throw new ArgumentNullException("value");
            return Convert(value, 0, value.Length);
        }

        public Guid Convert(byte[] value, int startIndex)
        {
            if (value == null) throw new ArgumentNullException("value");
            return Convert(value, startIndex, value.Length - startIndex);
        }

        public Guid Convert(byte[] value, int startIndex, int length)
        {
            if (startIndex > 0)
                return new Guid(value.Skip(startIndex).Take(length).ToArray());

            return new Guid(value.Take(length).ToArray());
        }

        public byte[] Convert(Guid value)
        {
            return value.ToByteArray();
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
            return Convert((Guid)value);
        }

        public void Convert(Guid value, byte[] buffer)
        {
            Convert(value, buffer, 0);
        }

        public void Convert(Guid value, byte[] buffer, int offset)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            var bytes = Convert(value);
            if (buffer.Length < offset + bytes.Length)
                throw new BufferOverflowException("The buffer can not contain the value");

            Array.Copy(bytes, 0, buffer, offset, bytes.Length);
        }

        void IBinaryConverter.Convert(object value, byte[] buffer)
        {
            Convert((Guid)value, buffer, 0);
        }

        void IBinaryConverter.Convert(object value, byte[] buffer, int offset)
        {
            Convert((Guid)value, buffer, offset);
        }

        public void Convert(Guid value, BinaryWriteBuffer writeBuffer)
        {
            var offset = writeBuffer.Advance(16);
            Convert(value, writeBuffer.Buffer, offset);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Guid)value, writeBuffer);
        }

    }
}
