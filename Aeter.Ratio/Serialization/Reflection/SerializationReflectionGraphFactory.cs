/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using Aeter.Ratio.Reflection;

namespace Aeter.Ratio.Serialization.Reflection
{
    public class SerializationReflectionGraphFactory : IReflectionGraphFactory
    {
        private readonly SerializationReflectionGraphCollection _graphs;

        public SerializationReflectionGraphFactory(SerializationReflectionGraphCollection graphs)
        {
            _graphs = graphs;
        }

        /// <summary>
        /// Visits the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        public void Visit(Type type)
        {
            // System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

        }

        public bool TryCreatePropertyVisitor(Type type, out IReflectionGraphPropertyVisitor visitor)
        {
            bool wasAdded;
            var graph = _graphs.GetOrAdd(type, out wasAdded);
            visitor = new SerializationReflectionGraphPropertyVisitor(graph);
            return true;
        }


    }
}
