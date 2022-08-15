/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using Aeter.Ratio.Reflection;

namespace Aeter.Ratio.Serialization.Reflection.Graph
{
    public class GraphTypeFactory
    {
        private static readonly Dictionary<Type, Func<SerializableProperty, VisitArgs, IGraphProperty>> PredefinedGraphPropertyFactories
            = new Dictionary<Type, Func<SerializableProperty, VisitArgs, IGraphProperty>> {
            {typeof (Int16), (ser, args) => new Int16GraphProperty(ser, args)}
        }; 

        private readonly SerializableTypeProvider _provider;
        private readonly Dictionary<Type, IGraphType> _graphTypes;

        public GraphTypeFactory(SerializableTypeProvider provider)
        {
            _provider = provider;
            _graphTypes = new Dictionary<Type, IGraphType>();
        }

        public IGraphType GetOrCreate(Type type)
        {
            var serType = _provider.GetOrCreate(type);
            var visitArgsFactory = new VisitArgsFactory(_provider, type);

            var graphProperties = new List<IGraphProperty>();
            var properties = serType.Properties;
            foreach (var property in properties) {
                var args = visitArgsFactory.Construct(property.Ref.Name);
                var graphProperty = Create(property, args);
                graphProperties.Add(graphProperty);
            }

            var graphType = new ComplexGraphType(graphProperties);
            _graphTypes.Add(type, graphType);
            return graphType;
        }

        private IGraphProperty Create(SerializableProperty ser, VisitArgs args)
        {
            if (PredefinedGraphPropertyFactories.TryGetValue(ser.Ref.PropertyType, out var factory))
                return factory(ser, args);

            if (ser.Ext.TryGetDictionaryTypeInfo(out var dictionaryTypeInfo)) {
                
            }

            if (ser.Ext.TryGetCollectionTypeInfo(out var collectionTypeInfo)) {
                
            }

            return new ComplexGraphProperty(ser, GetOrCreate(ser.Ref.PropertyType), args);
        }

    }
}