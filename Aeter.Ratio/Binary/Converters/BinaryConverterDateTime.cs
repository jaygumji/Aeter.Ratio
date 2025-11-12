/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterDateTime : IBinaryConverter<DateTime>
    {
        private const int Size = sizeof(long);

        public DateTime Convert(ReadOnlySpan<byte> value)
        {
            var ticks = BinaryPrimitives.ReadInt64LittleEndian(value);
            return new DateTime(ticks);
        }

        public byte[] Convert(DateTime value)
        {
            return BitConverter.GetBytes(value.Ticks);
        }

        public void Convert(DateTime value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value.Ticks);
        }

        public void Convert(DateTime value, BinaryWriteBuffer writeBuffer)
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
            return Convert((DateTime)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((DateTime)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((DateTime)value, writeBuffer);
        }

    }
}
