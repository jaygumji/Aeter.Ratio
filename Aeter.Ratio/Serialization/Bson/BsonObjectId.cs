/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Serialization.Bson
{
    public struct BsonObjectId : IBsonNode
    {
        public byte[] Value { get; }

        public BsonObjectId(byte[] value)
        {
            Value = value;
        }

        public bool IsNull => Value == null;

        public override string ToString()
        {
            return "ObjectId";
        }
    }
}
