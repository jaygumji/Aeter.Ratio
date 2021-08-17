/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Globalization;
using Aeter.Ratio.Binary;

namespace Aeter.Ratio.Serialization.Json
{
    public class JsonReadVisitor : IReadVisitor
    {
        private readonly JsonEncoding _encoding;
        private readonly IFieldNameResolver _fieldNameResolver;
        private readonly BinaryReadBuffer _buffer;
        private readonly JsonReader _reader;
        private readonly Stack<JsonReadLevel> _parents;

        private static readonly JsonReadLevel ArrayLevel = new JsonReadLevel(new JsonArray());
        private class JsonReadLevel
        {
            public readonly IJsonNode Node;
            public bool IsFullyParsed;

            public JsonReadLevel(IJsonNode node)
            {
                Node = node;
            }
        }

        public JsonReadVisitor(JsonEncoding encoding, IFieldNameResolver fieldNameResolver, BinaryReadBuffer buffer)
        {
            _encoding = encoding;
            _fieldNameResolver = fieldNameResolver;
            _buffer = buffer;
            _reader = new JsonReader(buffer, _encoding);
            _parents = new Stack<JsonReadLevel>();
        }

        private IJsonNode ParseUntilFound(VisitArgs args)
        {
            var name = string.IsNullOrEmpty(args.Name)
                ? null
                : _fieldNameResolver.Resolve(args);

            JsonLiteral literal;
            if (args.IsRoot) {
                // When the value being deserialized is a simple value only
                literal = _reader.ReadLiteral();
                return _reader.ReadValue(literal);
            }

            var parent = _parents.Peek();
            if (args.Type.IsDictionaryKey()) {
                if (!_reader.ReadFieldName(out var key)) {
                    parent.IsFullyParsed = true;
                    return JsonUndefined.Instance;
                }
                return new JsonString(key);
            }
            if (args.Type.IsDictionaryValue()) {
                literal = _reader.ReadLiteral();
                if (literal != JsonLiteral.Assignment) {
                    throw UnexpectedJsonException.From("dictionary assignment token", _buffer, _encoding);
                }
                literal = _reader.ReadLiteral();
                if (literal == JsonLiteral.ObjectBegin) {
                    var child = new JsonObject();
                    _parents.Push(new JsonReadLevel(child));
                    return child;
                }
                if (literal == JsonLiteral.ArrayBegin) {
                    _parents.Push(ArrayLevel);
                    return ArrayLevel.Node;
                }
                return _reader.ReadValue(literal);
            }

            if (parent.Node is JsonObject obj) {
                if (obj.TryGet(name, out var field)) {
                    if (field is JsonObject || field is JsonArray) {
                        _parents.Push(new JsonReadLevel(field));
                    }
                    return field;
                }

                if (parent.IsFullyParsed) {
                    return JsonUndefined.Instance;
                }

                while (true) {
                    if (!_reader.ReadFieldName(out var fieldName)) {
                        parent.IsFullyParsed = true;
                        return JsonUndefined.Instance;
                    }

                    if (string.Equals(fieldName, name, StringComparison.Ordinal)) {
                        if (args.Type == LevelType.Single || args.Type.IsDictionary()) {
                            literal = _reader.ReadLiteral();
                            if (literal != JsonLiteral.Assignment) {
                                throw UnexpectedJsonException.From("assignment token", _buffer, _encoding);
                            }
                            literal = _reader.ReadLiteral();
                            if (literal == JsonLiteral.Null) {
                                return JsonNull.Instance;
                            }
                            if (literal == JsonLiteral.ObjectBegin) {
                                var child = new JsonObject();
                                _parents.Push(new JsonReadLevel(child));
                                return child;
                            }
                            throw UnexpectedJsonException.From("object begin token", _buffer, _encoding);
                        }
                        if (args.Type.IsCollection()) {
                            literal = _reader.ReadLiteral();
                            if (literal != JsonLiteral.Assignment) {
                                throw UnexpectedJsonException.From("assignment token", _buffer, _encoding);
                            }
                            literal = _reader.ReadLiteral();
                            if (literal == JsonLiteral.Null) {
                                return JsonNull.Instance;
                            }
                            if (literal == JsonLiteral.ArrayBegin) {
                                _parents.Push(ArrayLevel);
                                return ArrayLevel.Node;
                            }
                            throw UnexpectedJsonException.From("array begin token", _buffer, _encoding);
                        }
                        if (!_reader.ReadFieldValue(out var fieldValue)) {
                            throw UnexpectedJsonException.From("field value", _buffer, _encoding);
                        }
                        return fieldValue;
                    }

                    if (!_reader.ReadFieldValue(out var fieldNode)) {
                        throw UnexpectedJsonException.From("field value", _buffer, _encoding);
                    }

                    obj.Add(fieldName, fieldNode);
                }
            }

            literal = _reader.ReadLiteral();
            if (literal == JsonLiteral.Comma) {
                literal = _reader.ReadLiteral();
            }

            if (args.Type == LevelType.Collection) {
                if (literal == JsonLiteral.ArrayBegin) {
                    _parents.Push(ArrayLevel);
                    return ArrayLevel.Node;
                }
                throw UnexpectedJsonException.From("array begin token", _buffer, _encoding);
            }
            if (literal == JsonLiteral.ObjectBegin) {
                var child = new JsonObject();
                _parents.Push(new JsonReadLevel(child));
                return child;
            }
            if (literal == JsonLiteral.ArrayEnd) {
                return JsonUndefined.Instance;
            }

            return _reader.ReadValue(literal);
        }

        public ValueState TryVisit(VisitArgs args)
        {
            if (args.IsRoot) {
                var literal = _reader.ReadLiteral();
                if (literal == JsonLiteral.Null) {
                    return ValueState.Null;
                }
                switch (args.Type) {
                    case LevelType.Dictionary:
                    case LevelType.Single:
                        if (literal == JsonLiteral.ObjectBegin) {
                            _parents.Push(new JsonReadLevel(new JsonObject()));
                            return ValueState.Found;
                        }
                        throw UnexpectedJsonException.From("object begin token", _buffer, _encoding);
                    case LevelType.Collection:
                        if (literal == JsonLiteral.ArrayBegin) {
                            _parents.Push(ArrayLevel);
                            return ValueState.Found;
                        }
                        throw UnexpectedJsonException.From("array begin token", _buffer, _encoding);
                }

                throw UnexpectedJsonException.From("root begin", _buffer, _encoding);
            }

            var node = ParseUntilFound(args);
            if (node == null) {
                return ValueState.NotFound;
            }
            if (node.IsNull) {
                return ValueState.Null;
            }
            return ValueState.Found;
        }

        public void Leave(VisitArgs args)
        {
            var node = _parents.Peek();
            if (!node.IsFullyParsed) {
                if (args.Type == LevelType.Single || args.Type.IsDictionary()
                    || args.Type.IsDictionaryValue() || args.Type.IsCollectionItem()) {

                    while (_reader.ReadField(out var _, out var _)) {}
                }
            }
            _parents.Pop();
        }

        private bool TryVisitNumber<T>(VisitArgs args, Func<decimal?, T> converter, out T value)
        {
            var node = ParseUntilFound(args);
            if (node is JsonUndefined) {
                value = default;
                return false;
            }
            if (node.IsNull) {
                value = default;
                return true;
            }

            if (node is JsonString str) {
                var parsedNumber = decimal.Parse(str.Value);
                value = converter.Invoke(parsedNumber);
                return true;
            }
            if (!(node is JsonNumber number)) {
                throw UnexpectedJsonException.Type(args.Name, node, typeof(T));
            }
            value = converter.Invoke(number.Value);
            return true;
        }

        private bool TryVisitString<T>(VisitArgs args, Func<string, T> converter, out T value)
        {
            var node = ParseUntilFound(args);
            if (node is JsonUndefined) {
                value = default(T);
                return false;
            }
            if (node.IsNull) {
                value = default(T);
                return true;
            }
            if (!(node is JsonString s)) {
                throw UnexpectedJsonException.Type(args.Name, node, typeof(T));
            }
            value = converter.Invoke(s.Value);
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out byte? value)
        {
            return TryVisitNumber(args, v => (byte?) v, out value);
        }

        public bool TryVisitValue(VisitArgs args, out short? value)
        {
            return TryVisitNumber(args, v => (short?)v, out value);
        }

        public bool TryVisitValue(VisitArgs args, out int? value)
        {
            return TryVisitNumber(args, v => (int?)v, out value);
        }

        public bool TryVisitValue(VisitArgs args, out long? value)
        {
            return TryVisitNumber(args, v => (long?)v, out value);
        }

        public bool TryVisitValue(VisitArgs args, out ushort? value)
        {
            return TryVisitNumber(args, v => (ushort?)v, out value);
        }

        public bool TryVisitValue(VisitArgs args, out uint? value)
        {
            return TryVisitNumber(args, v => (uint?)v, out value);
        }

        public bool TryVisitValue(VisitArgs args, out ulong? value)
        {
            return TryVisitNumber(args, v => (ulong?)v, out value);
        }

        public bool TryVisitValue(VisitArgs args, out bool? value)
        {
            var node = ParseUntilFound(args);
            if (node is JsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }
            if (!(node is JsonBool b)) {
                throw UnexpectedJsonException.Type(args.Name, node, typeof(bool));
            }
            value = b.Value;
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out float? value)
        {
            return TryVisitNumber(args, v => (float?)v, out value);
        }

        public bool TryVisitValue(VisitArgs args, out double? value)
        {
            return TryVisitNumber(args, v => (double?)v, out value);
        }

        public bool TryVisitValue(VisitArgs args, out decimal? value)
        {
            return TryVisitNumber(args, v => (decimal?)v, out value);
        }

        public bool TryVisitValue(VisitArgs args, out TimeSpan? value)
        {
            return TryVisitString(args, s => TimeSpan.Parse(s), out value);
        }

        public bool TryVisitValue(VisitArgs args, out DateTime? value)
        {
            return TryVisitString(args,
                s => DateTime.Parse(s, JsonEncoding.DateTimeFormat,
                    DateTimeStyles.RoundtripKind), out value);
        }

        public bool TryVisitValue(VisitArgs args, out string value)
        {
            return TryVisitString(args, s => s, out value);
        }

        public bool TryVisitValue(VisitArgs args, out Guid? value)
        {
            return TryVisitString(args, s => Guid.Parse(s), out value);
        }

        public bool TryVisitValue(VisitArgs args, out byte[] value)
        {
            return TryVisitString(args, Convert.FromBase64String, out value);
        }
    }
}