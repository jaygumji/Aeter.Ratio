/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterTimeSpan : IBinaryConverter<TimeSpan>
    {
        private const int Size = sizeof(long);

        public TimeSpan Convert(ReadOnlySpan<byte> value)
        {
            var ticks = BinaryPrimitives.ReadInt64LittleEndian(value);
            return new TimeSpan(ticks);
        }

        public byte[] Convert(TimeSpan value)
        {
            return BitConverter.GetBytes(value.Ticks);
        }

        public void Convert(TimeSpan value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value.Ticks);
        }

        public void Convert(TimeSpan value, BinaryWriteBuffer writeBuffer)
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
            return Convert((TimeSpan)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((TimeSpan)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((TimeSpan)value, writeBuffer);
        }

    }
}
