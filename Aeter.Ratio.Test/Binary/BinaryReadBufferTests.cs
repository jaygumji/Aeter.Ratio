/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Aeter.Ratio.Test.Binary
{
    public class BinaryReadBufferTests
    {
        [Fact]
        public void ReadByte_RefillsSynchronously()
        {
            var payload = Enumerable.Range(0, 5).Select(i => (byte)i).ToArray();
            using var stream = new MemoryStream(payload);
            using var buffer = new BinaryReadBuffer(2, BinaryStream.MemoryStream(stream));

            var observed = new List<byte>();
            for (var i = 0; i < payload.Length; i++) {
                observed.Add(buffer.ReadByte());
            }

            Assert.Equal(payload, observed);
        }

        [Fact]
        public void CopyTo_ExpandsAndAdvancesSynchronously()
        {
            var payload = Enumerable.Range(0, 10).Select(i => (byte)i).ToArray();
            using var stream = new MemoryStream(payload);
            using var buffer = new BinaryReadBuffer(3, BinaryStream.MemoryStream(stream));
            var destination = new byte[6];

            buffer.CopyTo(destination, 0, destination.Length);

            Assert.Equal(payload.Take(destination.Length), destination);
            Assert.Equal(destination.Length, buffer.TotalConsumed);
        }

        [Fact]
        public async Task SkipToAsync_MovesBufferToRequestedOffset()
        {
            var payload = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray();
            using var stream = new MemoryStream(payload);
            using var buffer = new BinaryReadBuffer(8, BinaryStream.MemoryStream(stream));

            await buffer.SkipToAsync(25);
            var value = buffer.ReadByte();

            Assert.Equal(payload[25], value);
        }

        [Fact]
        public async Task SkipToAsync_ThrowsWhenAttemptingToMoveBackwards()
        {
            var payload = Enumerable.Range(0, 20).Select(i => (byte)i).ToArray();
            using var stream = new MemoryStream(payload);
            using var buffer = new BinaryReadBuffer(8, BinaryStream.MemoryStream(stream));

            await buffer.SkipAsync(10);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => buffer.SkipToAsync(5));
        }
    }
}
