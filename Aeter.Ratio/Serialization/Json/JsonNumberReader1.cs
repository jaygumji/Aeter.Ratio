/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;

namespace Aeter.Ratio.Serialization.Json
{
    public class JsonNumberReader1 : JsonNumberReader
    {
        private const int Size = 1;

        public JsonNumberReader1(BinaryReadBuffer buffer, JsonEncoding encoding) : base(buffer, encoding)
        {
        }

        public override bool ReadNext(ref byte next)
        {
            var buffer = Buffer.Span;
            var pos = Buffer.Position;
            var b1 = buffer[pos];

            if (b1 == Encoding.One[0]) {
                next = 1;
                Buffer.Advance(Size);
                return true;
            }
            if (b1 == Encoding.Two[0]) {
                next = 2;
                Buffer.Advance(Size);
                return true;
            }
            if (b1 == Encoding.Three[0]) {
                next = 3;
                Buffer.Advance(Size);
                return true;
            }
            if (b1 == Encoding.Four[0]) {
                next = 4;
                Buffer.Advance(Size);
                return true;
            }
            if (b1 == Encoding.Five[0]) {
                next = 5;
                Buffer.Advance(Size);
                return true;
            }
            if (b1 == Encoding.Six[0]) {
                next = 6;
                Buffer.Advance(Size);
                return true;
            }
            if (b1 == Encoding.Seven[0]) {
                next = 7;
                Buffer.Advance(Size);
                return true;
            }
            if (b1 == Encoding.Eight[0]) {
                next = 8;
                Buffer.Advance(Size);
                return true;
            }
            if (b1 == Encoding.Nine[0]) {
                next = 9;
                Buffer.Advance(Size);
                return true;
            }
            if (b1 == Encoding.Zero[0]) {
                next = 0;
                Buffer.Advance(Size);
                return true;
            }
            if (b1 == Encoding.Point[0]) {
                next = Decimal;
                Buffer.Advance(Size);
                return true;
            }
            if (b1 == Encoding.Minus[0]) {
                next = Negative;
                Buffer.Advance(Size);
                return true;
            }
            return false;
        }

    }
}