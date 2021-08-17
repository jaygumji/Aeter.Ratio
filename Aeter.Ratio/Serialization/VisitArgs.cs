/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Serialization.Reflection;

namespace Aeter.Ratio.Serialization
{
    public class VisitArgs
    {
        public static readonly VisitArgs CollectionItem = new VisitArgs(LevelType.CollectionItem);
        public static readonly VisitArgs DictionaryKey = new VisitArgs(LevelType.DictionaryKey);
        public static readonly VisitArgs DictionaryValue = new VisitArgs(LevelType.DictionaryValue);
        public static readonly VisitArgs CollectionInCollection = new VisitArgs(LevelType.CollectionInCollection);
        public static readonly VisitArgs DictionaryInCollection = new VisitArgs(LevelType.DictionaryInCollection);
        public static readonly VisitArgs DictionaryInDictionaryKey = new VisitArgs(LevelType.DictionaryInDictionaryKey);
        public static readonly VisitArgs DictionaryInDictionaryValue = new VisitArgs(LevelType.DictionaryInDictionaryValue);
        public static readonly VisitArgs CollectionInDictionaryKey = new VisitArgs(LevelType.CollectionInDictionaryKey);
        public static readonly VisitArgs CollectionInDictionaryValue = new VisitArgs(LevelType.CollectionInDictionaryValue);

        private VisitArgs(LevelType type)
            : this(null, type, 0, EnigmaSerializationAttributes.Empty, null, isRoot: false)
        {
        }

        public VisitArgs(string name, LevelType type)
            : this(name, type, 0, EnigmaSerializationAttributes.Empty, null, isRoot: false)
        {
        }

        public VisitArgs(string name, LevelType type, uint index, EnigmaSerializationAttributes attributes, object state)
            : this(name, type, index, attributes, state, isRoot: false)
        {
        }

        private VisitArgs(string name, LevelType type, uint index, EnigmaSerializationAttributes attributes, object state, bool isRoot)
        {
            Name = name;
            Type = type;
            Index = index;
            Attributes = attributes;
            State = state;
            IsRoot = isRoot;
        }

        public string Name { get; }
        public LevelType Type { get; }
        public uint Index { get; }
        public bool IsRoot { get; }
        public EnigmaSerializationAttributes Attributes { get; }
        public object State { get; }

        public override string ToString()
        {
            return string.Concat(Type, " args ", Name, " with index ", Index);
        }

        public static VisitArgs CreateRoot(LevelType type, object state = null)
        {
            return new VisitArgs(null, type, 1, EnigmaSerializationAttributes.Empty, state, isRoot: true);
        }

    }
}