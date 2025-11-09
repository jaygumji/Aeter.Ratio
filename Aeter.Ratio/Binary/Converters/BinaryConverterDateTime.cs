/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterDateTime : IBinaryConverter<DateTime>
    {
        public DateTime Convert(Span<byte> value)
        {
            return Convert(value, 0, value.Length);
        }

        public DateTime Convert(Span<byte> value, int startIndex)
        {
            var ticks = BinaryPrimitives.ReadInt64LittleEndian(value.Slice(startIndex));
            return new DateTime(ticks);
        }

        public DateTime Convert(Span<byte> value, int startIndex, int length)
        {
            return Convert(value, startIndex);
        }

        public byte[] Convert(DateTime value)
        {
            return BitConverter.GetBytes(value.Ticks);
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
            return Convert((DateTime)value);
        }

        public void Convert(DateTime value, Span<byte> buffer)
        {
            Convert(value, buffer, 0);
        }

        public void Convert(DateTime value, Span<byte> buffer, int offset)
        {
            if (buffer.Length < offset + 8)
                throw new BufferOverflowException("The buffer can not contain the value");
            BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(offset), value.Ticks);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((DateTime)value, buffer, 0);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer, int offset)
        {
            Convert((DateTime)value, buffer, offset);
        }

        public void Convert(DateTime value, BinaryWriteBuffer writeBuffer)
        {
            var bytes = Convert(value);
            writeBuffer.Write(bytes);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((DateTime)value, writeBuffer);
        }

    }
}
