/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.IO;
using System;
using System.IO;
using Xunit;

namespace Aeter.Ratio.Test.Binary
{
    public class BinaryWriteBufferTests
    {
        [Fact]
        public void WriteAndFlush_UsesSynchronousStream()
        {
            using var stream = new MemoryStream();
            using var buffer = new BinaryWriteBuffer(4, BinaryStream.MemoryStream(stream));

            buffer.Write(new byte[] { 1, 2, 3, 4 });
            buffer.Flush();

            Assert.Equal(new byte[] { 1, 2, 3, 4 }, stream.ToArray());
        }

        [Fact]
        public void ReserveAndUse_WritesDataBeforeFlush()
        {
            using var stream = new MemoryStream();
            using var buffer = new BinaryWriteBuffer(8, BinaryStream.MemoryStream(stream));

            var reservation = buffer.Reserve(4);
            buffer.Use(reservation, new byte[] { 9, 8, 7, 6 }, 0, 4);
            buffer.Write(new byte[] { 5, 4 });
            buffer.Flush();

            Assert.Equal(new byte[] { 9, 8, 7, 6, 5, 4 }, stream.ToArray());
        }

        [Fact]
        public void Use_RespectsProvidedLength()
        {
            using var stream = new MemoryStream();
            using var buffer = new BinaryWriteBuffer(8, BinaryStream.MemoryStream(stream));

            var reservation = buffer.Reserve(4);
            buffer.Use(reservation, new byte[] { 1, 2, 3, 4 }, 0, 2);
            buffer.Flush();

            Assert.Equal(new byte[] { 1, 2, 0, 0 }, stream.ToArray());
        }

        [Fact]
        public void Use_ThrowsWhenLengthExceedsReservation()
        {
            using var stream = new MemoryStream();
            using var buffer = new BinaryWriteBuffer(4, BinaryStream.MemoryStream(stream));

            var reservation = buffer.Reserve(2);

            Assert.Throws<ArgumentException>(() => buffer.Use(reservation, new byte[] { 1, 2, 3 }, 0, 3));
        }

        [Fact]
        public void WriteByte_FlushesWhenBufferIsFull()
        {
            using var stream = new MemoryStream();
            using var buffer = new BinaryWriteBuffer(2, BinaryStream.MemoryStream(stream));

            buffer.Write(new byte[] { 9, 8 });
            buffer.WriteByte(7);
            buffer.Flush();

            Assert.Equal(new byte[] { 9, 8, 7 }, stream.ToArray());
        }
    }
}
