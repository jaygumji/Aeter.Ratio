/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.IO;
using System;
using System.IO;

namespace Aeter.Ratio.Serialization.Bson
{
    public class BsonSerializer(BinaryBufferPool bufferPool) : ISerializer, IEntitySerializer
    {
        private readonly SerializationEngine _engine = new();

        public IFieldNameResolver FieldNameResolver { get; set; } = new CamelCaseFieldNameResolver();
        public BsonEncoding Encoding { get; set; } = BsonEncoding.UTF8;
        public static readonly ARID ARID = new("bson");

        public BsonSerializer() : this(BinaryBufferPool.Default)
        {
        }

        public void Serialize(IBinaryWriteStream stream, object graph)
        {
            using var buffer = bufferPool.AcquireWriteBuffer(stream);
            Serialize(buffer, graph);
        }

        public object? Deserialize(Type type, IBinaryReadStream stream)
        {
            using var buffer = bufferPool.AcquireReadBuffer(stream);
            return Deserialize(type, buffer);
        }

        public T Deserialize<T>(byte[] bson)
        {
            using var stream = new MemoryStream(bson);
            return Deserialize<T>(BinaryStream.MemoryStream(stream));
        }

        public T Deserialize<T>(IBinaryReadStream stream)
        {
            using var buffer = bufferPool.AcquireReadBuffer(stream);
            return Deserialize<T>(buffer);
        }

        public void Serialize<T>(IBinaryWriteStream stream, T graph)
        {
            using var buffer = bufferPool.AcquireWriteBuffer(stream);
            Serialize(buffer, graph);
        }

        public void Serialize(BinaryWriteBuffer buffer, object graph)
        {
            var visitor = new BsonWriteVisitor(Encoding, FieldNameResolver, buffer);
            _engine.Serialize(visitor, graph);
        }

        public object? Deserialize(Type type, BinaryReadBuffer buffer)
        {
            var visitor = new BsonReadVisitor(Encoding, FieldNameResolver, buffer);
            return _engine.Deserialize(visitor, type);
        }

        public T Deserialize<T>(BinaryReadBuffer buffer)
        {
            var visitor = new BsonReadVisitor(Encoding, FieldNameResolver, buffer);
            return _engine.Deserialize<T>(visitor);
        }

        public void Serialize<T>(BinaryWriteBuffer buffer, T graph)
        {
            var visitor = new BsonWriteVisitor(Encoding, FieldNameResolver, buffer);
            _engine.Serialize(visitor, graph);
        }
    }
}
