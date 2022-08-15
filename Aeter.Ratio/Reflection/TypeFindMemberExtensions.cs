/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;

namespace Aeter.Ratio.Reflection
{
    public static class TypeFindMemberExtensions
    {
        public static ConstructorInfo MakeGenericAndGetConstructor(this Type type, params Type[] parameterTypes)
        {
            var actualType = type.MakeGenericType(parameterTypes);
            return actualType.GetConstructor(parameterTypes)
                ?? throw MissingMemberException.MissingConstructor(actualType, parameterTypes);
        }

        public static ConstructorInfo FindConstructor(this Type type)
        {
            return FindConstructor(type, Type.EmptyTypes);
        }

        public static ConstructorInfo FindConstructor(this Type type, params Type[] parameterTypes)
        {
            return type.GetConstructor(parameterTypes)
                ?? throw MissingMemberException.MissingConstructor(type, parameterTypes);
        }

        public static MethodInfo FindMethod(this Type type, string methodName)
        {
            return type.GetMethod(methodName)
                ?? throw MissingMemberException.MissingMethod(type, methodName);
        }

        public static MethodInfo FindMethod(this Type type, string methodName, params Type[] parameterTypes)
        {
            return type.GetMethod(methodName, parameterTypes)
                ?? throw MissingMemberException.MissingMethod(type, methodName);
        }

        public static FieldInfo FindField(this Type type, string fieldName)
        {
            return type.GetField(fieldName)
                ?? throw MissingMemberException.MissingField(type, fieldName);
        }

        public static PropertyInfo FindProperty(this Type type, string propertyName)
        {
            return type.GetProperty(propertyName)
                ?? throw MissingMemberException.MissingProperty(type, propertyName);
        }
    }
}
