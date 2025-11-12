/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterGuid : IBinaryConverter<Guid>
    {
        private const int Size = 16;

        public Guid Convert(ReadOnlySpan<byte> value)
        {
            return new Guid(value);
        }

        public byte[] Convert(Guid value)
        {
            return value.ToByteArray();
        }

        public void Convert(Guid value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            if (!value.TryWriteBytes(buffer))
                throw new BufferOverflowException("The buffer can not contain the value");
        }

        public void Convert(Guid value, BinaryWriteBuffer writeBuffer)
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
            return Convert((Guid)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((Guid)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Guid)value, writeBuffer);
        }

    }
}
