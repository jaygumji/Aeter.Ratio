/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Xunit;


namespace Aeter.Ratio.Test.Serialization.Binary
{
    
    public class BinaryZPackerTests
    {
        [Fact]
        public void PackZTest()
        {
            const uint value = (uint) 0x0FC0D096;
            var buffer = BinaryZPacker.Pack(value);

            Assert.True((3 & buffer[0]) == 3);
            Assert.True(((buffer[0] & 0xFC) & 0x58) == 0x58);

            var unpacked = BinaryZPacker.Unpack(buffer, 0);
            Assert.Equal(value, unpacked);
        }

    }
}
