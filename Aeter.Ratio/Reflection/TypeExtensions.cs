/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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
                return new ArrayContainerTypeInfo(elementType!, ranks);
            }

            if (type.IsGenericType) {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition == DictionaryType) {
                    var arguments = type.GetGenericArguments();
                    return new DictionaryContainerTypeInfo(arguments[0], arguments[1]);
                }

                if (genericTypeDefinition == CollectionType)
                    return new CollectionContainerTypeInfo(type.GetGenericArguments()[0]);

                if (genericTypeDefinition == NullableType)
                    return new NullableContainerTypeInfo(type, type.GetGenericArguments()[0]);
            }

            var interfaceTypes = type.GetInterfaces();
            foreach (var interfaceType in interfaceTypes.Where(interfaceType => interfaceType.IsGenericType)) {
                var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();
                if (genericTypeDefinition == CollectionType)
                    return new CollectionContainerTypeInfo(interfaceType.GetGenericArguments()[0]);
                if (genericTypeDefinition == DictionaryType) {
                    var arguments = interfaceType.GetGenericArguments();
                    return new DictionaryContainerTypeInfo(arguments[0], arguments[1]);
                }
            }

            return LeafContainerTypeInfo.Instance;
        }

        public static Type AsNullable(this Type type)
        {
            return NullableType.MakeGenericType(type);
        }

        public static bool TryGetInterface(this Type type, Type interfaceType, [MaybeNullWhen(false)] out Type matchedInterfaceType)
        {
            var interfaces = type.GetInterfaces();
            var isGenericTypeDef = interfaceType.IsGenericTypeDefinition;
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

        public static TypeClassification GetClassification(this Type type, IContainerTypeInfo? containerInfo = null)
        {
            if (type.IsPrimitive) return TypeClassification.Value;
            if (type.IsEnum) return TypeClassification.Value;
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
            if (type.IsEnum) return Enum.GetUnderlyingType(type);

            if (type.IsGenericType) {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition == NullableType) {
                    var innerType = type.GenericTypeArguments[0];
                    if (innerType.IsEnum) {
                        var underlyingType = Enum.GetUnderlyingType(innerType);
                        return NullableType.MakeGenericType(underlyingType);
                    }
                }
            }

            throw new InvalidOperationException("The type is not an enum");
        }

    }
}
