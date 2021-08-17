/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Binary.Converters
{
    public sealed class Base64BufferDecoder
    {

        private readonly byte[] _paddingChar;
        private readonly int _charSize;
        private readonly IBase64CharacterMap _map;

        public Base64BufferDecoder(Base64EncodedCharacterSet charSet)
        {
            _paddingChar = charSet.PaddingChar;
            _charSize = charSet.CharSize;
            _map = Base64CharacterMap.Create(charSet);
        }

        public int GetSizeOf(byte[] value)
        {
            return GetSizeOf(value, 0, value.Length);
        }

        public int GetSizeOf(byte[] value, int offset, int count)
        {
            unsafe {
                fixed (byte* vp = value) {
                    var v = vp + offset;
                    SizeOf(v, count, out var targetSize, out var padding);
                    return targetSize;
                }
            }
        }

        private unsafe void SizeOf(byte* v, int count, out int targetSize, out int padding)
        {
            var charCount = count / _charSize;
            var blockCount = (charCount - 1) / 4 + 1;
            targetSize = blockCount * 3;
            padding = blockCount * 4 - charCount;

            if (_charSize == 1) {
                if (count > 2 && v[count - 2] == _paddingChar[0]) {
                    padding = 2;
                }
                else if (count > 1 && v[count - 1] == _paddingChar[0]) {
                    padding = 1;
                }
            }
            else {
                if (count > 2 && IsEqual(v + count - (2 * _charSize), _paddingChar)) {
                    padding = 2;
                }
                else if (count > 1 && IsEqual(v + count - _charSize, _paddingChar)) {
                    padding = 1;
                }
            }

            targetSize -= padding;
        }

        private unsafe bool IsEqual(byte* left, byte[] right)
        {
            for (var i = 0; i < _charSize; i++) {
                if (left[i] != right[i]) {
                    return false;
                }
            }
            return true;
        }

        public void Decode(byte[] source, int sourceOffset, int sourceCount, byte[] target, int targetOffset)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (sourceCount == 0) {
                return;
            }

            if (sourceOffset < 0 || sourceOffset >= source.Length) {
                throw new ArgumentException("The source offset is not within the bounds of the array.");
            }

            var charCount = sourceCount / _charSize;
            var blockCount = (charCount - 1) / 4 + 1;

            var targetLength = target.Length;

            unsafe {
                fixed (byte* startOfSource = source) {
                    var s = startOfSource + sourceOffset;

                    SizeOf(s, sourceCount, out var numberOfBytes, out var padding);
                    if (numberOfBytes > targetLength - targetOffset) {
                        throw new ArgumentException("The base64 encoding does not fit into the target array.");
                    }

                    if (_charSize == 1) {
                        if (sourceCount > 2 && s[sourceCount - 2] == _paddingChar[0]) {
                            padding = 2;
                        }
                        else if (sourceCount > 1 && s[sourceCount - 1] == _paddingChar[0]) {
                            padding = 1;
                        }
                    }
                    else {
                        if (sourceCount > 2 && IsEqual(s + sourceCount - (2 * _charSize), _paddingChar)) {
                            padding = 2;
                        }
                        else if (sourceCount > 1 && IsEqual(s + sourceCount - _charSize, _paddingChar)) {
                            padding = 1;
                        }
                    }

                    fixed (byte* startOfTarget = target) {
                        var t = startOfTarget + targetOffset;

                        for (var i = 1; i < blockCount; i++) {
                            _map.MapTo(ref s, ref t);
                        }

                        _map.MapLast(ref s, ref t, ref padding);
                    }
                }
            }
        }

    }
}