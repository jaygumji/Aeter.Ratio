/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Aeter.Ratio.Serialization.Bson
{
    public enum BsonBinarySubtypeCode : byte
    {
        Generic = 0x00,
        Function = 0x01,
        UUID = 0x04,
        MD5 = 0x05,
        Encrypted = 0x06,
        Compressed = 0x07,
        UserDefined = 0x80
    }

}