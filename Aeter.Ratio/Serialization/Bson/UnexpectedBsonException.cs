/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using Aeter.Ratio.Binary;

namespace Aeter.Ratio.Serialization.Bson
{
    public class UnexpectedBsonException : Exception
    {

        public UnexpectedBsonException(string message)
            : base(message)
        {
        }

        public static UnexpectedBsonException From(string expected, BinaryReadBuffer buffer, BsonEncoding encoding) => new("Unexpected token in bson. Expected " + expected);
        public static UnexpectedBsonException Type(string? name, IBsonNode node, Type expectedType) => new($"Unable to parse field {name}, expected {expectedType.Name}, but found {node.GetType().Name}");
        public static UnexpectedBsonException ValueWouldBeTruncated<T>(string? name, T value, Type targetType) => new($"Unable to deserialize field {name}, value '{value}' would be truncated if converted to {targetType.Name}");
        public static UnexpectedBsonException Validation(string message) => new(message);
        public static UnexpectedBsonException MissingTerminator(string typeName) => new($"Missing 0x00 terminator of {typeName}");
    }
}