/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Aeter.Ratio.Serialization.Bson
{
    public class BsonDocument : IBsonNode, IEnumerable<KeyValuePair<string, IBsonNode>>
    {
        private readonly Dictionary<string, IBsonNode> _fields;

        public int Size { get; }

        public BsonDocument(int size)
        {
            _fields = new Dictionary<string, IBsonNode>(StringComparer.Ordinal);
            Size = size;
        }

        bool IBsonNode.IsNull => false;

        public void Add(string fieldName, IBsonNode field)
        {
            _fields.Add(fieldName, field);
        }

        public bool Remove(string fieldName)
        {
            return _fields.Remove(fieldName);
        }

        public bool TryGet(string fieldName, [MaybeNullWhen(false)] out IBsonNode field)
        {
            return _fields.TryGetValue(fieldName, out field);
        }

        public IBsonNode this[string fieldName] => _fields[fieldName];

        public IEnumerator<KeyValuePair<string, IBsonNode>> GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
