/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Aeter.Ratio.Binary;
using Aeter.Ratio.IO;

namespace Aeter.Ratio.Serialization.Json
{
    public class JsonWriteVisitor : IWriteVisitor
    {
        private readonly JsonEncoding _encoding;
        private readonly IFieldNameResolver _fieldNameResolver;
        private readonly BinaryWriteBuffer _writeBuffer;
        private readonly Stack<bool> _isFirsts;

        public JsonWriteVisitor(JsonEncoding encoding,
            IFieldNameResolver fieldNameResolver,
            BinaryWriteBuffer writeBuffer)
            : this(encoding, fieldNameResolver, writeBuffer, new Stack<bool>())
        {
        }

        /// <summary>
        /// Used by unit tests to simulate the stack already been set.
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="fieldNameResolver"></param>
        /// <param name="writeBuffer"></param>
        /// <param name="stack"></param>
        public JsonWriteVisitor(JsonEncoding encoding,
            IFieldNameResolver fieldNameResolver,
            BinaryWriteBuffer writeBuffer,
            Stack<bool> stack)
        {
            _encoding = encoding;
            _fieldNameResolver = fieldNameResolver;
            _writeBuffer = writeBuffer;
            _isFirsts = stack;
        }

        private void WriteFieldName(VisitArgs args)
        {
            WriteEscaped(_fieldNameResolver.Resolve(args));
        }

        private void Write(string value)
        {
            var bytes = _encoding.BaseEncoding.GetBytes(value);
            _writeBuffer.Write(bytes, 0, bytes.Length);
        }

        private void WriteValuePrefix(VisitArgs args)
        {
            if (args.IsRoot) return;
            switch (args.Type) {
                case LevelType.CollectionItem:
                case LevelType.DictionaryKey:
                case LevelType.Value:
                    EnsureComma();
                    break;
            }
            if (args.Type == LevelType.Value) {
                WriteFieldName(args);
                _writeBuffer.Write(_encoding.Assignment);
            }
        }

        private void WriteValueSuffix(VisitArgs args)
        {
            switch (args.Type) {
                case LevelType.DictionaryKey:
                    _writeBuffer.Write(_encoding.Assignment);
                    return;
            }
        }

        private void WriteValueDefault<T>(T? value, VisitArgs args)
            where T : struct, IConvertible
        {
            WriteValuePrefix(args);
            if (value == null) {
                _writeBuffer.Write(_encoding.Null);
            }
            else {
                if (args.Type == LevelType.DictionaryKey) {
                    _writeBuffer.Write(_encoding.Quote);
                    Write(value.Value.ToString(JsonEncoding.NumberFormat));
                    _writeBuffer.Write(_encoding.Quote);
                }
                else {
                    Write(value.Value.ToString(JsonEncoding.NumberFormat));
                }
            }
            WriteValueSuffix(args);
        }

        private void EnsureComma()
        {
            if (_isFirsts.Peek()) {
                _isFirsts.Pop();
                _isFirsts.Push(false);
            }
            else {
                _writeBuffer.Write(_encoding.Comma);
            }
        }

        private void WriteEscaped(string? value)
        {
            if (value == null) {
                _writeBuffer.Write(_encoding.Null);
                return;
            }
            _writeBuffer.Write(_encoding.Quote);
            if (value.Length == 0) {
                _writeBuffer.Write(_encoding.Quote);
                return;
            }
            var index = 0;
            var position = 0;
            for (var i = 0; i < value.Length; i++) {
                if (!_encoding.RequiresEscape(value[i], out var charBytes)) {
                    continue;
                }
                if (position == 0) {
                    var maxSize = _encoding.BaseEncoding.GetMaxByteCount(value.Length) + value.Length;
                    _writeBuffer.RequestSpace(maxSize);
                    position = _writeBuffer.Position;
                }
                if (index < i - 1) {
                    position += _encoding.BaseEncoding.GetBytes(value, index, i - index, _writeBuffer.Buffer, position);
                }
                index = i + 1;
                Buffer.BlockCopy(_encoding.ReverseSolidus, 0, _writeBuffer.Buffer, position,
                    _encoding.ReverseSolidus.Length);
                position += _encoding.ReverseSolidus.Length;
                Buffer.BlockCopy(charBytes, 0, _writeBuffer.Buffer, position, charBytes.Length);
                position += charBytes.Length;
            }
            if (index == 0) {
                Write(value);
                _writeBuffer.Write(_encoding.Quote);
                return;
            }

            if (index < value.Length) {
                position += _encoding.BaseEncoding.GetBytes(value, index, value.Length, _writeBuffer.Buffer, position);
            }

            _writeBuffer.Advance(position - _writeBuffer.Position);
            _writeBuffer.Write(_encoding.Quote);
        }

        private void Visit(object? level, VisitArgs args, byte[] beginToken)
        {
            if (!args.IsRoot) {
                WriteFieldName(args);
                _writeBuffer.Write(_encoding.Assignment);
            }
            _writeBuffer.Write(level == null ? _encoding.Null : beginToken);
        }

        public void Visit(object? level, VisitArgs args)
        {
            if (!args.IsRoot && !args.Type.IsDictionaryValue()) {
                EnsureComma();
            }
            switch (args.Type) {
                case LevelType.CollectionInDictionaryKey:
                    throw new NotSupportedException("Collections are not supported in dictionary keys");
                case LevelType.DictionaryInDictionaryKey:
                    throw new NotSupportedException("Dictionaries are not supported in dictionary keys");
                case LevelType.DictionaryKey:
                    throw new NotSupportedException("Objects are not supported in dictionary keys");
                case LevelType.Single:
                case LevelType.Dictionary:
                    Visit(level, args, _encoding.ObjectBegin);
                    break;
                case LevelType.Collection:
                    Visit(level, args, _encoding.ArrayBegin);
                    break;
                case LevelType.CollectionInCollection:
                case LevelType.CollectionInDictionaryValue:
                    _writeBuffer.Write(level == null ? _encoding.Null : _encoding.ArrayBegin);
                    break;
                default:
                    _writeBuffer.Write(level == null ? _encoding.Null : _encoding.ObjectBegin);
                    break;
            }
            _isFirsts.Push(true);
        }

        public void Leave(object? level, VisitArgs args)
        {
            _isFirsts.Pop();

            if (level == null) {
                return;
            }

            switch (args.Type) {
                case LevelType.Collection:
                case LevelType.CollectionInCollection:
                case LevelType.CollectionInDictionaryKey:
                case LevelType.CollectionInDictionaryValue: {
                    _writeBuffer.Write(_encoding.ArrayEnd);

                    return;
                }
            }
            _writeBuffer.Write(_encoding.ObjectEnd);
            if (args.Type.IsDictionaryKey()) {
                _writeBuffer.Write(_encoding.Assignment);
            }
        }

        public void VisitValue(byte? value, VisitArgs args)
        {
            WriteValueDefault(value, args);
        }

        public void VisitValue(short? value, VisitArgs args)
        {
            WriteValueDefault(value, args);
        }

        public void VisitValue(int? value, VisitArgs args)
        {
            WriteValueDefault(value, args);
        }

        public void VisitValue(long? value, VisitArgs args)
        {
            WriteValueDefault(value, args);
        }

        public void VisitValue(ushort? value, VisitArgs args)
        {
            WriteValueDefault(value, args);
        }

        public void VisitValue(uint? value, VisitArgs args)
        {
            WriteValueDefault(value, args);
        }

        public void VisitValue(ulong? value, VisitArgs args)
        {
            WriteValueDefault(value, args);
        }

        public void VisitValue(bool? value, VisitArgs args)
        {
            if (args.Type == LevelType.DictionaryKey) {
                throw new NotSupportedException("A boolean is not supported as dictionary key.");
            }

            WriteValuePrefix(args);
            if (value == null) {
                _writeBuffer.Write(_encoding.Null);
            }
            else {
                _writeBuffer.Write(value.Value ? _encoding.True : _encoding.False);
            }
            WriteValueSuffix(args);
        }

        public void VisitValue(float? value, VisitArgs args)
        {
            WriteValueDefault(value, args);
        }

        public void VisitValue(double? value, VisitArgs args)
        {
            WriteValueDefault(value, args);
        }

        public void VisitValue(decimal? value, VisitArgs args)
        {
            WriteValueDefault(value, args);
        }

        public void VisitValue(TimeSpan? value, VisitArgs args)
        {
            WriteValuePrefix(args);
            if (value == null) {
                _writeBuffer.Write(_encoding.Null);
            }
            else {
                _writeBuffer.Write(_encoding.Quote);
                Write(value.Value.ToString());
                _writeBuffer.Write(_encoding.Quote);
            }
            WriteValueSuffix(args);
        }

        public void VisitValue(DateTime? value, VisitArgs args)
        {
            WriteValuePrefix(args);
            if (value == null) {
                _writeBuffer.Write(_encoding.Null);
            }
            else {
                _writeBuffer.Write(_encoding.Quote);
                Write(value.Value.ToString("O", JsonEncoding.NumberFormat));
                _writeBuffer.Write(_encoding.Quote);
            }
            WriteValueSuffix(args);
        }

        public void VisitValue(string? value, VisitArgs args)
        {
            WriteValuePrefix(args);
            WriteEscaped(value);
            WriteValueSuffix(args);
        }

        public void VisitValue(Guid? value, VisitArgs args)
        {
            WriteValuePrefix(args);
            if (value == null) {
                _writeBuffer.Write(_encoding.Null);
            }
            else {
                _writeBuffer.Write(_encoding.Quote);
                Write(value.Value.ToString());
                _writeBuffer.Write(_encoding.Quote);
            }
            WriteValueSuffix(args);
        }

        public void VisitValue(byte[]? value, VisitArgs args)
        {
            if (args.Type == LevelType.DictionaryKey) {
                throw new NotSupportedException("A blob is not supported as dictionary key.");
            }
            WriteValuePrefix(args);

            if (value == null) {
                _writeBuffer.Write(_encoding.Null);
            }
            else {
                _writeBuffer.Write(_encoding.Quote);
                var size = _encoding.Base64.GetEncodedSizeOf(value);
                var offset = _writeBuffer.Advance(size);
                _encoding.Base64.Encode(value, 0, value.Length, _writeBuffer.Buffer, offset);
                _writeBuffer.Write(_encoding.Quote);
            }

            WriteValueSuffix(args);
        }
    }
}