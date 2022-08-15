/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.Serialization.Json;
using System;
using System.IO;

namespace Aeter.Ratio.Serialization.Bson
{
    public class BsonSerializer : ISerializer
    {
        private readonly IBinaryBufferPool _bufferPool;
        private readonly SerializationEngine _engine;

        public IFieldNameResolver FieldNameResolver { get; set; }
        public BsonEncoding Encoding { get; set; }

        public BsonSerializer() : this(BinaryBufferPool.Instance)
        {
        }

        public BsonSerializer(IBinaryBufferPool bufferPool)
        {
            _bufferPool = bufferPool;
            _engine = new SerializationEngine();
            FieldNameResolver = new CamelCaseFieldNameResolver();
            Encoding = BsonEncoding.UTF8;
        }

        public void Serialize(Stream stream, object graph)
        {
            using (var buffer = _bufferPool.AcquireWriteBuffer(stream)) {
                var visitor = new BsonWriteVisitor(Encoding, FieldNameResolver, buffer);
                _engine.Serialize(visitor, graph);
            }
        }

        public object? Deserialize(Type type, Stream stream)
        {
            using (var buffer = _bufferPool.AcquireReadBuffer(stream)) {
                var visitor = new BsonReadVisitor(Encoding, FieldNameResolver, buffer);
                return _engine.Deserialize(visitor, type);
            }
        }
    }

    public class BsonSerializer<T> : ITypedSerializer<T>
    {
        private readonly IBinaryBufferPool _bufferPool;
        private readonly SerializationEngine _engine;

        public IFieldNameResolver FieldNameResolver { get; set; }
        public BsonEncoding Encoding { get; set; }

        public BsonSerializer() : this(new BinaryBufferFactory())
        {
        }

        public BsonSerializer(IBinaryBufferPool bufferPool)
        {
            _bufferPool = bufferPool;
            _engine = new SerializationEngine();
            FieldNameResolver = new CamelCaseFieldNameResolver();
            Encoding = BsonEncoding.UTF8;
        }

        void ITypedSerializer.Serialize(Stream stream, object graph)
        {
            Serialize(stream, (T)graph);
        }

        public T Deserialize(byte[] bson)
        {
            using (var stream = new MemoryStream(bson)) {
                return Deserialize(stream);
            }
        }

        public T Deserialize(Stream stream)
        {
            using (var buffer = _bufferPool.AcquireReadBuffer(stream)) {
                var visitor = new BsonReadVisitor(Encoding, FieldNameResolver, buffer);
                return _engine.Deserialize<T>(visitor);
            }
        }

        public void Serialize(Stream stream, T graph)
        {
            using (var buffer = _bufferPool.AcquireWriteBuffer(stream)) {
                var visitor = new BsonWriteVisitor(Encoding, FieldNameResolver, buffer);
                _engine.Serialize(visitor, graph);
            }
        }

        object? ITypedSerializer.Deserialize(Stream stream)
        {
            return Deserialize(stream);
        }
    }
}
