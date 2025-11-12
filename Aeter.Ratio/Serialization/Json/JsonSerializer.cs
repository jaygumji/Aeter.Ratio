/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.IO;
using System;
using System.IO;

namespace Aeter.Ratio.Serialization.Json
{

    public class JsonSerializer(BinaryBufferPool bufferPool) : ISerializer
    {
        private readonly SerializationEngine _engine = new();

        public IFieldNameResolver FieldNameResolver { get; set; } = new CamelCaseFieldNameResolver();
        public JsonEncoding Encoding { get; set; } = JsonEncoding.UTF8;

        public JsonSerializer() : this(BinaryBufferPool.Default)
        {
        }

        public void Serialize(IBinaryWriteStream stream, object graph)
        {
            using var buffer = bufferPool.AcquireWriteBuffer(stream);
            var visitor = new JsonWriteVisitor(Encoding, FieldNameResolver, buffer);
            _engine.Serialize(visitor, graph);
        }

        public object? Deserialize(Type type, IBinaryReadStream stream)
        {
            using var buffer = bufferPool.AcquireReadBuffer(stream);
            var visitor = new JsonReadVisitor(Encoding, FieldNameResolver, buffer);
            return _engine.Deserialize(visitor, type);
        }

        public T Deserialize<T>(string json)
        {
            var bytes = Encoding.BaseEncoding.GetBytes(json);
            using var stream = new MemoryStream(bytes);
            return Deserialize<T>(BinaryStream.MemoryStream(stream));
        }

        public T Deserialize<T>(IBinaryReadStream stream)
        {
            using var buffer = bufferPool.AcquireReadBuffer(stream);
            var visitor = new JsonReadVisitor(Encoding, FieldNameResolver, buffer);
            return _engine.Deserialize<T>(visitor);
        }

        public void Serialize<T>(IBinaryWriteStream stream, T graph)
        {
            using var buffer = bufferPool.AcquireWriteBuffer(stream);
            var visitor = new JsonWriteVisitor(Encoding, FieldNameResolver, buffer);
            _engine.Serialize(visitor, graph);
        }
    }
}
