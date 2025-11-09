/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

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

        private byte MapSymbol(ReadOnlySpan<byte> source, ref int index)
        {
            var first = source[index++];
            var second = source[index++];
            return _map[first][second];
        }

        public void MapTo(ReadOnlySpan<byte> source, ref int sourceIndex, Span<byte> target, ref int targetIndex)
        {
            var b1 = MapSymbol(source, ref sourceIndex);
            var b2 = MapSymbol(source, ref sourceIndex);

            target[targetIndex++] = (byte)((b1 << 2) | ((b2 & 0x30) >> 4));
            b1 = MapSymbol(source, ref sourceIndex);
            target[targetIndex++] = (byte)(((b1 & 0x3C) >> 2) | ((b2 & 0x0F) << 4));
            b2 = MapSymbol(source, ref sourceIndex);
            target[targetIndex++] = (byte)(((b1 & 0x03) << 6) | b2);
        }

        public void MapLast(ReadOnlySpan<byte> source, ref int sourceIndex, Span<byte> target, ref int targetIndex, ref int padding)
        {
            var b1 = MapSymbol(source, ref sourceIndex);
            var b2 = MapSymbol(source, ref sourceIndex);

            target[targetIndex++] = (byte)((b1 << 2) | ((b2 & 0x30) >> 4));

            b1 = MapSymbol(source, ref sourceIndex);

            if (padding != 2) {
                target[targetIndex++] = (byte)(((b1 & 0x3C) >> 2) | ((b2 & 0x0F) << 4));
            }

            b2 = MapSymbol(source, ref sourceIndex);
            if (padding == 0) {
                target[targetIndex++] = (byte)(((b1 & 0x03) << 6) | b2);
            }
        }

    }
}
