/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Aeter.Ratio.Serialization.Bson
{
    public class BsonReadVisitor : IReadVisitor
    {
        private readonly BsonEncoding _encoding;
        private readonly IFieldNameResolver _fieldNameResolver;
        private readonly BinaryReadBuffer _buffer;
        private readonly BsonReader _reader;
        private readonly Stack<BsonReadLevel> _parents;
        private readonly Stack<DictionaryState> _dictionaryStates = new();

        private class BsonReadLevel
        {
            public readonly IBsonNode Node;
            public readonly int Size;
            public int Position = 5; // The 4 size bytes is included and already read, and we are including the 0x00 termination
            public int Index;
            public bool IsFullyParsed => Position >= Size; // Include the trailing zero in the check
            public object? State; // Custom state to manage special cases like dictionaries

            public BsonReadLevel(IBsonNode node)
            {
                Node = node;
                if (node is BsonDocument doc) {
                    Size = doc.Size;
                }
                else if (node is BsonArray array) {
                    Size = array.Size;
                }
                else {
                    Size = -1;
                }
            }
        }
        private class DictionaryState
        {
            public List<string> Keys = new();
            public int Index = -1;
        }

        public BsonReadVisitor(BsonEncoding encoding, IFieldNameResolver fieldNameResolver, BinaryReadBuffer buffer)
        {
            _encoding = encoding;
            _fieldNameResolver = fieldNameResolver;
            _buffer = buffer;
            _reader = new BsonReader(buffer, _encoding);
            _parents = new Stack<BsonReadLevel>();
        }

        private IBsonNode ParseUntilFound(VisitArgs args)
        {
            var name = string.IsNullOrEmpty(args.Name)
                ? null
                : _fieldNameResolver.Resolve(args);

            if (args.IsRoot) {
                // When the value being deserialized is a simple value only
                throw new ArgumentException("Root must be an object");
            }

            var parent = _parents.Peek();

            if (parent.Node is BsonDocument obj) {
                if (args.Type.IsDictionaryKey()) {
                    var state = (DictionaryState)(parent.State ??= new DictionaryState());
                    if (parent.IsFullyParsed) {
                        if (!obj.Any()) return BsonUndefined.Instance;

                        if (state.Index == -1) {
                            state.Keys.AddRange(obj.Select(x => x.Key));
                            state.Index = 0;
                            return new BsonString(state.Keys[0]);
                        }
                        else {
                            if (++state.Index >= state.Keys.Count) return BsonUndefined.Instance;
                            return new BsonString(state.Keys[state.Index]);
                        }
                    }
                    else {
                        var knode = _reader.ReadDocument(obj, ref parent.Position, out var key, nameToFind: null, findFirst: true);
                        if (knode == BsonUndefined.Instance) return knode;

                        state.Keys.Add(key!);
                        state.Index++;
                        return new BsonString(key);
                    }
                }
                if (args.Type.IsDictionaryValue()) {
                    var state = (DictionaryState)(parent.State ??= new DictionaryState());
                    var key = state.Keys[state.Index];
                    if (obj.TryGet(key, out var vnode)) {
                        if (vnode is BsonDocument || vnode is BsonArray) {
                            _parents.Push(new BsonReadLevel(vnode));
                        }
                        return vnode;
                    }

                    return BsonUndefined.Instance;
                }

                if (obj.TryGet(name!, out var field)) {
                    if (field is BsonDocument) {
                        _parents.Push(new BsonReadLevel(field));
                    }
                    return field;
                }

                if (parent.IsFullyParsed) {
                    return BsonUndefined.Instance;
                }

                var node = _reader.ReadDocument(obj, ref parent.Position, out _, nameToFind: name);
                if (node is BsonDocument || node is BsonArray) {
                    _parents.Push(new BsonReadLevel(node));
                }
                return node;
            }
            if (parent.Node is BsonArray arr) {
                if (args.Type.IsCollectionItem()) {
                    if (parent.IsFullyParsed) {
                        if (arr.Count > parent.Index) {
                            return arr[parent.Index++];
                        }
                        return BsonUndefined.Instance;
                    }
                    var node = _reader.ReadArray(arr, ref parent.Position, deep: false);
                    parent.Index++;
                    if (node is BsonDocument || node is BsonArray) {
                        _parents.Push(new BsonReadLevel(node));
                    }
                    return node;
                }
                throw UnexpectedBsonException.From(args.Type.ToString(), _buffer, _encoding);
            }
            throw UnexpectedBsonException.From(string.Concat(args.Type, '|', args.Name), _buffer, _encoding);
        }

        public ValueState TryVisit(VisitArgs args)
        {
            if (args.IsRoot) {
                var size = _reader.ReadInt32();
                if (size == 0) {
                    return ValueState.Null;
                }
                switch (args.Type) {
                    case LevelType.Dictionary:
                    case LevelType.Single:
                        _parents.Push(new BsonReadLevel(new BsonDocument(size)));
                        return ValueState.Found;
                    case LevelType.Collection:
                        _parents.Push(new BsonReadLevel(new BsonArray(size)));
                        return ValueState.Found;
                }
                throw UnexpectedBsonException.From("root begin", _buffer, _encoding);
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
            var node = _parents.Pop();
            if (!node.IsFullyParsed) {
                _buffer.Advance(node.Size - node.Position - 1);
            }
            var termination = _buffer.ReadByte();
            if (termination != 0) throw UnexpectedBsonException.From("termination", _buffer, _encoding);
        }

        public bool TryVisitValue(VisitArgs args, out byte? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }

            if (node is BsonInt32 int32) {
                if (int32.Value!.Value < byte.MinValue || int32.Value!.Value > byte.MaxValue) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, int32.Value!.Value, typeof(byte));
                value = (byte?)int32.Value;
                return true;
            }
            if (node is BsonInt64 int64) {
                if (int64.Value!.Value < byte.MinValue || int64.Value!.Value > byte.MaxValue) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, int64.Value!.Value, typeof(byte));
                value = (byte?)int64.Value;
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(byte));
        }

        public bool TryVisitValue(VisitArgs args, out short? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }

            if (node is BsonInt32 int32) {
                if (int32.Value!.Value < short.MinValue || int32.Value!.Value > short.MaxValue) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, int32.Value!.Value, typeof(short));
                value = (short?)int32.Value;
                return true;
            }
            if (node is BsonInt64 int64) {
                if (int64.Value!.Value < short.MinValue || int64.Value!.Value > short.MaxValue) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, int64.Value!.Value, typeof(short));
                value = (short?)int64.Value;
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(short));
        }

        public bool TryVisitValue(VisitArgs args, out int? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }
            if (node is BsonString str && args.Type == LevelType.DictionaryKey) {
                value = ValueConverter.ChangeType<int>(str.Value);
                return true;
            }

            if (node is BsonInt32 int32) {
                value = int32.Value;
                return true;
            }
            if (node is BsonInt64 int64) {
                if (int64.Value!.Value < int.MinValue || int64.Value!.Value > int.MaxValue) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, int64.Value!.Value, typeof(int));
                value = (int?)int64.Value;
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(int));
        }

        public bool TryVisitValue(VisitArgs args, out long? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }

            if (node is BsonInt32 int32) {
                value = int32.Value;
                return true;
            }
            if (node is BsonInt64 int64) {
                value = int64.Value;
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(long));
        }

        public bool TryVisitValue(VisitArgs args, out ushort? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }

            if (node is BsonInt32 int32) {
                if (int32.Value!.Value < ushort.MinValue || int32.Value!.Value > ushort.MaxValue) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, int32.Value!.Value, typeof(ushort));
                value = (ushort?)int32.Value;
                return true;
            }
            if (node is BsonInt64 int64) {
                if (int64.Value!.Value < ushort.MinValue || int64.Value!.Value > ushort.MaxValue) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, int64.Value!.Value, typeof(ushort));
                value = (ushort?)int64.Value;
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(ushort));
        }

        public bool TryVisitValue(VisitArgs args, out uint? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }

            if (node is BsonInt32 int32) {
                if (int32.Value!.Value < uint.MinValue) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, int32.Value!.Value, typeof(uint));
                value = (uint?)int32.Value;
                return true;
            }
            if (node is BsonInt64 int64) {
                if (int64.Value!.Value < uint.MinValue || int64.Value!.Value > uint.MaxValue) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, int64.Value!.Value, typeof(uint));
                value = (uint?)int64.Value;
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(uint));
        }

        public bool TryVisitValue(VisitArgs args, out ulong? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }

            if (node is BsonInt32 int32) {
                if (int32.Value!.Value < uint.MinValue) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, int32.Value!.Value, typeof(ulong));
                value = (ulong?)int32.Value;
                return true;
            }
            if (node is BsonInt64 int64) {
                if (int64.Value!.Value < uint.MinValue) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, int64.Value!.Value, typeof(ulong));
                value = (ulong?)int64.Value;
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(ulong));
        }

        public bool TryVisitValue(VisitArgs args, out bool? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }
            if (node is not BsonBoolean b) {
                throw UnexpectedBsonException.Type(args.Name, node, typeof(bool));
            }
            value = b.Value;
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out float? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }

            if (node is BsonInt32 int32) {
                value = int32.Value;
                return true;
            }
            if (node is BsonDouble dbl) {
                value = (float)dbl.Value!.Value;
                if (float.IsInfinity(value.Value)) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, dbl.Value!.Value, typeof(float));
                return true;
            }
            if (node is BsonDecimal128 dec) {
                value = (float)dec.Value!.Value;
                if (float.IsInfinity(value.Value)) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, dec.Value!.Value, typeof(float));
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(float));
        }

        public bool TryVisitValue(VisitArgs args, out double? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }

            if (node is BsonInt32 int32) {
                value = int32.Value;
                return true;
            }
            if (node is BsonInt64 int64) {
                value = int64.Value;
                return true;
            }
            if (node is BsonDouble dbl) {
                value = dbl.Value!.Value;
                return true;
            }
            if (node is BsonDecimal128 dec) {
                value = (double)dec.Value!.Value;
                if (double.IsInfinity(value.Value)) throw UnexpectedBsonException.ValueWouldBeTruncated(args.Name, dec.Value!.Value, typeof(double));
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(double));
        }

        public bool TryVisitValue(VisitArgs args, out decimal? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }

            if (node is BsonInt32 int32) {
                value = int32.Value;
                return true;
            }
            if (node is BsonInt32 int64) {
                value = int64.Value;
                return true;
            }
            if (node is BsonDouble dbl) {
                value = (decimal)dbl.Value!.Value;
                return true;
            }
            if (node is BsonDecimal128 dec) {
                value = dec.Value!.Value;
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(decimal));
        }

        public bool TryVisitValue(VisitArgs args, out TimeSpan? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }
            if (node is BsonInt32 int32) {
                value = TimeSpan.FromMilliseconds(int32.Value!.Value);
                return true;
            }
            if (node is BsonInt64 int64) {
                value = TimeSpan.FromMilliseconds(int64.Value!.Value);
                return true;
            }
            if (node is BsonTimestamp ts) {
                value = TimeSpan.FromMilliseconds(ts.Value!.Value);
                return true;
            }
            if (node is BsonString str) {
                value = TimeSpan.Parse(str.Value!);
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(TimeSpan));
        }

        public bool TryVisitValue(VisitArgs args, out DateTime? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }
            if (node is BsonInt32 int32) {
                value = DateTime.UnixEpoch.AddMilliseconds(int32.Value!.Value);
                return true;
            }
            if (node is BsonInt64 int64) {
                value = DateTime.UnixEpoch.AddMilliseconds(int64.Value!.Value);
                return true;
            }
            if (node is BsonTimestamp ts) {
                value = DateTime.UnixEpoch.AddMilliseconds(ts.Value!.Value);
                return true;
            }
            if (node is BsonString str) {
                value = DateTime.Parse(str.Value!);
                return true;
            }
            if (node is BsonDateTime dt) {
                value = dt.Value;
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(DateTime));
        }

        public bool TryVisitValue(VisitArgs args, out string? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = default;
                return false;
            }
            if (node.IsNull) {
                value = default;
                return true;
            }
            if (node is not BsonString s) {
                throw UnexpectedBsonException.Type(args.Name, node, typeof(string));
            }
            value = s.Value;
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out Guid? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }
            if (node is BsonBinary bin) {
                value = new Guid(bin.Value);
                return true;
            }
            if (node is BsonString str) {
                value = Guid.Parse(str.Value!);
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(Guid));
        }

        public bool TryVisitValue(VisitArgs args, out byte[]? value)
        {
            var node = ParseUntilFound(args);
            if (node is BsonUndefined) {
                value = null;
                return false;
            }
            if (node.IsNull) {
                value = null;
                return true;
            }
            if (node is BsonBinary bin) {
                value = bin.Value;
                return true;
            }
            if (node is BsonString str) {
                value = Convert.FromBase64String(str.Value!);
                return true;
            }
            throw UnexpectedBsonException.Type(args.Name, node, typeof(byte[]));
        }
    }
}
