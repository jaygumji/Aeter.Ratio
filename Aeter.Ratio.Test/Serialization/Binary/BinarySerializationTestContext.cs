/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.IoC;
using Aeter.Ratio.Reflection;
using Aeter.Ratio.Serialization;
using Aeter.Ratio.Serialization.PackedBinary;
using Aeter.Ratio.Serialization.Reflection;
using Aeter.Ratio.Test.Fakes.Entities;
using Aeter.Ratio.Test.Serialization.HardCoded;
using System.IO;
using System.Linq;
using Xunit;

namespace Aeter.Ratio.Test.Serialization.Binary
{
    public class BinarySerializationTestContext : SerializationTestContext
    {
        public static readonly GraphTravellerProvider Provider = new GraphTravellerProvider(
            new DynamicGraphTravellerFactory(new IoCContainer(), new SerializableTypeProvider(new SerializationReflectionInspector(), FactoryTypeProvider.Instance)));

        public BinarySerializationTestContext()
        {
        }

        public IGraphTraveller<T> CreateTraveller<T>()
        {
            var traveller = Provider.Get<T>();
            return traveller;
        }

        protected override ITypedSerializer<T> CreateSerializer<T>()
        {
            return new PackedDataSerializer<T>();
        }

        public byte[] Pack<T>(T graph)
        {
            var stream = new MemoryStream();
            using (var buffer = new BinaryWriteBuffer(1024, stream)) {
                var visitor = new PackedDataWriteVisitor(buffer);

                var traveller = CreateTraveller<T>();
                traveller.Travel(visitor, graph);
            }
            return stream.ToArray();
        }

        public static string GetFilledDataBlockHexString()
        {
            var bytes = GetFilledDataBlockBlob();
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
            var hex = "0x" + string.Join("", bytes.Select(b => b.ToString("X")));
            Assert.NotNull(hex);
            return hex;
        }

        public static byte[] GetFilledDataBlockBlob()
        {
            var stream = new MemoryStream();
            using (var buffer = new BinaryWriteBuffer(1024, stream)) {
                var visitor = new PackedDataWriteVisitor(buffer);
                var traveller = DataBlockHardCodedTraveller.Create();
                traveller.Travel(visitor, DataBlock.Filled());
            }

            var bytes = stream.ToArray();
            return bytes;
        }
    }
}
