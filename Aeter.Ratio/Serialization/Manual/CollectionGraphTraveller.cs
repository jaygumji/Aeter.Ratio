/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;

namespace Aeter.Ratio.Serialization.Manual
{
    public class CollectionGraphTraveller<TCollection, TElement> : IGraphTraveller<TCollection>, ICollectionGraphTraveller
        where TCollection : IList<TElement>
    {
        private readonly IGraphTraveller<TElement> _elementTraveller;
        private readonly SerializationInstanceFactory? _instanceFactory;
        private readonly IValueVisitor<TElement>? _valueVisitor;
        private readonly Type? _elementType;

        public LevelType Level => LevelType.Collection;

        public CollectionGraphTraveller(IGraphTraveller<TElement> elementTraveller, SerializationInstanceFactory instanceFactory)
        {
            _elementTraveller = elementTraveller;
            if (elementTraveller is EmptyGraphTraveller) {
                _valueVisitor = ValueVisitor.Create<TElement>();
            }
            else {
                _instanceFactory = instanceFactory;
                _elementType = typeof(TElement);
            }
        }

        void IGraphTraveller.Travel(IReadVisitor visitor, object graph)
        {
            Travel(visitor, (TCollection)graph);
        }

        void IGraphTraveller.Travel(IWriteVisitor visitor, object graph)
        {
            Travel(visitor, (TCollection)graph);
        }

        public void Travel(IReadVisitor visitor, TCollection graph)
        {
            var itemArgs = VisitArgs.CollectionItem;
            if (_valueVisitor != null) {
                while (_valueVisitor.TryVisitValue(visitor, itemArgs, out var value)) {
                    graph.Add(value);
                }
                return;
            }
            while (visitor.TryVisit(itemArgs) == ValueState.Found) {
                var element = (TElement)_instanceFactory!.CreateInstance(_elementType!);
                _elementTraveller.Travel(visitor, element);
                graph.Add(element);
                visitor.Leave(itemArgs);
            }
        }

        public void Travel(IWriteVisitor visitor, TCollection graph)
        {
            var itemArgs = VisitArgs.CollectionItem;
            foreach (var element in graph) {
                if (_valueVisitor != null) {
                    _valueVisitor.VisitValue(visitor, itemArgs, element);
                }
                else {
                    visitor.Visit(element, itemArgs);
                    _elementTraveller.Travel(visitor, element);
                    visitor.Leave(element, itemArgs);
                }
            }
        }
    }
}