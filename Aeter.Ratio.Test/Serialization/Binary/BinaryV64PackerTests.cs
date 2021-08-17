/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
using Aeter.Ratio.Binary;
using Xunit;


namespace Aeter.Ratio.Test.Serialization.Binary
{
    
    public class BinaryV64PackerTests
    {

        private static void AssertPackU(UInt64? value)
        {
            using (var stream = new MemoryStream()) {
                BinaryV64Packer.PackU(stream, value);
                stream.Seek(0, SeekOrigin.Begin);
                var actual = BinaryV64Packer.UnpackU(stream);
                Assert.Equal(value, actual);
            }
        }

        private static void AssertPackS(Int64? value)
        {
            using (var stream = new MemoryStream()) {
                BinaryV64Packer.PackS(stream, value);
                stream.Seek(0, SeekOrigin.Begin);
                var actual = BinaryV64Packer.UnpackS(stream);
                Assert.Equal(value, actual);
            }
        }

        [Fact]
        public void Pack64NullUTest()
        {
            AssertPackU(null);
        }

        [Fact]
        public void Pack64NullSTest()
        {
            AssertPackS(null);
        }

        [Fact]
        public void Pack64UTest()
        {
            AssertPackU(0x0FC0D096U);
        }

        [Fact]
        public void Pack64STest()
        {
            AssertPackS(0x0FC0D096);
        }

        [Fact]
        public void Pack64HighUTest()
        {
            AssertPackU(UInt64.MaxValue);
        }

        [Fact]
        public void Pack64HighSTest()
        {
            AssertPackS(Int64.MaxValue);
        }

        [Fact]
        public void Pack64LowUTest()
        {
            AssertPackU(UInt64.MinValue);
        }

        [Fact]
        public void Pack64LowSTest()
        {
            AssertPackS(Int64.MinValue);
        }

    }
}