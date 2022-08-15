/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections;
using System.Collections.Generic;

namespace Aeter.Ratio.Serialization.Bson
{
    public class BsonArray : IBsonNode, IEnumerable<IBsonNode>
    {
        private readonly List<IBsonNode> _inner;

        public int Size { get; }

        public BsonArray(int size)
        {
            _inner = new List<IBsonNode>();
            Size = size;
        }

        bool IBsonNode.IsNull => false;

        public int Count => _inner.Count;

        public void Add(IBsonNode item)
        {
            _inner.Add(item);
        }

        public bool Remove(IBsonNode item)
        {
            return _inner.Remove(item);
        }

        public IBsonNode this[int index] => _inner[index];

        public IEnumerator<IBsonNode> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
