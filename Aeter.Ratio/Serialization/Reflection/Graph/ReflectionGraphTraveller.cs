/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Serialization.Reflection.Graph
{
    public class ReflectionGraphTraveller<T> : IGraphTraveller<T>
    {
        private readonly IGraphType _type;

        public ReflectionGraphTraveller(IGraphType type)
        {
            _type = type;
        }

        public void Travel(IWriteVisitor visitor, object graph)
        {
            _type.Visit(graph, visitor);
        }

        public void Travel(IReadVisitor visitor, object graph)
        {
            _type.Visit(graph, visitor);
        }

        public void Travel(IWriteVisitor visitor, T graph)
        {
            Travel(visitor, (object)graph);
        }

        public void Travel(IReadVisitor visitor, T graph)
        {
            Travel(visitor, (object)graph);
        }
    }
}
