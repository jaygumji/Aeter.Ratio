/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Aeter.Ratio.Serialization.Bson
{
    // cstring = (byte*) "\x00" - UTF8 zero terminated binary data without length
    // string = int32 (byte*) "\x00" - UTF8 zero terminated binary data with length of binary data including the zero termination character
    // binary = int32 subtype (byte*) - Binary data with length of binary data
    public enum BsonTypeCode : byte
    {
        Double = 0x01, // 64 bit binary floating point
        String = 0x02, // See string comment
        Document = 0x03, // Embedded Document
        Array = 0x04,
        Binary = 0x05,
        ObjectId = 0x07,
        Boolean = 0x08, // Followed by 0x00 (False), 0x01 (True)
        DateTime = 0x09, // UTC
        Null = 0x0A,
        Regex = 0x0B, // cstring (pattern) followed by cstring (options)
        JavaScriptCode = 0x0D,
        Int32 = 0x10,
        UInt64 = 0x11, // Timestamp
        Int64 = 0x12,
        Decimal128 = 0x13,
        MinKey = 0xFF,
        MaxKey = 0x7F
    }
    public static class BsonBooleanExtensions
    {
        private const byte True = 0x01;
        private const byte False = 0x00;
        public static byte ToBsonByte(this bool value)
        {
            return value ? True : False;
        }
    }
}