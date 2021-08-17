/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Binary.Converters
{
    public class Base64TwoCharacterMap : IBase64CharacterMap
    {
        private readonly byte[][] _map;

        public Base64TwoCharacterMap(Base64EncodedCharacterSet charSet)
        {
            const byte maxSize = 123;
            _map = new byte[maxSize][];
            var chars = charSet.Chars;
            byte v = 0;
            for (var i = 0; i < chars.Length; i+=2) {
                var xi = chars[i];
                var x = _map[xi];
                if (x == null) {
                    x = new byte[maxSize];
                    _map[xi] = x;
                }
                x[chars[i+1]] = v++;
            }
        }

        public unsafe void MapTo(ref byte* s, ref byte* t)
        {
            var b1 = _map[*s++][*s++];
            var b2 = _map[*s++][*s++];

            *t++ = (byte)((b1 << 2) | ((b2 & 0x30) >> 4));
            b1 = _map[*s++][*s++];
            *t++ = (byte)(((b1 & 0x3C) >> 2) | ((b2 & 0x0F) << 4));
            b2 = _map[*s++][*s++];
            *t++ = (byte)(((b1 & 0x03) << 6) | b2);
        }

        public unsafe void MapLast(ref byte* s, ref byte* t, ref int padding)
        {
            var b1 = _map[*s++][*s++];
            var b2 = _map[*s++][*s++];

            *t++ = (byte)((b1 << 2) | ((b2 & 0x30) >> 4));

            b1 = _map[*s++][*s++];

            if (padding != 2) {
                *t++ = (byte)(((b1 & 0x3C) >> 2) | ((b2 & 0x0F) << 4));
            }

            b2 = _map[*s++][*s++];
            if (padding == 0) {
                *t++ = (byte)(((b1 & 0x03) << 6) | b2);
            }
        }

    }
}