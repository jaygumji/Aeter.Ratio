/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;

namespace Aeter.Ratio.Reflection
{
    public class NullableContainerTypeInfo : IContainerTypeInfo
    {
        private readonly Lazy<ConstructorInfo> _constructor;
        private readonly Lazy<MethodInfo> _getHasValueMethod;
        public Type ElementType { get; }

        public NullableContainerTypeInfo(Type type, Type elementType)
        {
            ElementType = elementType;
            _constructor = new Lazy<ConstructorInfo>(() => type.FindConstructor(elementType));
            _getHasValueMethod = new Lazy<MethodInfo>(() => type.FindProperty("HasValue").GetMethod!);
        }

        public ConstructorInfo Constructor => _constructor.Value;
        public MethodInfo GetHasValueMethod => _getHasValueMethod.Value;
    }
}