/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.IO;
using System.Reflection;
using Aeter.Ratio.Serialization;
using Xunit;

namespace Aeter.Ratio.Test.Serialization
{
    public abstract class SerializationTestContext
    {

        protected SerializationTestContext()
        {
        }

        protected abstract ITypedSerializer<T> CreateSerializer<T>();

        public byte[] Serialize<T>(T graph)
        {
            var serializer = CreateSerializer<T>();
            using (var stream = new MemoryStream()) {
                serializer.Serialize(stream, graph);
                return stream.ToArray();
            }
        }

        public T SerializeAndDeserialize<T>(T graph)
        {
            var serializer = CreateSerializer<T>();
            using (var stream = new MemoryStream()) {
                serializer.Serialize(stream, graph);
                stream.Seek(0, SeekOrigin.Begin);
                return serializer.Deserialize(stream);
            }
        }

        public T AssertBinarySingleProperty<T>(T graph)
        {
            var actual = SerializeAndDeserialize(graph);

            var type = typeof (T);
            var property = type.GetProperty("Value");
            if (property != null) {
                var expectedValue = property.GetValue(graph);
                var actualValue = property.GetValue(actual);

                Assert.Equal(expectedValue, actualValue);
            }

            return actual;
        }

    }
}
