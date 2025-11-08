/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.DependencyInjection;
using Aeter.Ratio.Serialization;
using System.IO;

namespace Aeter.Ratio.Db.Serialization
{
    internal static class ModelDataSerializer
    {
        internal static readonly ModelSerializationReflectionInspector Inspector = new ModelSerializationReflectionInspector();
    }
    public class ModelDataSerializer<T> : ITypedSerializer<T>
    {
        private readonly BinaryBufferPool _bufferPool;
        private readonly SerializationEngine _engine;

        public ModelDataSerializer() : this(BinaryBufferPool.Default)
        {
        }

        public ModelDataSerializer(BinaryBufferPool bufferPool) : this(new DependencyInjectionContainer(), bufferPool)
        {
        }

        public ModelDataSerializer(IInstanceFactory instanceFactory, BinaryBufferPool bufferPool)
        {
            _bufferPool = bufferPool;
            _engine = new SerializationEngine(instanceFactory,
                new GraphTravellerProvider(new DynamicGraphTravellerFactory(instanceFactory, ModelDataSerializer.Inspector)));
        }

        void ITypedSerializer.Serialize(Stream stream, object graph)
        {
            Serialize(stream, (T)graph);
        }

        public T Deserialize(Stream stream)
        {
            var visitor = new ModelDataReadVisitor(stream);
            return _engine.Deserialize<T>(visitor);
        }

        public void Serialize(Stream stream, T graph)
        {
            using (var buffer = _bufferPool.AcquireWriteBuffer(stream)) {
                var visitor = new ModelDataWriteVisitor(buffer);
                _engine.Serialize(visitor, graph);
            }
        }

        object? ITypedSerializer.Deserialize(Stream stream)
        {
            return Deserialize(stream);
        }
    }
}
