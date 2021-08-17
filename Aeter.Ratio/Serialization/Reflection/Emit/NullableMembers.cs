/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;

namespace Aeter.Ratio.Serialization.Reflection.Emit
{
    public sealed class NullableMembers
    {
        public static readonly Type NullableTypeDefinition = typeof(Nullable<>);

        public readonly Type NullableType;
        public readonly ConstructorInfo Constructor;
        public readonly MethodInfo GetHasValue;
        public readonly MethodInfo GetValue;

        public NullableMembers(Type elementType)
        {
            NullableType = NullableTypeDefinition.MakeGenericType(elementType);

            var typeInfo = NullableType.GetTypeInfo();
            Constructor = typeInfo.GetConstructor(new[] { elementType });
            GetHasValue = typeInfo.GetProperty("HasValue").GetGetMethod();
            GetValue = typeInfo.GetProperty("Value").GetGetMethod();
        }
    }
}