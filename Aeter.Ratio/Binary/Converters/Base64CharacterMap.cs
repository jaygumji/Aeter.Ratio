/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Binary.Converters
{
    public class Base64CharacterMap : IBase64CharacterMap
    {
        private readonly byte[] _map;

        public Base64CharacterMap(Base64EncodedCharacterSet charSet)
        {
            _map = new byte[123];
            var chars = charSet.Chars;
            for (byte i = 0; i < chars.Length; i++) {
                _map[chars[i]] = i;
            }
        }

        public unsafe void MapTo(ref byte* s, ref byte* t)
        {
            var b1 = _map[*s++];
            var b2 = _map[*s++];

            *t++ = (byte)((b1 << 2) | ((b2 & 0x30) >> 4));
            b1 = _map[*s++];
            *t++ = (byte)(((b1 & 0x3C) >> 2) | ((b2 & 0x0F) << 4));
            b2 = _map[*s++];
            *t++ = (byte)(((b1 & 0x03) << 6) | b2);
        }

        public unsafe void MapLast(ref byte* s, ref byte* t, ref int padding)
        {
            var b1 = _map[*s++];
            var b2 = _map[*s++];

            *t++ = (byte)((b1 << 2) | ((b2 & 0x30) >> 4));

            b1 = _map[*s++];

            if (padding != 2) {
                *t++ = (byte)(((b1 & 0x3C) >> 2) | ((b2 & 0x0F) << 4));
            }

            b2 = _map[*s++];
            if (padding == 0) {
                *t++ = (byte)(((b1 & 0x03) << 6) | b2);
            }
        }

        public static IBase64CharacterMap Create(Base64EncodedCharacterSet charSet)
        {
            if (charSet.CharSize == 1) {
                return new Base64CharacterMap(charSet);
            }

            if (charSet.CharSize == 2) {
                return new Base64TwoCharacterMap(charSet);
            }

            if (charSet.CharSize == 4) {
                return new Base64FourCharacterMap(charSet);
            }

            throw new NotSupportedException($"Encodings with character size {charSet.CharSize} is currently not supported.");
        }
    }
}