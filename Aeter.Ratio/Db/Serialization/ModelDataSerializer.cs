/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.DependencyInjection;
using Aeter.Ratio.IO;
using Aeter.Ratio.Serialization;
using System;

namespace Aeter.Ratio.Db.Serialization
{
    public class ModelDataSerializer(IInstanceFactory instanceFactory, BinaryBufferPool bufferPool) : ISerializer
    {
        internal static readonly ModelSerializationReflectionInspector Inspector = new ModelSerializationReflectionInspector();
        private readonly SerializationEngine _engine = new(instanceFactory,
                new GraphTravellerProvider(new DynamicGraphTravellerFactory(instanceFactory, Inspector)));

        public ModelDataSerializer() : this(BinaryBufferPool.Default)
        {
        }

        public ModelDataSerializer(BinaryBufferPool bufferPool) : this(new DependencyInjectionContainer(), bufferPool)
        {
        }

        public void Serialize(IBinaryWriteStream stream, object graph)
        {
        }

        public T Deserialize<T>(IBinaryReadStream stream)
        {
            using var buffer = bufferPool.AcquireReadBuffer(stream);
            var visitor = new ModelDataReadVisitor(buffer);
            return _engine.Deserialize<T>(visitor);
        }

        public void Serialize<T>(IBinaryWriteStream stream, T graph)
        {
            using var buffer = bufferPool.AcquireWriteBuffer(stream);
            var visitor = new ModelDataWriteVisitor(buffer);
            _engine.Serialize(visitor, graph);
        }

        public object? Deserialize(Type type, IBinaryReadStream stream)
        {
            using var buffer = bufferPool.AcquireReadBuffer(stream);
            var visitor = new ModelDataReadVisitor(buffer);
            return _engine.Deserialize(visitor, type);
        }
    }
}
