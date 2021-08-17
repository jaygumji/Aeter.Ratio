/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
using Aeter.Ratio.Binary;
using Xunit;


namespace Aeter.Ratio.Test.Serialization.Binary
{
    
    public class BinaryV32PackerTests
    {
        private static void AssertPackU(UInt32? value)
        {
            using (var stream = new MemoryStream()) {
                BinaryV32Packer.PackU(stream, value);
                stream.Seek(0, SeekOrigin.Begin);
                var actual = BinaryV32Packer.UnpackU(stream);
                Assert.Equal(value, actual);
            }
        }

        private static void AssertPackS(Int32? value)
        {
            using (var stream = new MemoryStream()) {
                BinaryV32Packer.PackS(stream, value);
                stream.Seek(0, SeekOrigin.Begin);
                var actual = BinaryV32Packer.UnpackS(stream);
                Assert.Equal(value, actual);
            }
        }

        [Fact]
        public void Pack32NullUTest()
        {
            AssertPackU(null);
        }

        [Fact]
        public void Pack32NullSTest()
        {
            AssertPackS(null);
        }

        [Fact]
        public void Pack32UTest()
        {
            AssertPackU(0x0FC0D096U);
        }

        [Fact]
        public void Pack32STest()
        {
            AssertPackS(0x0FC0D096);
        }

        [Fact]
        public void Pack32HighUTest()
        {
            AssertPackU(UInt32.MaxValue);
        }

        [Fact]
        public void Pack32HighSTest()
        {
            AssertPackS(Int32.MaxValue);
        }

        [Fact]
        public void Pack32LowUTest()
        {
            AssertPackU(UInt32.MinValue);
        }

        [Fact]
        public void Pack32LowSTest()
        {
            AssertPackS(Int32.MinValue);
        }

    }
}