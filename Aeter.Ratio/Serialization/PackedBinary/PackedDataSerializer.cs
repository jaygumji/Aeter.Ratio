/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using System.IO;

namespace Aeter.Ratio.Serialization.PackedBinary
{
    public class PackedDataSerializer<T> : ITypedSerializer<T>
    {
        private readonly IBinaryBufferPool _bufferPool;
        private readonly SerializationEngine _engine;

        public PackedDataSerializer() : this(new BinaryBufferFactory())
        {
        }

        public PackedDataSerializer(IBinaryBufferPool bufferPool)
        {
            _bufferPool = bufferPool;
            _engine = new SerializationEngine();
        }

        void ITypedSerializer.Serialize(Stream stream, object graph)
        {
            Serialize(stream, (T)graph);
        }

        public T Deserialize(Stream stream)
        {
            var visitor = new PackedDataReadVisitor(stream);
            return _engine.Deserialize<T>(visitor);
        }

        public void Serialize(Stream stream, T graph)
        {
            using (var buffer = _bufferPool.AcquireWriteBuffer(stream)) {
                var visitor = new PackedDataWriteVisitor(buffer);
                _engine.Serialize(visitor, graph);
            }
        }

        object? ITypedSerializer.Deserialize(Stream stream)
        {
            return Deserialize(stream);
        }
    }
}
