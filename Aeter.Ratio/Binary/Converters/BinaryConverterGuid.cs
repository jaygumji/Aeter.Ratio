/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterGuid : IBinaryConverter<Guid>
    {
        public Guid Convert(Span<byte> value)
        {
            return Convert(value, 0, value.Length);
        }

        public Guid Convert(Span<byte> value, int startIndex)
        {
            return Convert(value, startIndex, value.Length - startIndex);
        }

        public Guid Convert(Span<byte> value, int startIndex, int length)
        {
            return new Guid(value.Slice(startIndex, length));
        }

        public byte[] Convert(Guid value)
        {
            return value.ToByteArray();
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
            return Convert((Guid)value);
        }

        public void Convert(Guid value, Span<byte> buffer)
        {
            Convert(value, buffer, 0);
        }

        public void Convert(Guid value, Span<byte> buffer, int offset)
        {
            if (buffer.Length < offset + 16)
                throw new BufferOverflowException("The buffer can not contain the value");
            if (!value.TryWriteBytes(buffer.Slice(offset)))
                throw new BufferOverflowException("The buffer can not contain the value");
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((Guid)value, buffer, 0);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer, int offset)
        {
            Convert((Guid)value, buffer, offset);
        }

        public void Convert(Guid value, BinaryWriteBuffer writeBuffer)
        {
            var bytes = Convert(value);
            writeBuffer.Write(bytes);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Guid)value, writeBuffer);
        }

    }
}
