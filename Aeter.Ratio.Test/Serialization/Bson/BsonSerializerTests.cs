/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.IO;
using Aeter.Ratio.Serialization.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Aeter.Ratio.Test.Serialization.Bson
{
    public class BsonSerializerTests
    {
        private readonly BsonSerializationTestContext _context;

        public BsonSerializerTests(ITestOutputHelper output)
        {
            _context = new BsonSerializationTestContext(output);
        }

        [Fact]
        public void SerializeSmall()
        {
            var expected = Convert.FromBase64String("OQAAAAVpZAAQAAAABAZRHE8n9dBLr05Ft4bYjaoQbm8AAQAAAAJjYXRlZ29yeQAFAAAATWluaQAA");
            var block = SmallBlock.Filled();
            _context.AssertSerialize(expected, block);
        }

        [Fact]
        public void SerializeLarge()
        {
            var expected = Convert.FromBase64String("jgMAAAVpZAAQAAAABEKRFfWjufpFha7gyeYJkKkCc3RyaW5nAAwAAABIZWxsbyBXb3JsZAAQaW50MTYACk8AABBpbnQzMgBel5gCEmludDY0AHGywJhz7V08EHVJbnQxNgBC+wAAEnVJbnQzMgD21+nNAAAAABJ1SW50NjQAOeny0Z3Irg0Bc2luZ2xlAAAAAMDMDEBAAWRvdWJsZQC4HoXrUQWxQBF0aW1lU3BhbgBAyEACAAAAABNkZWNpbWFsAJTlqgIAAAAAAAAAAAAAAwARZGF0ZVRpbWUAAC27HEUBAAAQYnl0ZQAqAAAACGJvb2xlYW4AAQVibG9iAAMAAAAAAQIDBG1lc3NhZ2VzAEYAAAACMAAGAAAAVGVzdDEAAjEABgAAAFRlc3QyAAIyAAYAAABUZXN0MwACMwAGAAAAVGVzdDQAAjQABgAAAFRlc3Q1AAAEc3RhbXBzABAAAAARMAAAK74bJwEAAAADcmVsYXRpb24AdAAAAAVpZAAQAAAABNT3jvZib2tHvF5xrYZUmmMCbmFtZQALAAAAQ29ubmVjdGlvbgACZGVzY3JpcHRpb24AJQAAAEdlbmVyaWMgY29ubmVjdGlvbiBiZXR3ZWVuIHJlbGF0aW9ucwAQdmFsdWUATQAAAAAKZHVtbXlSZWxhdGlvbgAEc2Vjb25kYXJ5UmVsYXRpb25zAGgAAAADMABgAAAABWlkABAAAAAEFrbtyewmu0SecD84x8GMkQJuYW1lAAYAAABMaW5lMQACZGVzY3JpcHRpb24AFgAAAEZpcnN0IGxpbmUgb2YgY2FzY2FkZQAQdmFsdWUAuwAAAAAAA2luZGV4ZWRWYWx1ZXMAJQAAABBWMQABAAAAEFYyAAIAAAAQVjMAAwAAABBWNAAEAAAAAANjYXRlZ29yaWVzANcAAAADMQBOAAAAAm5hbWUACAAAAFdhcm5pbmcAAmRlc2NyaXB0aW9uABUAAABXYXJuaW5nIG9mIHNvbWV0aGluZwAFaW1hZ2UABQAAAAABAgMEBQADMgBOAAAAAm5hbWUABgAAAEVycm9yAAJkZXNjcmlwdGlvbgATAAAARXJyb3Igb2Ygc29tZXRoaW5nAAVpbWFnZQAJAAAAAAECAwQFBgcICQADMwAtAAAAAm5hbWUACgAAAFRlbXBvcmFyeQAKZGVzY3JpcHRpb24ACmltYWdlAAAAAA==");
            //var expected = "{\"id\":\"f5159142-b9a3-45fa-85ae-e0c9e60990a9\",\"string\":\"Hello World\",\"int16\":20234,\"int32\":43554654,\"int64\":4349893895849554545,\"uInt16\":64322,\"uInt32\":3454654454,\"uInt64\":9859459485984955454,\"single\":32.1,\"double\":4357.32,\"timeSpan\":\"10:30:00\",\"decimal\":44754.324,\"dateTime\":\"2014-04-01T10:00:00.0000000\",\"byte\":42,\"boolean\":true,\"blob\":\"AQID\",\"messages\":[\"Test1\",\"Test2\",\"Test3\",\"Test4\",\"Test5\"],\"stamps\":[\"2010-03-01T22:00:00.0000000\"],\"relation\":{\"id\":\"f68ef7d4-6f62-476b-bc5e-71ad86549a63\",\"name\":\"Connection\",\"description\":\"Generic connection between relations\",\"value\":77},\"dummyRelation\":null,\"secondaryRelations\":[{\"id\":\"c9edb616-26ec-44bb-9e70-3f38c7c18c91\",\"name\":\"Line1\",\"description\":\"First line of cascade\",\"value\":187}],\"indexedValues\":{\"V1\":1,\"V2\":2,\"V3\":3,\"V4\":4},\"categories\":{\"1\":{\"name\":\"Warning\",\"description\":\"Warning of something\",\"image\":\"AQIDBAU=\"},\"2\":{\"name\":\"Error\",\"description\":\"Error of something\",\"image\":\"AQIDBAUGBwgJ\"},\"3\":{\"name\":\"Temporary\",\"description\":null,\"image\":null}}}";
            var block = BsonDataBlock.Filled();
            _context.AssertSerialize(expected, block);
            _context.AssertSerialize(expected, block);
        }

        [Fact]
        public void SerializeRootDictionary()
        {
            var expected = Convert.FromBase64String("KgAAAAIxAAYAAABIZWxsbwACMgAEAAAAYmlnAAIzAAYAAAB3b3JsZAAA");
            //var expected = "{\"1\":\"Hello\",\"2\":\"big\",\"3\":\"world\"}";
            var graph = new Dictionary<int, string> {
                {1, "Hello"},
                {2, "big"},
                {3, "world"}
            };
            _context.AssertSerialize(expected, graph);
            _context.AssertSerialize(expected, graph);
        }

        [Fact]
        public void ReadValue_ReturnsScalarProperty()
        {
            var serializer = new BsonSerializer();
            var block = BsonDataBlock.Filled();
            var bson = _context.Serialize(block);
            using var buffer = CreateReadBuffer(bson);

            var values = serializer.ReadValue(buffer, "String", typeof(string));

            Assert.Single(values);
            Assert.Equal(block.String, values[0]);
        }

        [Fact]
        public void ReadValue_ReturnsNestedProperty()
        {
            var serializer = new BsonSerializer();
            var block = BsonDataBlock.Filled();
            var bson = _context.Serialize(block);
            using var buffer = CreateReadBuffer(bson);

            var values = serializer.ReadValue(buffer, "Relation.Name", typeof(string));

            Assert.Single(values);
            Assert.Equal(block.Relation?.Name, values[0]);
        }

        [Fact]
        public void ReadValue_ReturnsEntireCollectionWhenUsingWildcard()
        {
            var serializer = new BsonSerializer();
            var block = BsonDataBlock.Filled();
            var bson = _context.Serialize(block);
            using var buffer = CreateReadBuffer(bson);

            var values = serializer.ReadValue(buffer, "Messages[]", typeof(string));

            Assert.Equal(block.Messages, values.Cast<string>());
        }

        [Fact]
        public void ReadValue_ReturnsSpecificIndexedItem()
        {
            var serializer = new BsonSerializer();
            var block = BsonDataBlock.Filled();
            var bson = _context.Serialize(block);
            using var buffer = CreateReadBuffer(bson);

            var values = serializer.ReadValue(buffer, "SecondaryRelations[0].Name", typeof(string));

            Assert.Single(values);
            Assert.Equal(block.SecondaryRelations?[0].Name, values[0]);
        }

        [Fact]
        public void ReadValue_ReturnsEmptyWhenPathDoesNotExist()
        {
            var serializer = new BsonSerializer();
            var block = BsonDataBlock.Filled();
            var bson = _context.Serialize(block);
            using var buffer = CreateReadBuffer(bson);

            var values = serializer.ReadValue(buffer, "Unknown.Path", typeof(string));

            Assert.Empty(values);
        }

        private static BinaryReadBuffer CreateReadBuffer(byte[] data)
        {
            var stream = new MemoryStream(data);
            return new BinaryReadBuffer(4096, BinaryStream.MemoryStream(stream));
        }
    }
}
