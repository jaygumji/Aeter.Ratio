/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;

namespace Aeter.Ratio.Serialization.Manual
{
    public class DictionaryGraphTraveller<TDictionary, TKey, TValue> : IGraphTraveller<TDictionary>, IDictionaryGraphTraveller
        where TDictionary : IDictionary<TKey, TValue>
    {
        private readonly IGraphTraveller<TKey> _keyTraveller;
        private readonly IGraphTraveller<TValue> _valueTraveller;
        private readonly SerializationInstanceFactory _instanceFactory;
        private readonly IValueVisitor<TKey> _keyVisitor;
        private readonly IValueVisitor<TValue> _valueVisitor;
        private readonly Type _keyType;
        private readonly Type _valueType;

        public DictionaryGraphTraveller(IGraphTraveller<TKey> keyTraveller, IGraphTraveller<TValue> valueTraveller, SerializationInstanceFactory instanceFactory)
        {
            _keyTraveller = keyTraveller;
            _valueTraveller = valueTraveller;
            _instanceFactory = instanceFactory;
            if (keyTraveller == null) {
                _keyVisitor = ValueVisitor.Create<TKey>();
            }
            else {
                _keyType = typeof(TKey);
            }
            if (valueTraveller == null) {
                _valueVisitor = ValueVisitor.Create<TValue>();
            }
            else {
                _valueType = typeof(TValue);
            }
        }

        void IGraphTraveller.Travel(IReadVisitor visitor, object graph)
        {
            Travel(visitor, (TDictionary)graph);
        }

        void IGraphTraveller.Travel(IWriteVisitor visitor, object graph)
        {
            Travel(visitor, (TDictionary)graph);
        }

        public void Travel(IReadVisitor visitor, TDictionary graph)
        {
            var valueArgs = VisitArgs.DictionaryValue;
            TValue TravelValue()
            {
                TValue value;
                if (_valueVisitor != null) {
                    if (!_valueVisitor.TryVisitValue(visitor, valueArgs, out value)) {
                        throw new InvalidGraphException("There were no corresponding value to the dictionary key.");
                    }
                    return value;
                }

                if (visitor.TryVisit(valueArgs) != ValueState.Found) {
                    throw new InvalidGraphException("There were no corresponding value to the dictionary key.");
                }
                value = (TValue)_instanceFactory.CreateInstance(_valueType);
                _valueTraveller.Travel(visitor, value);
                visitor.Leave(valueArgs);
                return value;
            }
            var keyArgs = VisitArgs.DictionaryKey;
            if (_keyVisitor != null) {
                while (_keyVisitor.TryVisitValue(visitor, keyArgs, out var key)) {
                    var value = TravelValue();
                    graph.Add(key, value);
                }
                return;
            }
            while (visitor.TryVisit(keyArgs) == ValueState.Found) {
                var key = (TKey)_instanceFactory.CreateInstance(_keyType);
                _keyTraveller.Travel(visitor, key);
                visitor.Leave(keyArgs);

                var value = TravelValue();
                graph.Add(key, value);
            }
        }

        public void Travel(IWriteVisitor visitor, TDictionary graph)
        {
            var valueArgs = VisitArgs.DictionaryValue;
            void TravelValue(TValue value)
            {
                if (_valueVisitor != null) {
                    _valueVisitor.VisitValue(visitor, valueArgs, value);
                    return;
                }

                visitor.Visit(value, valueArgs);
                _valueTraveller.Travel(visitor, value);
                visitor.Leave(value, valueArgs);
            }

            var keyArgs = VisitArgs.DictionaryKey;
            foreach (var kv in graph) {
                if (_keyVisitor != null) {
                    _keyVisitor.VisitValue(visitor, keyArgs, kv.Key);
                    TravelValue(kv.Value);
                }
                else {
                    var key = kv.Key;
                    visitor.Visit(key, keyArgs);
                    _keyTraveller.Travel(visitor, key);
                    visitor.Leave(key, keyArgs);
                    TravelValue(kv.Value);
                }
            }
        }
    }
}