/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Aeter.Ratio.Serialization.Bson
{
    public class BsonMinKey : IBsonNode
    {
        public static BsonMinKey Instance { get; } = new BsonMinKey();

        private BsonMinKey()
        {

        }

        public bool IsNull => false;

        public override string ToString()
        {
            return "MinKey";
        }
    }
}
