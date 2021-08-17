/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Reflection
{
    public class CollectionContainerTypeInfo : IContainerTypeInfo
    {
        private readonly Lazy<Type> _collectionInterfaceType;
        public Type ElementType { get; }

        public CollectionContainerTypeInfo(Type elementType)
        {
            ElementType = elementType;
            _collectionInterfaceType = new Lazy<Type>(() => TypeExtensions.CollectionType.MakeGenericType(elementType));
        }

        public Type CollectionInterfaceType => _collectionInterfaceType.Value;
    }
}