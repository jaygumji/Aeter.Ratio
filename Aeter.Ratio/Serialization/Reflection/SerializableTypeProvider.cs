/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Aeter.Ratio.Serialization.Reflection
{
    public class SerializableTypeProvider
    {
        private const BindingFlags PropertyFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly SerializationReflectionInspector _inspector;
        private readonly Dictionary<Type, SerializableType> _types;
        public ITypeProvider Provider { get; }

        public SerializableTypeProvider(SerializationReflectionInspector inspector, ITypeProvider provider)
        {
            _inspector = inspector;
            _types = new Dictionary<Type, SerializableType>();
            Provider = provider;
        }

        public SerializableType GetOrCreate(Type type)
        {
            if (_types.TryGetValue(type, out var serializableType))
                return serializableType;

            serializableType = Build(type);
            _types.Add(type, serializableType);
            return serializableType;
        }

        private SerializableType Build(Type type)
        {
            var properties = type.GetTypeInfo().GetProperties(PropertyFlags);

            UInt32 nextIndex = 1;

            var serializableProperties = new Dictionary<string, SerializableProperty>();
            foreach (var property in properties) {
                if (!_inspector.CanBeSerialized(type, property)) continue;
                if (serializableProperties.ContainsKey(property.Name))
                    throw InvalidGraphException.DuplicateProperties(type, property);

                var metadata = _inspector.AcquirePropertyMetadata(type, property, ref nextIndex);
                
                var ser = new SerializableProperty(property, metadata, Provider);
                serializableProperties.Add(property.Name, ser);
            }
            return new SerializableType(type, serializableProperties);
        }
    }
}