/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Binary.Converters
{
    public sealed class Base64BufferEncoder
    {

        private readonly byte[] _chars;
        private readonly byte[] _paddingChar;
        private readonly int _charSize;

        public Base64BufferEncoder(Base64EncodedCharacterSet charSet)
        {
            _chars = charSet.Chars;
            _paddingChar = charSet.PaddingChar;
            _charSize = charSet.CharSize;
        }

        public int GetSizeOf(int count)
        {
            var blockCount = (count - 1) / 3 + 1;
            var numberOfChars = blockCount * 4;

            return numberOfChars * _charSize;
        }

        public void Encode(byte[] source, int sourceOffset, int sourceCount, byte[] target, int targetOffset)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (sourceCount == 0) {
                return;
            }

            var targetLength = target.Length;

            if (sourceOffset < 0 || sourceOffset >= source.Length) {
                throw new ArgumentException("The source offset is not within the bounds of the array.");
            }

            var padding = sourceCount % 3;

            if (padding > 0) {
                padding = 3 - padding;
            }
            var blockCount = (sourceCount - 1) / 3 + 1;
            var numberOfChars = blockCount * 4;

            if (numberOfChars * _charSize > targetLength - targetOffset) {
                throw new ArgumentException("The base64 encoding does not fit into the target array.");
            }

            unsafe {
                fixed (byte* startOfSource = source) {
                    fixed (byte* startOfChars = _chars) {
                        var s = startOfSource + sourceOffset;

                        fixed (byte* startOfTarget = target) {
                            var t = startOfTarget + targetOffset;

                            byte b1, b2, b3;
                            for (var i = 1; i < blockCount; i++) {
                                b1 = *s++;
                                b2 = *s++;
                                b3 = *s++;

                                if (_charSize == 1) {
                                    *t++ = startOfChars[(b1 & 0xFC) >> 2];
                                    *t++ = startOfChars[(b2 & 0xF0) >> 4 | (b1 & 0x03) << 4];
                                    *t++ = startOfChars[(b3 & 0xC0) >> 6 | (b2 & 0x0F) << 2];
                                    *t++ = startOfChars[b3 & 0x3F];
                                }
                                else {
                                    var c = ((b1 & 0xFC) >> 2) * _charSize;
                                    for (var ci = 0; ci < _charSize; ci++) {
                                        *t++ = startOfChars[c + ci];
                                    }
                                    c = ((b2 & 0xF0) >> 4 | (b1 & 0x03) << 4) * _charSize;
                                    for (var ci = 0; ci < _charSize; ci++) {
                                        *t++ = startOfChars[c + ci];
                                    }
                                    c = ((b3 & 0xC0) >> 6 | (b2 & 0x0F) << 2) * _charSize;
                                    for (var ci = 0; ci < _charSize; ci++) {
                                        *t++ = startOfChars[c + ci];
                                    }
                                    c = (b3 & 0x3F) * _charSize;
                                    for (var ci = 0; ci < _charSize; ci++) {
                                        *t++ = startOfChars[c + ci];
                                    }
                                }
                            }

                            var usePadding2 = padding == 2;
                            var usePadding1 = padding > 0;

                            b1 = *s++;
                            b2 = usePadding2 ? (byte)0 : *s++;
                            b3 = usePadding1 ? (byte)0 : *s++;

                            if (_charSize == 1) {
                                *t++ = startOfChars[(b1 & 0xFC) >> 2];
                                *t++ = startOfChars[(b2 & 0xF0) >> 4 | (b1 & 0x03) << 4];
                                *t++ = usePadding2 ? _paddingChar[0] : startOfChars[(b3 & 0xC0) >> 6 | (b2 & 0x0F) << 2];
                                *t++ = usePadding1 ? _paddingChar[0] : startOfChars[b3 & 0x3F];
                            }
                            else {
                                var c = ((b1 & 0xFC) >> 2) * _charSize;
                                for (var ci = 0; ci < _charSize; ci++) {
                                    *t++ = startOfChars[c + ci];
                                }
                                c = ((b2 & 0xF0) >> 4 | (b1 & 0x03) << 4) * _charSize;
                                for (var ci = 0; ci < _charSize; ci++) {
                                    *t++ = startOfChars[c + ci];
                                }
                                if (usePadding2) {
                                    for (var ci = 0; ci < _charSize; ci++) {
                                        *t++ = _paddingChar[ci];
                                    }
                                }
                                else {
                                    c = ((b3 & 0xC0) >> 6 | (b2 & 0x0F) << 2) * _charSize;
                                    for (var ci = 0; ci < _charSize; ci++) {
                                        *t++ = startOfChars[c + ci];
                                    }
                                }
                                if (usePadding1) {
                                    for (var ci = 0; ci < _charSize; ci++) {
                                        *t++ = _paddingChar[ci];
                                    }
                                }
                                else {
                                    c = (b3 & 0x3F) * _charSize;
                                    for (var ci = 0; ci < _charSize; ci++) {
                                        *t++ = startOfChars[c + ci];
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}