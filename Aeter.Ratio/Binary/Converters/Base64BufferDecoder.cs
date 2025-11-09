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
            if (value == null) throw new ArgumentNullException(nameof(value));
            return GetSizeOf(value, 0, value.Length);
        }

        public int GetSizeOf(byte[] value, int offset, int count)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (offset < 0 || offset > value.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > value.Length) throw new ArgumentOutOfRangeException(nameof(count));

            var span = value.AsSpan(offset, count);
            SizeOf(span, out var targetSize, out _);
            return targetSize;
        }

        private void SizeOf(ReadOnlySpan<byte> span, out int targetSize, out int padding)
        {
            var count = span.Length;
            var charCount = count / _charSize;
            var blockCount = (charCount - 1) / 4 + 1;
            targetSize = blockCount * 3;
            padding = blockCount * 4 - charCount;

            if (_charSize == 1) {
                if (count > 2 && span[count - 2] == _paddingChar[0]) {
                    padding = 2;
                }
                else if (count > 1 && span[count - 1] == _paddingChar[0]) {
                    padding = 1;
                }
            }
            else {
                if (count > 2 && IsEqual(span.Slice(count - (2 * _charSize), _charSize), _paddingChar)) {
                    padding = 2;
                }
                else if (count > 1 && IsEqual(span.Slice(count - _charSize, _charSize), _paddingChar)) {
                    padding = 1;
                }
            }

            targetSize -= padding;
        }

        private bool IsEqual(ReadOnlySpan<byte> left, byte[] right)
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
            if (targetOffset < 0 || targetOffset > target.Length) {
                throw new ArgumentException("The target offset is not within the bounds of the array.");
            }

            if (sourceCount == 0) {
                return;
            }

            if (sourceOffset < 0 || sourceOffset >= source.Length) {
                throw new ArgumentException("The source offset is not within the bounds of the array.");
            }
            if (sourceCount < 0 || sourceOffset + sourceCount > source.Length) {
                throw new ArgumentException("The source count is not within the bounds of the array.");
            }

            var sourceSpan = source.AsSpan(sourceOffset, sourceCount);
            var targetSpan = target.AsSpan(targetOffset);
            Decode(sourceSpan, targetSpan);
        }

        public void Decode(ReadOnlySpan<byte> source, Span<byte> target)
        {
            if (source.Length == 0) {
                return;
            }

            var sourceCount = source.Length;
            var charCount = sourceCount / _charSize;
            var blockCount = (charCount - 1) / 4 + 1;

            SizeOf(source, out var numberOfBytes, out var padding);
            if (numberOfBytes > target.Length) {
                throw new ArgumentException("The base64 encoding does not fit into the target span.");
            }

            if (_charSize == 1) {
                if (sourceCount > 2 && source[sourceCount - 2] == _paddingChar[0]) {
                    padding = 2;
                }
                else if (sourceCount > 1 && source[sourceCount - 1] == _paddingChar[0]) {
                    padding = 1;
                }
            }
            else {
                if (sourceCount > 2 && IsEqual(source.Slice(sourceCount - (2 * _charSize), _charSize), _paddingChar)) {
                    padding = 2;
                }
                else if (sourceCount > 1 && IsEqual(source.Slice(sourceCount - _charSize, _charSize), _paddingChar)) {
                    padding = 1;
                }
            }

            var sourceIndex = 0;
            var targetIndex = 0;

            for (var i = 1; i < blockCount; i++) {
                _map.MapTo(source, ref sourceIndex, target, ref targetIndex);
            }

            _map.MapLast(source, ref sourceIndex, target, ref targetIndex, ref padding);
        }

    }
}
