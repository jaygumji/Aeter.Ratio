/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using Aeter.Ratio.Binary;

namespace Aeter.Ratio.Serialization.Json
{
    public abstract class JsonNumberReader
    {
        protected BinaryReadBuffer Buffer { get; }
        protected JsonEncoding Encoding { get; }

        public const int Negative = 255;
        public const int Decimal = 254;

        protected JsonNumberReader(BinaryReadBuffer buffer, JsonEncoding encoding)
        {
            Buffer = buffer;
            Encoding = encoding;
        }

        public static JsonNumberReader Create(BinaryReadBuffer buffer, JsonEncoding encoding)
        {
            if (encoding.Zero.Length == 4) {
                return new JsonNumberReader4(buffer, encoding);
            }
            if (encoding.Zero.Length == 2) {
                return new JsonNumberReader2(buffer, encoding);
            }
            if (encoding.Zero.Length == 1) {
                return new JsonNumberReader1(buffer, encoding);
            }
            throw new NotSupportedException($"Does not support encodings with numbers encoded with {encoding.Zero.Length} bytes");
        }

        public abstract bool ReadNext(ref byte next);
    }
}