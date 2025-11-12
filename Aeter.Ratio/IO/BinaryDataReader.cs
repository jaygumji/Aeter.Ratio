/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Runtime.CompilerServices;
using Aeter.Ratio.Binary;

namespace Aeter.Ratio.IO
{
    public class BinaryDataReader(BinaryReadBuffer buffer) : IDataReader
    {
        public UInt32 ReadZ() => BinaryZPacker.Unpack(buffer);
        public UInt32 ReadV() => BinaryV32Packer.UnpackU(buffer) ?? 0;
        public UInt32? ReadNV() => BinaryV32Packer.UnpackU(buffer);
        public void Skip(uint length) => buffer.Advance((int)length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T Read<T>(IBinaryInformation<T> info) => info.Converter.Convert(buffer.Read(info.FixedLength));

        public byte ReadByte() => (Byte) buffer.ReadByte();

        public short ReadInt16() => Read(BinaryInformation.Int16);
        public int ReadInt32() => Read(BinaryInformation.Int32);
        public long ReadInt64() => Read(BinaryInformation.Int64);
        public ushort ReadUInt16() => Read(BinaryInformation.UInt16);
        public uint ReadUInt32() => Read(BinaryInformation.UInt32);
        public ulong ReadUInt64() => Read(BinaryInformation.UInt64);
        public bool ReadBoolean() => Read(BinaryInformation.Boolean);
        public float ReadSingle() => Read(BinaryInformation.Single);
        public double ReadDouble() => Read(BinaryInformation.Double);
        public decimal ReadDecimal() => Read(BinaryInformation.Decimal);
        public TimeSpan ReadTimeSpan() => Read(BinaryInformation.TimeSpan);
        public DateTime ReadDateTime() => Read(BinaryInformation.DateTime);
        public string ReadString()
        {
            var length = Read(BinaryInformation.UInt32);
            return ReadString(length);
        }
        public string ReadString(uint length)
        {
            return BinaryInformation.String.Converter.Convert(buffer.Read((int)length));
        }
        public Guid ReadGuid() => Read(BinaryInformation.Guid);
        public byte[] ReadBlob()
        {
            var length = Read(BinaryInformation.UInt32);
            return ReadBlob(length);
        }
        public byte[] ReadBlob(uint length) => buffer.Read((int)length).ToArray();
    }
}