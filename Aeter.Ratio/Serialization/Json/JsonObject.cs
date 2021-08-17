/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections;
using System.Collections.Generic;

namespace Aeter.Ratio.Serialization.Json
{
    public class JsonObject : IJsonNode, IEnumerable<KeyValuePair<string, IJsonNode>>
    {

        private readonly Dictionary<string, IJsonNode> _fields;

        public JsonObject()
        {
            _fields = new Dictionary<string, IJsonNode>(StringComparer.Ordinal);
        }

        bool IJsonNode.IsNull => false;

        public void Add(string fieldName, IJsonNode field)
        {
            _fields.Add(fieldName, field);
        }

        public bool Remove(string fieldName)
        {
            return _fields.Remove(fieldName);
        }

        public bool TryGet(string fieldName, out IJsonNode field)
        {
            return _fields.TryGetValue(fieldName, out field);
        }

        public IJsonNode this[string fieldName] => _fields[fieldName];

        public IEnumerator<KeyValuePair<string, IJsonNode>> GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
