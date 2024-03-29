﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
namespace Aeter.Ratio.Binary.Converters
{
    public class BinaryConverterUInt32 : IBinaryConverter<UInt32>
    {
        public UInt32 Convert(byte[] value)
        {
            if (value == null) throw new ArgumentNullException("value");
            return Convert(value, 0, value.Length);
        }

        public UInt32 Convert(byte[] value, int startIndex)
        {
            if (value == null) throw new ArgumentNullException("value");
            return BitConverter.ToUInt32(value, startIndex);
        }

        public UInt32 Convert(byte[] value, int startIndex, int length)
        {
            return Convert(value, startIndex);
        }

        public byte[] Convert(UInt32 value)
        {
            return BitConverter.GetBytes(value);
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
            return Convert((UInt32)value);
        }

        public void Convert(UInt32 value, byte[] buffer)
        {
            Convert(value, buffer, 0);
        }

        public void Convert(UInt32 value, byte[] buffer, int offset)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            var bytes = Convert(value);
            if (buffer.Length < offset + bytes.Length)
                throw new BufferOverflowException("The buffer can not contain the value");
            Array.Copy(bytes, 0, buffer, offset, bytes.Length);
        }

        void IBinaryConverter.Convert(object value, byte[] buffer)
        {
            Convert((UInt32)value, buffer, 0);
        }

        void IBinaryConverter.Convert(object value, byte[] buffer, int offset)
        {
            Convert((UInt32)value, buffer, offset);
        }

        public void Convert(UInt32 value, BinaryWriteBuffer writeBuffer)
        {
            var offset = writeBuffer.Advance(4);
            Convert(value, writeBuffer.Buffer, offset);
        }

        void IBinaryConverter.Convert(object value, BinaryWriteBuffer writeBuffer)
        {
            Convert((UInt32)value, writeBuffer);
        }

    }
}
