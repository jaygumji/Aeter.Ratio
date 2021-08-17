/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections.Generic;

namespace Aeter.Ratio.Serialization.Reflection.Graph
{
    public class ComplexGraphType : IGraphType
    {
        private readonly IEnumerable<IGraphProperty> _properties;

        public ComplexGraphType(IEnumerable<IGraphProperty> properties)
        {
            _properties = properties;
        }

        public IEnumerable<IGraphProperty> Properties
        {
            get { return _properties; }
        }

        public void Visit(object graph, IReadVisitor visitor)
        {
            foreach (var property in _properties)
                property.Visit(graph, visitor);
        }

        public void Visit(object graph, IWriteVisitor visitor)
        {
            foreach (var property in _properties)
                property.Visit(graph, visitor);
        }
    }
}