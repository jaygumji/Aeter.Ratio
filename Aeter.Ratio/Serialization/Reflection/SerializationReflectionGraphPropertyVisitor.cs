/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Reflection;
using Aeter.Ratio.Reflection;

namespace Aeter.Ratio.Serialization.Reflection
{
    public class SerializationReflectionGraphPropertyVisitor : IReflectionGraphPropertyVisitor
    {
        private readonly SerializationReflectionGraph _graph;

        public SerializationReflectionGraphPropertyVisitor(SerializationReflectionGraph graph)
        {
            _graph = graph;
        }

        public void Visit(PropertyInfo property)
        {
            if (!property.CanRead) return;
            if (!property.CanWrite) return;
            if (property.GetGetMethod().IsStatic) return;

            
        }

    }
}