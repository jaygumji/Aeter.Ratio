/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.IO;
using System;

namespace Aeter.Ratio.Serialization.PackedBinary
{
    public class PackedDataSerializer : ISerializer
    {
        private readonly BinaryBufferPool _bufferPool;
        private readonly SerializationEngine _engine;

        public PackedDataSerializer() : this(BinaryBufferPool.Default)
        {
        }

        public PackedDataSerializer(BinaryBufferPool bufferPool)
        {
            _bufferPool = bufferPool;
            _engine = new SerializationEngine();
        }

        public void Serialize(IBinaryWriteStream stream, object graph)
        {
            using var buffer = _bufferPool.AcquireWriteBuffer(stream);
            var visitor = new PackedDataWriteVisitor(buffer);
            _engine.Serialize(visitor, graph);
        }

        public T Deserialize<T>(IBinaryReadStream stream)
        {
            using var buffer = _bufferPool.AcquireReadBuffer(stream);
            var visitor = new PackedDataReadVisitor(buffer);
            return _engine.Deserialize<T>(visitor);
        }

        public void Serialize<T>(IBinaryWriteStream stream, T graph)
        {
            using var buffer = _bufferPool.AcquireWriteBuffer(stream);
            var visitor = new PackedDataWriteVisitor(buffer);
            _engine.Serialize(visitor, graph);
        }

        public object? Deserialize(Type type, IBinaryReadStream stream)
        {
            using var buffer = _bufferPool.AcquireReadBuffer(stream);
            var visitor = new PackedDataReadVisitor(buffer);
            return _engine.Deserialize(visitor, type);
        }
    }
}
