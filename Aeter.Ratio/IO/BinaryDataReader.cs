/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Runtime.CompilerServices;
using Aeter.Ratio.Binary;

namespace Aeter.Ratio.IO
{
    /// <summary>
    /// Reads strongly typed values from a <see cref="BinaryReadBuffer"/>.
    /// </summary>
    /// <param name="buffer">Backing buffer that supplies the raw bytes.</param>
    public class BinaryDataReader(BinaryReadBuffer buffer) : IDataReader
    {
        /// <summary>
        /// Reads a zig-zag encoded unsigned 32-bit integer.
        /// </summary>
        public UInt32 ReadZ() => BinaryZPacker.Unpack(buffer);

        /// <summary>
        /// Reads a variable-length unsigned 32-bit integer, returning zero when no value is present.
        /// </summary>
        public UInt32 ReadV() => BinaryV32Packer.UnpackU(buffer) ?? 0;

        /// <summary>
        /// Reads a nullable variable-length unsigned 32-bit integer.
        /// </summary>
        public UInt32? ReadNV() => BinaryV32Packer.UnpackU(buffer);

        /// <summary>
        /// Advances the buffer by the specified number of bytes without reading them.
        /// </summary>
        /// <param name="length">Number of bytes to skip.</param>
        public void Skip(uint length) => buffer.Advance((int)length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T Read<T>(IBinaryInformation<T> info) => info.Converter.Convert(buffer.Read(info.FixedLength));

        /// <inheritdoc />
        public byte ReadByte() => (Byte) buffer.ReadByte();

        /// <inheritdoc />
        public short ReadInt16() => Read(BinaryInformation.Int16);
        /// <inheritdoc />
        public int ReadInt32() => Read(BinaryInformation.Int32);
        /// <inheritdoc />
        public long ReadInt64() => Read(BinaryInformation.Int64);
        /// <inheritdoc />
        public ushort ReadUInt16() => Read(BinaryInformation.UInt16);
        /// <inheritdoc />
        public uint ReadUInt32() => Read(BinaryInformation.UInt32);
        /// <inheritdoc />
        public ulong ReadUInt64() => Read(BinaryInformation.UInt64);
        /// <inheritdoc />
        public bool ReadBoolean() => Read(BinaryInformation.Boolean);
        /// <inheritdoc />
        public float ReadSingle() => Read(BinaryInformation.Single);
        /// <inheritdoc />
        public double ReadDouble() => Read(BinaryInformation.Double);
        /// <inheritdoc />
        public decimal ReadDecimal() => Read(BinaryInformation.Decimal);
        /// <inheritdoc />
        public TimeSpan ReadTimeSpan() => Read(BinaryInformation.TimeSpan);
        /// <inheritdoc />
        public DateTime ReadDateTime() => Read(BinaryInformation.DateTime);
        /// <inheritdoc />
        public string ReadString()
        {
            var length = Read(BinaryInformation.UInt32);
            return ReadString(length);
        }
        /// <inheritdoc />
        public string ReadString(uint length)
        {
            return BinaryInformation.String.Converter.Convert(buffer.Read((int)length));
        }
        /// <inheritdoc />
        public Guid ReadGuid() => Read(BinaryInformation.Guid);
        /// <inheritdoc />
        public byte[] ReadBlob()
        {
            var length = Read(BinaryInformation.UInt32);
            return ReadBlob(length);
        }
        /// <inheritdoc />
        public byte[] ReadBlob(uint length) => buffer.Read((int)length).ToArray();
    }
}
