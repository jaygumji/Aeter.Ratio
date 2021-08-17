/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Serialization.Reflection.Graph
{
    public class Int16GraphProperty : IGraphProperty
    {

        private readonly SerializableProperty _property;
        private readonly VisitArgs _args;

        public Int16GraphProperty(SerializableProperty property, VisitArgs args)
        {
            _property = property;
            _args = args;
        }

        public void Visit(object graph, IReadVisitor visitor)
        {
            Int16? value;
            if (visitor.TryVisitValue(_args, out value) && value.HasValue)
                _property.Ref.SetValue(graph, value.Value);
        }

        public void Visit(object graph, IWriteVisitor visitor)
        {
            var value = (Int16) _property.Ref.GetValue(graph);
            visitor.VisitValue(value, _args);
        }
    }
}