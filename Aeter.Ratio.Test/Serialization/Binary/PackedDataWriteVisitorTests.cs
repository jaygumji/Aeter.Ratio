﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Linq;
using Aeter.Ratio.Test.Fakes.Entities;
using Aeter.Ratio.Testing.Fakes.Entities;
using Xunit;


namespace Aeter.Ratio.Test.Serialization.Binary
{
    
    public class PackedDataWriteVisitorTests
    {

        private readonly BinarySerializationTestContext _context = new BinarySerializationTestContext();

        //[Fact]
        //public void WriteHardCodedTravelTest()
        //{
        //    var hex = GetHardCodedHexString();
        //    StringAssert.StartsWith(hex, "0x410429115F5A3B9FA4585AEE0C9E6990A98FFB00048656C6C6F20576F726C64C2A4F1045E9798214871B2C09873ED5D3C18242FB1C4F6D7E9CD2083E1C7D332AD6D3882446666042288B81E85EB515B1402C80448E258000301094E5AA20000000000303480504347A81BD183812A3C1140FF300012344FF37000FF50005465737431FF50005465737432FF50005465737433FF50005465737434FF50005465737435048FFE000803025ACA187CC804CFF57000410D4F78EF6626F6B47BC5E71AD86549A638FFA000436F6E6E656374696F6ECFF2400047656E6572696320636F6E6E656374696F6E206265747765656E2072656C6174696F6E731044D0000");
        //}

        [Fact]
        public void WriteDynamicTravelTest()
        {
            var bytes = _context.Pack(DataBlock.Filled());
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
            var hex = "0x" + string.Join("", bytes.Select(b => b.ToString("X")));
            Assert.NotNull(hex);
            var expected = BinarySerializationTestContext.GetFilledDataBlockHexString();
            Assert.Equal(expected, hex);
        }

        //[Fact]
        //public void ProtoBufTest()
        //{
        //    var converter = new ProtocolBuffer.ProtocolBufferBinaryConverter<DataBlock>();
        //    var bytes = converter.ConvertTo(DataBlock.Filled());
        //    Assert.NotNull(bytes);
        //    Assert.True(bytes.Length > 0);
        //    var hex = "0x" + string.Join("", bytes.Select(b => b.ToString("X")));
        //    Assert.NotNull(hex);
        //}

    }
}
