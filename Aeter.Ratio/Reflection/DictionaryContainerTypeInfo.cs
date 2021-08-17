/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;

namespace Aeter.Ratio.Reflection
{
    public class DictionaryContainerTypeInfo : CollectionContainerTypeInfo
    {
        private static readonly Type KeyValuePairType = typeof(KeyValuePair<,>);
        private readonly Lazy<Type> _dictionaryInterfaceType;

        public DictionaryContainerTypeInfo(Type keyType, Type valueType)
            : base(KeyValuePairType.MakeGenericType(keyType, valueType))
        {
            KeyType = keyType;
            ValueType = valueType;

            _dictionaryInterfaceType = new Lazy<Type>(() => TypeExtensions.DictionaryType.MakeGenericType(keyType, valueType));
        }

        public Type KeyType { get; }
        public Type ValueType { get; }
        public Type DictionaryInterfaceType => _dictionaryInterfaceType.Value;
    }
}