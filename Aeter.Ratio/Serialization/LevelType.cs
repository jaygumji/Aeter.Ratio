/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Serialization
{
    [Flags]
    public enum LevelType
    {
        Value = 1,
        Single = 2,
        Collection = 4,
        CollectionItem = 8,
        Dictionary = 16,
        DictionaryKey = 32,
        DictionaryValue = 64,

        CollectionInCollection = Collection | CollectionItem,
        DictionaryInCollection = Dictionary | CollectionItem,
        DictionaryInDictionaryKey = Dictionary | DictionaryKey,
        DictionaryInDictionaryValue = Dictionary | DictionaryValue,
        CollectionInDictionaryKey = Collection | DictionaryKey,
        CollectionInDictionaryValue = Collection | DictionaryValue
    }

    public static class LevelTypeExtensions
    {

        public static bool IsCollection(this LevelType type)
        {
            return (type & LevelType.Collection) == LevelType.Collection;
        }

        public static bool IsCollectionItem(this LevelType type)
        {
            return (type & LevelType.CollectionItem) == LevelType.CollectionItem;
        }

        public static bool IsDictionary(this LevelType type)
        {
            return (type & LevelType.Dictionary) == LevelType.Dictionary;
        }

        public static bool IsDictionaryKey(this LevelType type)
        {
            return (type & LevelType.DictionaryKey) == LevelType.DictionaryKey;
        }

        public static bool IsDictionaryValue(this LevelType type)
        {
            return (type & LevelType.DictionaryValue) == LevelType.DictionaryValue;
        }
    }
}