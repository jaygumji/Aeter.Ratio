/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using Aeter.Ratio.Binary;

namespace Aeter.Ratio.Serialization.Json
{
    public class UnexpectedJsonException : Exception
    {

        public UnexpectedJsonException(string message)
            : base(message)
        {
        }

        public static UnexpectedJsonException From(string expected, BinaryReadBuffer buffer, JsonEncoding encoding)
        {
            return new UnexpectedJsonException("Unexpected token in json. Expected " + expected);
        }

        public static UnexpectedJsonException InObject(JsonLiteral literal)
        {
            return new UnexpectedJsonException("An object can not directly contain " + literal);
        }

        public static UnexpectedJsonException InArray(JsonLiteral literal)
        {
            return new UnexpectedJsonException("An array can not directly contain " + literal);
        }

        public static UnexpectedJsonException Type(string name, IJsonNode node, Type expectedType)
        {
            return new UnexpectedJsonException($"Unable to parse field {name}, expected {expectedType.Name}, but found {node.GetType().Name}");
        }
    }
}