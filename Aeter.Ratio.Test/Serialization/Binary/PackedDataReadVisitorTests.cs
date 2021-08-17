/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Serialization;
using Aeter.Ratio.Serialization.PackedBinary;
using Aeter.Ratio.Test.Fakes.Entities;
using Aeter.Ratio.Test.Serialization.HardCoded;
using System.IO;
using Xunit;


namespace Aeter.Ratio.Test.Serialization.Binary
{


    public class PackedDataReadVisitorTests
    {

        [Fact]
        public void ReadHardCodedTravelTest()
        {
            var bytes = BinarySerializationTestContext.GetFilledDataBlockBlob();
            var stream = new MemoryStream(bytes);
            var visitor = new PackedDataReadVisitor(stream);

            var traveller = DataBlockHardCodedTraveller.Create();

            var graph = new DataBlock();
            traveller.Travel(visitor, graph);

            var expected = DataBlock.Filled();
            graph.AssertEqualTo(expected);
        }

        [Fact]
        public void ReadDynamicTravelTest()
        {
            var bytes = BinarySerializationTestContext.GetFilledDataBlockBlob();
            var stream = new MemoryStream(bytes);
            var visitor = new PackedDataReadVisitor(stream);

            var traveller = BinarySerializationTestContext.Provider.Get<DataBlock>();

            var graph = new DataBlock();
            traveller.Travel(visitor, graph);

            var expected = DataBlock.Filled();
            graph.AssertEqualTo(expected);
        }

    }
}
