/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterDecimal : IBinaryConverter<Decimal>
    {
        private const int Size = 16;

        private static Decimal ToDecimal(ReadOnlySpan<byte> buffer, int start = 0)
        {
            var lo = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(start, 4));
            var mid = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(start + 4, 4));
            var hi = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(start + 8, 4));
            var flags = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(start + 12, 4));
            return new Decimal(new[] { lo, mid, hi, flags });
        }

        private static void WriteBytes(Decimal value, Span<byte> buffer, int offset = 0)
        {
            var bits = Decimal.GetBits(value);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset, 4), bits[0]);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset + 4, 4), bits[1]);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset + 8, 4), bits[2]);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset + 12, 4), bits[3]);
        }

        public Decimal Convert(ReadOnlySpan<byte> value)
        {
            if (value.Length < Size)
                throw new ArgumentException("The span does not contain enough data.", nameof(value));
            return ToDecimal(value);
        }

        public byte[] Convert(Decimal value)
        {
            var result = new byte[Size];
            WriteBytes(value, result);
            return result;
        }

        public void Convert(Decimal value, Span<byte> buffer)
        {
            if (buffer.Length < Size)
                throw new BufferOverflowException("The buffer can not contain the value");
            WriteBytes(value, buffer);
        }

        public void Convert(Decimal value, BinaryWriteBuffer writeBuffer)
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
            return Convert((Decimal)value);
        }

        void IBinaryConverter.Convert(object value, Span<byte> buffer)
        {
            Convert((Decimal)value, buffer);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((Decimal)value, writeBuffer);
        }

    }
}
