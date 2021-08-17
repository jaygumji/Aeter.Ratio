/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection;
using Aeter.Ratio.Reflection.Emit;
using Aeter.Ratio.Serialization.Manual;
using Aeter.Ratio.Serialization.Reflection;
using Aeter.Ratio.Serialization.Reflection.Emit;
using System;

namespace Aeter.Ratio.Serialization
{
    public class DynamicGraphTravellerFactory : IGraphTravellerFactory
    {
        private readonly SerializableTypeProvider _typeProvider;
        private readonly AssemblyBuilderKit _assemblyBuilder;
        private readonly IntermediateGraphTravellerCollection _travellers;
        private readonly SerializationInstanceFactory _serializationInstanceFactory;

        public DynamicTravellerMembers Members { get; }

        private readonly VisitArgsTypeFactory _visitArgsFactory;

        public DynamicGraphTravellerFactory(IInstanceFactory instanceFactory) : this(
            instanceFactory,
            new SerializableTypeProvider(new SerializationReflectionInspector(), new CachedTypeProvider()))
        {
        }

        public DynamicGraphTravellerFactory(IInstanceFactory instanceFactory, SerializationReflectionInspector inspector)
            : this(instanceFactory, new SerializableTypeProvider(inspector, new CachedTypeProvider()))
        {
        }

        public DynamicGraphTravellerFactory(IInstanceFactory instanceFactory, SerializableTypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
            _travellers = new IntermediateGraphTravellerCollection();
            _assemblyBuilder = new AssemblyBuilderKit();
            Members = new DynamicTravellerMembers();
            _visitArgsFactory = new VisitArgsTypeFactory(typeProvider);
            _serializationInstanceFactory = new SerializationInstanceFactory(instanceFactory);
        }

        public IGraphTraveller Create(Type type)
        {
            var traveller = GetOrCreateRoot(type);
            _travellers.Complete();
            return traveller?.Instance;
        }

        private IntermediateGraphTraveller GetOrCreateRoot(Type type)
        {
            if (_travellers.TryGet(type, out var traveller)) {
                return traveller;
            }

            var containerTypeInfo = type.GetContainerTypeInfo();
            var classification = type.GetClassification(containerTypeInfo);
            if (classification == TypeClassification.Collection) {
                var elementType = containerTypeInfo.AsCollection().ElementType;
                var elementTraveller = GetOrCreateRoot(elementType);
                if (!_travellers.TryGet(type, out traveller)) {
                    var travellerType = typeof(CollectionGraphTraveller<,>).MakeGenericType(type, elementType);
                    traveller = new IntermediateGraphTraveller(type, travellerType, _visitArgsFactory.ConstructWith(type), elementTraveller, _serializationInstanceFactory);
                    _travellers.Register(traveller);
                }
                return traveller;
            }
            if (classification == TypeClassification.Dictionary) {
                var dictContainer = containerTypeInfo.AsDictionary();
                var keyType = dictContainer.KeyType;
                var valueType = dictContainer.ValueType;

                var keyTraveller = GetOrCreateRoot(keyType);
                var valueTraveller = GetOrCreateRoot(valueType);
                if (!_travellers.TryGet(type, out traveller)) {
                    var travellerType = typeof(DictionaryGraphTraveller<,,>).MakeGenericType(type, keyType, valueType);
                    traveller = new IntermediateGraphTraveller(type, travellerType, _visitArgsFactory.ConstructWith(type), keyTraveller, valueTraveller, _serializationInstanceFactory);
                    _travellers.Register(traveller);
                }
                return traveller;
            }
            if (classification == TypeClassification.Complex) {
                var builder = GetDynamicTravellerBuilder(type);
                traveller = new IntermediateGraphTraveller(builder, _visitArgsFactory.ConstructWith(type));
                _travellers.Register(traveller);
                return traveller;
            }
            else if (classification == TypeClassification.Value) {
                return null;
            }
            else if (classification == TypeClassification.Nullable) {
                var underlyingType = containerTypeInfo.AsNullable().ElementType;
                if (_travellers.TryGet(underlyingType, out traveller)) return traveller;
                return GetOrCreateRoot(underlyingType);
            }
            else {
                throw new NotSupportedException($"The type {type.FullName} is currenty not supported.");
            }
        }

        public DynamicTraveller PredefineDynamicTraveller(Type type)
        {
            if (_travellers.TryGet(type, out var traveller)) return traveller.Builder.DynamicTraveller;

            var builder = GetDynamicTravellerBuilder(type);
            traveller = new IntermediateGraphTraveller(builder, _visitArgsFactory.ConstructWith(type));
            _travellers.Register(traveller);
            return builder.DynamicTraveller;
        }

        private DynamicTravellerBuilder GetDynamicTravellerBuilder(Type graphType)
        {
            var graphTravellerType = typeof(IGraphTraveller<>).MakeGenericType(graphType);
            var classFullName = string.Concat(graphType.Namespace, '.', graphType.Name, "Traveller");
            var classBuilder = _assemblyBuilder.DefineClass(classFullName, typeof(object), new[] { graphTravellerType });

            return new DynamicTravellerBuilder(this, classBuilder, _typeProvider, graphType);
        }
    }
}