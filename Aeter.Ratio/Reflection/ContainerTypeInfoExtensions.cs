/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Reflection
{
    public static class ContainerTypeInfoExtensions
    {

        public static ArrayContainerTypeInfo AsArray(this IContainerTypeInfo container)
        {
            return container as ArrayContainerTypeInfo;
        }

        public static CollectionContainerTypeInfo AsCollection(this IContainerTypeInfo container)
        {
            return container as CollectionContainerTypeInfo;
        }

        public static DictionaryContainerTypeInfo AsDictionary(this IContainerTypeInfo container)
        {
            return container as DictionaryContainerTypeInfo;
        }

        public static NullableContainerTypeInfo AsNullable(this IContainerTypeInfo container)
        {
            return container as NullableContainerTypeInfo;
        }

    }
}