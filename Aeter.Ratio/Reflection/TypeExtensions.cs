/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aeter.Ratio.Reflection
{
    public static class TypeExtensions
    {
        private static readonly IList<Type> SystemValueClasses = new[] {
            typeof (DateTime), typeof (string), typeof (TimeSpan),
            typeof(Guid), typeof(decimal), typeof(byte[])
        };

        public static readonly Type CollectionType = typeof (ICollection<>);
        public static readonly Type DictionaryType = typeof (IDictionary<,>);
        public static readonly Type NullableType = typeof (Nullable<>);

        public static IContainerTypeInfo GetContainerTypeInfo(this Type type)
        {
            if (type.IsArray) {
                var ranks = type.GetArrayRank();
                var elementType = type.GetElementType();
                return new ArrayContainerTypeInfo(elementType, ranks);
            }

            var typeInfo = type.GetTypeInfo();
            if (type.GetTypeInfo().IsGenericType) {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition == DictionaryType) {
                    var arguments = typeInfo.GetGenericArguments();
                    return new DictionaryContainerTypeInfo(arguments[0], arguments[1]);
                }

                if (genericTypeDefinition == CollectionType)
                    return new CollectionContainerTypeInfo(typeInfo.GetGenericArguments()[0]);

                if (genericTypeDefinition == NullableType)
                    return new NullableContainerTypeInfo(type, typeInfo.GetGenericArguments()[0]);
            }

            var interfaceTypes = typeInfo.GetInterfaces();
            foreach (var interfaceType in interfaceTypes.Where(interfaceType => interfaceType.GetTypeInfo().IsGenericType)) {
                var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();
                var interfaceTypeInfo = interfaceType.GetTypeInfo();
                if (genericTypeDefinition == CollectionType)
                    return new CollectionContainerTypeInfo(interfaceTypeInfo.GetGenericArguments()[0]);
                if (genericTypeDefinition == DictionaryType) {
                    var arguments = interfaceTypeInfo.GetGenericArguments();
                    return new DictionaryContainerTypeInfo(arguments[0], arguments[1]);
                }
            }

            return null;
        }

        public static Type AsNullable(this Type type)
        {
            return NullableType.MakeGenericType(type);
        }

        public static bool TryGetInterface(this Type type, Type interfaceType, out Type matchedInterfaceType)
        {
            var interfaces = type.GetTypeInfo().GetInterfaces();
            var interfaceTypeInfo = interfaceType.GetTypeInfo();
            var isGenericTypeDef = interfaceTypeInfo.IsGenericTypeDefinition;
            foreach (var itInterface in interfaces) {
                if (isGenericTypeDef) {
                    if (itInterface.GetGenericTypeDefinition() == interfaceType) {
                        matchedInterfaceType = itInterface;
                        return true;
                    }
                }
                else {
                    if (itInterface == interfaceType) {
                        matchedInterfaceType = itInterface;
                        return true;
                    }
                }
            }

            matchedInterfaceType = null;
            return false;
        }

        public static TypeClassification GetClassification(this Type type, IContainerTypeInfo containerInfo = null)
        {
            var ti = type.GetTypeInfo();
            if (ti.IsPrimitive) return TypeClassification.Value;
            if (ti.IsEnum) return TypeClassification.Value;
            if (SystemValueClasses.Contains(type)) return TypeClassification.Value;

            if (containerInfo == null) {
                containerInfo = type.GetContainerTypeInfo();
            }

            if (containerInfo is DictionaryContainerTypeInfo) {
                return TypeClassification.Dictionary;
            }
            if (containerInfo is CollectionContainerTypeInfo) {
                return TypeClassification.Collection;
            }
            if (containerInfo is NullableContainerTypeInfo) {
                return TypeClassification.Nullable;
            }

            return TypeClassification.Complex;
        }

        public static Type GetUnderlyingEnumType(this Type type)
        {
            var info = type.GetTypeInfo();
            if (info.IsEnum) return Enum.GetUnderlyingType(type);

            if (info.IsGenericType) {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition == NullableType) {
                    var innerType = type.GenericTypeArguments[0];
                    if (innerType.GetTypeInfo().IsEnum) {
                        var underlyingType = Enum.GetUnderlyingType(innerType);
                        return NullableType.MakeGenericType(underlyingType);
                    }
                }
            }

            throw new InvalidOperationException("The type is not an enum");
        }

    }
}