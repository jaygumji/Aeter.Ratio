/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections;
using System.Collections.Generic;

namespace Aeter.Ratio.Serialization.Json
{
    public class JsonArray : IJsonNode, IList<IJsonNode>, IReadOnlyList<IJsonNode>
    {

        private readonly List<IJsonNode> _nodes;

        public JsonArray()
        {
            _nodes = new List<IJsonNode>();
        }

        public bool IsNull => false;

        public IEnumerator<IJsonNode> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IJsonNode node)
        {
            _nodes.Add(node);
        }

        public void Clear()
        {
            _nodes.Clear();
        }

        public bool Contains(IJsonNode node)
        {
            return _nodes.Contains(node);
        }

        void ICollection<IJsonNode>.CopyTo(IJsonNode[] array, int arrayIndex)
        {
            _nodes.CopyTo(array, arrayIndex);
        }

        public bool Remove(IJsonNode node)
        {
            return _nodes.Remove(node);
        }

        public int Count => _nodes.Count;
        bool ICollection<IJsonNode>.IsReadOnly => false;

        public int IndexOf(IJsonNode node)
        {
            return _nodes.IndexOf(node);
        }

        public void Insert(int index, IJsonNode node)
        {
            _nodes.Insert(index, node);
        }

        public void RemoveAt(int index)
        {
            _nodes.RemoveAt(index);
        }

        public IJsonNode this[int index]
        {
            get => _nodes[index];
            set => _nodes[index] = value;
        }
    }
}