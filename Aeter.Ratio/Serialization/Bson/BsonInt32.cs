/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Aeter.Ratio.Serialization.Bson
{
    public struct BsonInt32 : IBsonNode
    {
        public int? Value { get; }

        public BsonInt32(int? value)
        {
            Value = value;
        }

        public bool IsNull => !Value.HasValue;

        public override string ToString()
        {
            return Value.HasValue ? Value.Value.ToString() : "null";
        }
    }
    public struct BsonBoolean : IBsonNode
    {
        public static BsonBoolean True { get; } = new BsonBoolean(true);
        public static BsonBoolean False { get; } = new BsonBoolean(false);

        public bool? Value { get; }

        public BsonBoolean(bool? value)
        {
            Value = value;
        }

        public bool IsNull => !Value.HasValue;

        public override string ToString()
        {
            return Value.HasValue ? Value.Value.ToString() : "null";
        }

        public static bool TryGetValue(byte value, out BsonBoolean node)
        {
            switch (value) {
                case 0:
                    node = False;
                    return true;
                case 1:
                    node = True;
                    return true;
                default:
                    node = False;
                    return false;
            }
        }
    }
}
