/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Aeter.Ratio.Binary;

namespace Aeter.Ratio.IO
{
    /// <summary>
    /// Writes strongly typed values to a <see cref="BinaryWriteBuffer"/> with optional variable-length encodings.
    /// </summary>
    public class BinaryDataWriter : IDataWriter
    {
        private readonly BinaryWriteBuffer _writeBuffer;
        private readonly Encoding _encoding;

        /// <summary>
        /// Creates a writer that targets the provided stream using the specified encoding.
        /// </summary>
        /// <param name="stream">Destination stream.</param>
        /// <param name="encoding">Text encoding used for string serialization.</param>
        public BinaryDataWriter(Stream stream, Encoding encoding)
            : this(new BinaryWriteBuffer(8024, BinaryStream.Passthrough(stream)), encoding)
        {
        }

        /// <summary>
        /// Creates a writer that targets the provided stream using UTF-8 encoding.
        /// </summary>
        /// <param name="stream">Destination stream.</param>
        public BinaryDataWriter(Stream stream)
            : this(new BinaryWriteBuffer(8024, BinaryStream.Passthrough(stream)), Encoding.UTF8)
        {
        }

        /// <summary>
        /// Creates a writer that reuses an existing <see cref="BinaryWriteBuffer"/> and UTF-8 encoding.
        /// </summary>
        /// <param name="writeBuffer">Underlying write buffer.</param>
        public BinaryDataWriter(BinaryWriteBuffer writeBuffer)
            : this(writeBuffer, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Creates a writer that reuses an existing <see cref="BinaryWriteBuffer"/> with a custom encoding.
        /// </summary>
        /// <param name="writeBuffer">Underlying write buffer.</param>
        /// <param name="encoding">Text encoding used for string serialization.</param>
        public BinaryDataWriter(BinaryWriteBuffer writeBuffer, Encoding encoding)
        {
            _writeBuffer = writeBuffer;
            _encoding = encoding;
        }

        /// <summary>
        /// Reserves four bytes for a forthcoming length prefix and returns a handle that can be completed later.
        /// </summary>
        public BinaryBufferReservation Reserve()
        {
            return _writeBuffer.Reserve(4);
        }

        /// <summary>
        /// Applies the final size to the supplied reservation by writing it into the reserved space.
        /// </summary>
        /// <param name="reservation">Reservation previously created by <see cref="Reserve"/>.</param>
        public void Write(BinaryBufferReservation reservation)
        {
            _writeBuffer.ApplyUInt32Size(reservation);
        }

        public void Write(BinaryBufferReservation reservation, UInt32 value)
        {
            var valueBytes = BinaryInformation.UInt32.Converter.Convert(reservation);
            _writeBuffer.Use(reservation, valueBytes);
        }

        /// <summary>
        /// Writes a zig-zag encoded unsigned integer that favors small magnitudes.
        /// </summary>
        /// <param name="value">The value to pack.</param>
        public void WriteZ(UInt32 value)
        {
            BinaryZPacker.Pack(_writeBuffer, value);
        }

        /// <summary>
        /// Writes a variable-length unsigned integer using between one and five bytes.
        /// </summary>
        /// <param name="value">The value to pack.</param>
        public void WriteV(UInt32 value)
        {
            BinaryV32Packer.PackU(_writeBuffer, value);
        }

        /// <summary>
        /// Writes an optional variable-length unsigned integer using between one and five bytes.
        /// </summary>
        /// <param name="nullableValue">The value to pack.</param>
        public void WriteNV(UInt32? nullableValue)
        {
            BinaryV32Packer.PackU(_writeBuffer, nullableValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write<T>(IBinaryInformation<T> info, T value)
        {
            var bytes = info.Converter.Convert(value);
            _writeBuffer.Write(bytes);
        }

        /// <inheritdoc />
        public void Write(byte value)
        {
            _writeBuffer.WriteByte(value);
        }

        /// <inheritdoc />
        public void Write(short value)
        {
            Write(BinaryInformation.Int16, value);
        }

        /// <inheritdoc />
        public void Write(int value)
        {
            Write(BinaryInformation.Int32, value);
        }

        /// <inheritdoc />
        public void Write(long value)
        {
            Write(BinaryInformation.Int64, value);
        }

        /// <inheritdoc />
        public void Write(ushort value)
        {
            Write(BinaryInformation.UInt16, value);
        }

        /// <inheritdoc />
        public void Write(uint value)
        {
            Write(BinaryInformation.UInt32, value);
        }

        /// <inheritdoc />
        public void Write(ulong value)
        {
            Write(BinaryInformation.UInt64, value);
        }

        /// <inheritdoc />
        public void Write(bool value)
        {
            Write(BinaryInformation.Boolean, value);
        }

        /// <inheritdoc />
        public void Write(float value)
        {
            Write(BinaryInformation.Single, value);
        }

        /// <inheritdoc />
        public void Write(double value)
        {
            Write(BinaryInformation.Double, value);
        }

        /// <inheritdoc />
        public void Write(decimal value)
        {
            Write(BinaryInformation.Decimal, value);
        }

        /// <inheritdoc />
        public void Write(TimeSpan value)
        {
            Write(BinaryInformation.TimeSpan, value);
        }

        /// <inheritdoc />
        public void Write(DateTime value)
        {
            Write(BinaryInformation.DateTime, value);
        }

        /// <inheritdoc />
        public void Write(string value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var bytes = _encoding.GetBytes(value);
            _writeBuffer.Write(bytes);
        }

        /// <inheritdoc />
        public void Write(Guid value)
        {
            Write(BinaryInformation.Guid, value);
        }

        /// <inheritdoc />
        public void Write(byte[] value)
        {
            ArgumentNullException.ThrowIfNull(value);

            _writeBuffer.Write(value);
        }

    }
}
