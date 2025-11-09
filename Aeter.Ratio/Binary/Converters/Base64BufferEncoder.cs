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

            if (sourceOffset < 0 || sourceOffset >= source.Length) {
                throw new ArgumentException("The source offset is not within the bounds of the array.");
            }
            if (sourceCount < 0 || sourceOffset + sourceCount > source.Length) {
                throw new ArgumentException("The source count is not within the bounds of the array.");
            }
            if (targetOffset < 0 || targetOffset > target.Length) {
                throw new ArgumentException("The target offset is not within the bounds of the array.");
            }

            var sourceSpan = source.AsSpan(sourceOffset, sourceCount);
            var targetSpan = target.AsSpan(targetOffset);
            Encode(sourceSpan, targetSpan);
        }

        public void Encode(ReadOnlySpan<byte> source, Span<byte> target)
        {
            if (source.Length == 0) {
                return;
            }

            var sourceCount = source.Length;

            var padding = sourceCount % 3;

            if (padding > 0) {
                padding = 3 - padding;
            }
            var blockCount = (sourceCount - 1) / 3 + 1;
            var numberOfChars = blockCount * 4;

            if (numberOfChars * _charSize > target.Length) {
                throw new ArgumentException("The base64 encoding does not fit into the target span.");
            }

            var sourceSpan = source;
            var targetSpan = target;
            var chars = _chars.AsSpan();
            var targetIndex = 0;
            var sourceIndex = 0;

            if (_charSize == 1) {
                byte b1, b2, b3;
                for (var i = 1; i < blockCount; i++) {
                    b1 = sourceSpan[sourceIndex++];
                    b2 = sourceSpan[sourceIndex++];
                    b3 = sourceSpan[sourceIndex++];

                    targetSpan[targetIndex++] = chars[(b1 & 0xFC) >> 2];
                    targetSpan[targetIndex++] = chars[(b2 & 0xF0) >> 4 | (b1 & 0x03) << 4];
                    targetSpan[targetIndex++] = chars[(b3 & 0xC0) >> 6 | (b2 & 0x0F) << 2];
                    targetSpan[targetIndex++] = chars[b3 & 0x3F];
                }

                var usePadding2 = padding == 2;
                var usePadding1 = padding > 0;

                b1 = sourceSpan[sourceIndex++];
                b2 = usePadding2 ? (byte)0 : sourceSpan[sourceIndex++];
                b3 = usePadding1 ? (byte)0 : sourceSpan[sourceIndex++];

                targetSpan[targetIndex++] = chars[(b1 & 0xFC) >> 2];
                targetSpan[targetIndex++] = chars[(b2 & 0xF0) >> 4 | (b1 & 0x03) << 4];
                targetSpan[targetIndex++] = usePadding2 ? _paddingChar[0] : chars[(b3 & 0xC0) >> 6 | (b2 & 0x0F) << 2];
                targetSpan[targetIndex++] = usePadding1 ? _paddingChar[0] : chars[b3 & 0x3F];
            }
            else {
                static void WriteChar(Span<byte> targetSpan, ReadOnlySpan<byte> chars, ref int targetIndex, int charIndex, int charSize)
                {
                    var offset = charIndex * charSize;
                    for (var ci = 0; ci < charSize; ci++) {
                        targetSpan[targetIndex++] = chars[offset + ci];
                    }
                }

                static void WritePadding(Span<byte> targetSpan, ReadOnlySpan<byte> paddingChars, ref int targetIndex, int charSize)
                {
                    for (var ci = 0; ci < charSize; ci++) {
                        targetSpan[targetIndex++] = paddingChars[ci];
                    }
                }

                byte b1, b2, b3;
                for (var i = 1; i < blockCount; i++) {
                    b1 = sourceSpan[sourceIndex++];
                    b2 = sourceSpan[sourceIndex++];
                    b3 = sourceSpan[sourceIndex++];

                    WriteChar(targetSpan, chars, ref targetIndex, (b1 & 0xFC) >> 2, _charSize);
                    WriteChar(targetSpan, chars, ref targetIndex, (b2 & 0xF0) >> 4 | (b1 & 0x03) << 4, _charSize);
                    WriteChar(targetSpan, chars, ref targetIndex, (b3 & 0xC0) >> 6 | (b2 & 0x0F) << 2, _charSize);
                    WriteChar(targetSpan, chars, ref targetIndex, b3 & 0x3F, _charSize);
                }

                var usePadding2 = padding == 2;
                var usePadding1 = padding > 0;

                b1 = sourceSpan[sourceIndex++];
                b2 = usePadding2 ? (byte)0 : sourceSpan[sourceIndex++];
                b3 = usePadding1 ? (byte)0 : sourceSpan[sourceIndex++];

                WriteChar(targetSpan, chars, ref targetIndex, (b1 & 0xFC) >> 2, _charSize);
                WriteChar(targetSpan, chars, ref targetIndex, (b2 & 0xF0) >> 4 | (b1 & 0x03) << 4, _charSize);
                if (usePadding2) {
                    WritePadding(targetSpan, _paddingChar.AsSpan(), ref targetIndex, _charSize);
                }
                else {
                    WriteChar(targetSpan, chars, ref targetIndex, (b3 & 0xC0) >> 6 | (b2 & 0x0F) << 2, _charSize);
                }

                if (usePadding1) {
                    WritePadding(targetSpan, _paddingChar.AsSpan(), ref targetIndex, _charSize);
                }
                else {
                    WriteChar(targetSpan, chars, ref targetIndex, b3 & 0x3F, _charSize);
                }
            }
        }

    }
}
