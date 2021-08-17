/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.IoC;
using Aeter.Ratio.Serialization.Manual;
using System;

namespace Aeter.Ratio.Serialization
{
    public class SerializationEngine
    {
        private static GraphTravellerProvider CreateDefaultProvider(IInstanceFactory instanceFactory)
        {
            return new GraphTravellerProvider(new DynamicGraphTravellerFactory(instanceFactory));
        }

        private readonly SerializationInstanceFactory _instanceFactory;
        private readonly IGraphTravellerProvider _travellerProvider;

        public SerializationEngine()
            : this(new IoCContainer())
        {
        }

        public SerializationEngine(IGraphTravellerProvider travellerProvider)
            : this(new IoCContainer(), travellerProvider)
        {
        }

        public SerializationEngine(IInstanceFactory instanceFactory)
            : this(instanceFactory, CreateDefaultProvider(instanceFactory))
        {
        }

        public SerializationEngine(IInstanceFactory instanceFactory, IGraphTravellerProvider travellerProvider)
        {
            _instanceFactory = new SerializationInstanceFactory(instanceFactory);
            _travellerProvider = travellerProvider;
        }

        private static VisitArgs GetRootArgs(IGraphTraveller traveller)
        {
            if (traveller == null) return VisitArgs.CreateRoot(LevelType.Value);
            if (traveller is ICollectionGraphTraveller) return VisitArgs.CreateRoot(LevelType.Collection);
            if (traveller is IDictionaryGraphTraveller) return VisitArgs.CreateRoot(LevelType.Dictionary);
            return VisitArgs.CreateRoot(LevelType.Single);
        }

        public void Serialize(IWriteVisitor visitor, object graph)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var type = graph.GetType();

            var traveller = _travellerProvider.Get(type);
            var rootArgs = GetRootArgs(traveller);
            if (rootArgs.Type == LevelType.Value) {
                var valueVisitor = ValueVisitor.Create(type);
                valueVisitor.VisitValue(visitor, rootArgs, graph);
            }
            else {
                visitor.Visit(graph, rootArgs);
                traveller.Travel(visitor, graph);
                visitor.Leave(graph, rootArgs);
            }
        }

        public void Serialize<T>(IWriteVisitor visitor, T graph)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var traveller = _travellerProvider.Get<T>();
            var rootArgs = GetRootArgs(traveller);
            if (rootArgs.Type == LevelType.Value) {
                var valueVisitor = ValueVisitor.Create<T>();
                valueVisitor.VisitValue(visitor, rootArgs, graph);
            }
            else {
                visitor.Visit(graph, rootArgs);
                traveller.Travel(visitor, graph);
                visitor.Leave(graph, rootArgs);
            }
        }

        public object Deserialize(IReadVisitor visitor, Type type)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            if (type == null) throw new ArgumentNullException(nameof(type));

            var traveller = _travellerProvider.Get(type);
            var rootArgs = GetRootArgs(traveller);
            if (rootArgs.Type == LevelType.Value) {
                var valueVisitor = ValueVisitor.Create(type);
                return valueVisitor.TryVisitValue(visitor, rootArgs, out var value)
                    ? value : default;
            }

            if (visitor.TryVisit(rootArgs) != ValueState.Found) {
                return default;
            }

            var graph = _instanceFactory.CreateInstance(type);
            traveller.Travel(visitor, graph);
            visitor.Leave(rootArgs);

            return graph;
        }

        public void DeserializeTo(IReadVisitor visitor, Type type, object graph)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var traveller = _travellerProvider.Get(type);
            var rootArgs = GetRootArgs(traveller);
            if (rootArgs.Type == LevelType.Value) {
                throw new NotSupportedException("Values are not supported when deserializing into.");
            }

            if (visitor.TryVisit(rootArgs) != ValueState.Found) {
                return;
            }

            traveller.Travel(visitor, graph);
            visitor.Leave(rootArgs);
        }

        public T Deserialize<T>(IReadVisitor visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));

            var traveller = _travellerProvider.Get<T>();
            var rootArgs = GetRootArgs(traveller);
            if (rootArgs.Type == LevelType.Value) {
                var valueVisitor = ValueVisitor.Create<T>();
                return valueVisitor.TryVisitValue(visitor, rootArgs, out var value)
                    ? value : default;
            }

            if (visitor.TryVisit(rootArgs) != ValueState.Found) {
                return default;
            }

            var graph = (T)_instanceFactory.CreateInstance(typeof(T));
            traveller.Travel(visitor, graph);
            visitor.Leave(rootArgs);

            return graph;
        }

        public void DeserializeTo<T>(IReadVisitor visitor, T graph)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var traveller = _travellerProvider.Get<T>();
            var rootArgs = GetRootArgs(traveller);
            if (rootArgs.Type == LevelType.Value) {
                throw new NotSupportedException("Values are not supported when deserializing into.");
            }

            if (visitor.TryVisit(rootArgs) != ValueState.Found) {
                return;
            }

            traveller.Travel(visitor, graph);
            visitor.Leave(rootArgs);
        }

    }
}
