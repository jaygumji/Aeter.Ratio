/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.Serialization.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Aeter.Ratio.Serialization.Bson
{
    public class BsonReader
    {
        private readonly BinaryReadBuffer _buffer;
        private readonly BsonEncoding _encoding;

        public BsonReader(Stream stream)
            : this(stream, BsonEncoding.UTF8)
        {
        }

        public BsonReader(Stream stream, BsonEncoding encoding)
            : this(new BinaryReadBuffer(1024, stream), encoding)
        {
        }

        public BsonReader(BinaryReadBuffer buffer, BsonEncoding encoding)
        {
            _buffer = buffer;
            _encoding = encoding;
        }

        public int ReadInt32()
        {
            _buffer.RequestSpace(4);
            var value = BinaryInformation.Int32.Converter.Convert(_buffer.Buffer, _buffer.Position);
            _buffer.Advance(4);
            return value;
        }

        public BsonTypeCode ReadType()
        {
            var value = _buffer.ReadByte();
            return (BsonTypeCode)value;
        }

        private bool TryReadCString(int offset, int space, [MaybeNullWhen(false)] out string value, [MaybeNullWhen(false)] out int size)
        {
            _buffer.RequestSpace(space);
            for (var i = _buffer.Position + offset; i < _buffer.Length; i++) {
                if (_buffer.Buffer[i] == BsonEncoding.ZeroTermination) {
                    size = i - _buffer.Position + 1;
                    value = _encoding.BaseEncoding.GetString(_buffer.Buffer, _buffer.Position, size - 1);
                    _buffer.Advance(size);
                    return true;
                }
            }
            value = default;
            size = 0;
            return false;
        }

        public bool TryReadCString([MaybeNullWhen(false)] out string value, [MaybeNullWhen(false)] out int size)
        {
            if (TryReadCString(0, 128, out value, out size)) return true;
            if (TryReadCString(128, 256, out value, out size)) return true;
            if (TryReadCString(256, 512, out value, out size)) return true;
            if (TryReadCString(512, 1024, out value, out size)) return true;
            return false;
        }

        public string ReadLString(out int size)
        {
            size = ReadInt32();
            return ReadLString(size);
        }

        public string ReadLString(int size)
        {
            _buffer.RequestSpace(size);
            var value = _encoding.BaseEncoding.GetString(_buffer.Buffer, _buffer.Position, size-1);
            _buffer.Advance(size);
            return value;
        }

        private IBsonNode ReadFixedValue<T>(IBinaryInformation<T> info, Func<T, IBsonNode> factory, out int binarySize)
        {
            _buffer.RequestSpace(info.FixedLength);
            var raw = info.Converter.Convert(_buffer.Buffer, _buffer.Position);
            binarySize = info.FixedLength;
            return factory(raw);
        }

        public IBsonNode ReadValue(bool deep, out int binarySize)
        {
            var type = ReadType();
            var value = ReadValue(type, deep, out binarySize);
            binarySize++; // Add type
            return value;
        }

        public IBsonNode ReadValue(BsonTypeCode type, bool deep, out int binarySize)
        {
            switch (type) {
                case BsonTypeCode.Double:
                    return ReadFixedValue(BinaryInformation.Double, x => new BsonDouble(x), out binarySize);
                case BsonTypeCode.String:
                case BsonTypeCode.JavaScriptCode:
                    var str = ReadLString(out binarySize);
                    binarySize += 4; // Include the length field
                    return new BsonString(str);
                case BsonTypeCode.Regex:
                    if (!TryReadCString(out var regexPattern, out var regexPatternSize)) throw UnexpectedBsonException.From("regex pattern", _buffer, _encoding);
                    if (!TryReadCString(out var regexOptions, out var regexOptionsSize)) throw UnexpectedBsonException.From("regex options", _buffer, _encoding);
                    binarySize = regexPatternSize + regexOptionsSize + 8; // Include the length field 4 + 4
                    return new BsonRegex(regexPattern, regexOptions);
                case BsonTypeCode.Document:
                    binarySize = ReadInt32();
                    var doc = new BsonDocument(binarySize);
                    if (deep) {
                        var parsedSize = 0;
                        ReadDocument(doc, ref parsedSize);
                    }
                    binarySize += 4; // Include the length field
                    return doc;
                case BsonTypeCode.Array:
                    binarySize = ReadInt32();
                    var arr = new BsonArray(binarySize);
                    if (deep) {
                        var parsedSize = 0;
                        ReadArray(arr, in binarySize, ref parsedSize);
                    }
                    binarySize += 4; // Include the length field
                    return arr;
                case BsonTypeCode.Binary:
                    binarySize = ReadInt32();
                    var subtype = (BsonBinarySubtypeCode) _buffer.ReadByte();
                    var blob = new byte[binarySize];
                    _buffer.CopyTo(blob);
                    binarySize += 5; // Include the subtype and length field
                    return new BsonBinary(blob, subtype);
                case BsonTypeCode.ObjectId:
                    binarySize = 12;
                    var objectId = new byte[binarySize];
                    _buffer.CopyTo(objectId);
                    return new BsonObjectId(objectId);
                case BsonTypeCode.Boolean:
                    if (!BsonBoolean.TryGetValue(_buffer.ReadByte(), out var value)) {
                        throw UnexpectedBsonException.From("boolean", _buffer, _encoding);
                    }
                    binarySize = 1;
                    return value;
                case BsonTypeCode.DateTime:
                    binarySize = 8;
                    _buffer.RequestSpace(binarySize);
                    var ms = BinaryInformation.Int64.Converter.Convert(_buffer.Buffer, _buffer.Position);
                    _buffer.Advance(binarySize);
                    var dt = DateTime.UnixEpoch.AddMilliseconds(ms);
                    return new BsonDateTime(dt);
                case BsonTypeCode.Null:
                    binarySize = 0;
                    return BsonNull.Instance;
                case BsonTypeCode.Int32:
                    return ReadFixedValue(BinaryInformation.Int32, x => new BsonInt32(x), out binarySize);
                case BsonTypeCode.UInt64:
                    return ReadFixedValue(BinaryInformation.UInt64, x => new BsonTimestamp(x), out binarySize);
                case BsonTypeCode.Int64:
                    return ReadFixedValue(BinaryInformation.Int64, x => new BsonInt64(x), out binarySize);
                case BsonTypeCode.Decimal128:
                    return ReadFixedValue(BinaryInformation.Decimal, x => new BsonDecimal128(x), out binarySize);
                case BsonTypeCode.MinKey:
                    binarySize = 0;
                    return BsonMinKey.Instance;
                case BsonTypeCode.MaxKey:
                    binarySize = 0;
                    return BsonMaxKey.Instance;
            }
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        public IBsonNode? ReadArray(BsonArray array, in int size, ref int parsedSize, int indexToFind = -1)
        {
            while (parsedSize < size) {
                if (!TryReadCString(out var fieldName, out var fieldNameSize)) throw UnexpectedBsonException.From("field name", _buffer, _encoding);
                parsedSize += fieldNameSize;
                if (!int.TryParse(fieldName, out var index)) throw UnexpectedBsonException.From("array index", _buffer, _encoding);
                if (index == indexToFind) {
                    var value = ReadValue(deep: false, out var parsedValueSize);
                    parsedSize += parsedValueSize;
                    return value;
                }
                else {
                    var value = ReadValue(deep: true, out var parsedValueSize);
                    parsedSize += parsedValueSize;
                    array.Add(value);
                }
            }
            return default;
        }

        public IBsonNode ReadArray(BsonArray array, ref int parsedSize, bool deep = true)
        {
            var size = array.Size;
            while (parsedSize < size) {
                if (!TryReadCString(out var fieldName, out var fieldNameSize)) throw UnexpectedBsonException.From("field name", _buffer, _encoding);
                parsedSize += fieldNameSize;

                var value = ReadValue(deep: deep, out var parsedValueSize);
                parsedSize += parsedValueSize;
                array.Add(value);
                if (!deep) {
                    return value;
                }
            }
            return BsonUndefined.Instance;
        }

        public IBsonNode ReadDocument(BsonDocument document, ref int parsedSize, string? nameToFind = null)
        {
            var size = document.Size;
            while (parsedSize < size) {
                if (!TryReadCString(out var fieldName, out var fieldNameSize)) throw UnexpectedBsonException.From("field name", _buffer, _encoding);
                parsedSize += fieldNameSize;
                var isMatchedName = string.Equals(fieldName, nameToFind, StringComparison.Ordinal);
                var value = ReadValue(deep: !isMatchedName, out var parsedValueSize);
                parsedSize += parsedValueSize;
                document.Add(fieldName, value);

                if (isMatchedName) {
                    return value;
                }
            }
            return BsonUndefined.Instance;
        }
    }
}
