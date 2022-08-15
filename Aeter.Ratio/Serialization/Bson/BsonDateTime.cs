/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Aeter.Ratio.Serialization.Bson
{
    public struct BsonDateTime : IBsonNode
    {
        public System.DateTime? Value { get; }

        public BsonDateTime(System.DateTime? value)
        {
            Value = value;
        }

        public bool IsNull => !Value.HasValue;

        public override string ToString()
        {
            return Value.HasValue ? Value.Value.ToString() : "null";
        }
    }
}
