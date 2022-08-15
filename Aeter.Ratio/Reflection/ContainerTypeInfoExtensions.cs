/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Diagnostics.CodeAnalysis;

namespace Aeter.Ratio.Reflection
{
    public static class ContainerTypeInfoExtensions
    {
        public static bool AsArray(this IContainerTypeInfo container, [MaybeNullWhen(false)] out ArrayContainerTypeInfo array)
        {
            array = container as ArrayContainerTypeInfo;
            return array != null;
        }

        public static ArrayContainerTypeInfo? AsArray(this IContainerTypeInfo container)
        {
            return container as ArrayContainerTypeInfo;
        }

        public static bool AsCollection(this IContainerTypeInfo container, [MaybeNullWhen(false)] out CollectionContainerTypeInfo collection)
        {
            collection = container as CollectionContainerTypeInfo;
            return collection != null;
        }

        public static CollectionContainerTypeInfo? AsCollection(this IContainerTypeInfo container)
        {
            return container as CollectionContainerTypeInfo;
        }

        public static bool AsDictionary(this IContainerTypeInfo container, [MaybeNullWhen(false)] out DictionaryContainerTypeInfo dictionary)
        {
            dictionary = container as DictionaryContainerTypeInfo;
            return dictionary != null;
        }

        public static DictionaryContainerTypeInfo? AsDictionary(this IContainerTypeInfo container)
        {
            return container as DictionaryContainerTypeInfo;
        }

        public static bool AsNullable(this IContainerTypeInfo container, [MaybeNullWhen(false)] out NullableContainerTypeInfo nullable)
        {
            nullable = container as NullableContainerTypeInfo;
            return nullable != null;
        }

        public static NullableContainerTypeInfo? AsNullable(this IContainerTypeInfo container)
        {
            return container as NullableContainerTypeInfo;
        }

    }
}