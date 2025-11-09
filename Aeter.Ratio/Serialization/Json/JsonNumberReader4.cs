/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Runtime.CompilerServices;
using Aeter.Ratio.Binary;

namespace Aeter.Ratio.Serialization.Json
{
    public sealed class JsonNumberReader4 : JsonNumberReader
    {
        private const int Size = 4;

        public JsonNumberReader4(BinaryReadBuffer buffer, JsonEncoding encoding) : base(buffer, encoding)
        {
        }

        public override bool ReadNext(ref byte next)
        {
            var buffer = Buffer.Span;
            var pos = Buffer.Position;
            var b1 = buffer[pos];
            var b2 = buffer[pos+1];
            var b3 = buffer[pos+2];
            var b4 = buffer[pos+3];

            if (Equal(ref b1, ref b2, ref b3, ref b4, Encoding.One)) {
                next = 1;
                Buffer.Advance(Size);
                return true;
            }
            if (Equal(ref b1, ref b2, ref b3, ref b4, Encoding.Two)) {
                next = 2;
                Buffer.Advance(Size);
                return true;
            }
            if (Equal(ref b1, ref b2, ref b3, ref b4, Encoding.Three)) {
                next = 3;
                Buffer.Advance(Size);
                return true;
            }
            if (Equal(ref b1, ref b2, ref b3, ref b4, Encoding.Four)) {
                next = 4;
                Buffer.Advance(Size);
                return true;
            }
            if (Equal(ref b1, ref b2, ref b3, ref b4, Encoding.Five)) {
                next = 5;
                Buffer.Advance(Size);
                return true;
            }
            if (Equal(ref b1, ref b2, ref b3, ref b4, Encoding.Six)) {
                next = 6;
                Buffer.Advance(Size);
                return true;
            }
            if (Equal(ref b1, ref b2, ref b3, ref b4, Encoding.Seven)) {
                next = 7;
                Buffer.Advance(Size);
                return true;
            }
            if (Equal(ref b1, ref b2, ref b3, ref b4, Encoding.Eight)) {
                next = 8;
                Buffer.Advance(Size);
                return true;
            }
            if (Equal(ref b1, ref b2, ref b3, ref b4, Encoding.Nine)) {
                next = 9;
                Buffer.Advance(Size);
                return true;
            }
            if (Equal(ref b1, ref b2, ref b3, ref b4, Encoding.Zero)) {
                next = 0;
                Buffer.Advance(Size);
                return true;
            }
            if (Equal(ref b1, ref b2, ref b3, ref b4, Encoding.Point)) {
                next = Decimal;
                Buffer.Advance(Size);
                return true;
            }
            if (Equal(ref b1, ref b2, ref b3, ref b4, Encoding.Minus)) {
                next = Negative;
                Buffer.Advance(Size);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Equal(ref byte b1, ref byte b2, ref byte b3, ref byte b4, byte[] n)
        {
            return n[0] == b1 && n[1] == b2 && n[2] == b3 && n[3] == b4;
        }
    }
}