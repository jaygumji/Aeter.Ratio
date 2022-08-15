/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.Serialization.Json;
using System;
using System.Collections.Generic;

namespace Aeter.Ratio.Serialization.Bson
{
    public class BsonWriteVisitor : IWriteVisitor
    {
        private readonly BsonEncoding _encoding;
        private readonly IFieldNameResolver _fieldNameResolver;
        private readonly BinaryWriteBuffer _writeBuffer;
        private readonly Stack<BinaryBufferReservation> _states;

        public BsonWriteVisitor(BsonEncoding encoding,
            IFieldNameResolver fieldNameResolver,
            BinaryWriteBuffer writeBuffer)
            : this(encoding, fieldNameResolver, writeBuffer, new Stack<BinaryBufferReservation>())
        {
        }

        /// <summary>
        /// Used by unit tests to simulate the stack already been set.
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="fieldNameResolver"></param>
        /// <param name="writeBuffer"></param>
        /// <param name="stack"></param>
        public BsonWriteVisitor(BsonEncoding encoding,
            IFieldNameResolver fieldNameResolver,
            BinaryWriteBuffer writeBuffer,
            Stack<BinaryBufferReservation> states)
        {
            _encoding = encoding;
            _fieldNameResolver = fieldNameResolver;
            _writeBuffer = writeBuffer;
            _states = states;
        }

        private void WriteString(string? value, bool includeLength)
        {
            if (value == null) {
                _writeBuffer.WriteByte((byte)BsonTypeCode.Null);
                return;
            }
            _writeBuffer.WriteByte((byte)BsonTypeCode.String);
            _writeBuffer.RequestSpace(value.Length * 4 + 4);
            var length = _encoding.BaseEncoding.GetBytes(value, 0, value.Length, _writeBuffer.Buffer, _writeBuffer.Position + 1) + 1;
            if (includeLength) {
                var lengthBytes = BitConverter.GetBytes(length);
                _writeBuffer.Write(lengthBytes);
            }
            _writeBuffer.Advance(length);
        }

        public void Visit(object? level, VisitArgs args)
        {
            WriteValueHeader(args);
            if (level == null) {
                _writeBuffer.WriteByte((byte)BsonTypeCode.Null);
                return;
            }
            if (args.IsRoot) {
                _states.Push(_writeBuffer.Reserve(4));
                return;
            }
            switch (args.Type) {
                case LevelType.CollectionInDictionaryKey:
                    throw new NotSupportedException("Collections are not supported in dictionary keys");
                case LevelType.DictionaryInDictionaryKey:
                    throw new NotSupportedException("Dictionaries are not supported in dictionary keys");
                case LevelType.DictionaryKey:
                    throw new NotSupportedException("Objects are not supported in dictionary keys");
                case LevelType.Collection:
                case LevelType.CollectionInCollection:
                case LevelType.CollectionInDictionaryValue:
                    _states.Push(_writeBuffer.Reserve(4));
                    _writeBuffer.WriteByte((byte)BsonTypeCode.Array);
                    break;
                default:
                    _states.Push(_writeBuffer.Reserve(4));
                    _writeBuffer.WriteByte((byte)BsonTypeCode.Document);
                    break;
            }
        }

        public void Leave(object? level, VisitArgs args)
        {
            if (level == null) {
                return;
            }

            _writeBuffer.ApplyInt32Size(_states.Pop());
            _writeBuffer.WriteByte(0);
        }

        private void WriteValueHeader(VisitArgs args)
        {
            if (args.Name == null) return;
            switch (args.Type) {
                case LevelType.CollectionItem:
                    WriteString(args.Index.ToString(), includeLength: false);
                    return;
                case LevelType.DictionaryKey:
                    // Dictionary keys are written as value without length
                    return;
                case LevelType.DictionaryValue:
                    // The preceding DictionaryKey visit should have written the header
                    return;
            }
            // Written as cstring
            WriteString(_fieldNameResolver.Resolve(args), includeLength: false);
        }

        private void WriteValueDefault<T>(T? value, VisitArgs args, byte type, IBinaryInformation<T> info)
            where T : struct
        {
            WriteValueHeader(args);
            if (value == null) {
                _writeBuffer.WriteByte((byte)BsonTypeCode.Null);
            }
            else {
                _writeBuffer.WriteByte(type);
                _writeBuffer.Write(info.Converter.Convert(value));
            }
        }

        public void VisitValue(byte? value, VisitArgs args)
        {
            WriteValueHeader(args);
            if (value == null) {
                _writeBuffer.WriteByte((byte)BsonTypeCode.Null);
            }
            else {
                _writeBuffer.WriteByte((byte)BsonTypeCode.Int32);
                _writeBuffer.WriteByte(value.Value);
                _writeBuffer.WriteByte(0);
                _writeBuffer.WriteByte(0);
                _writeBuffer.WriteByte(0);
            }
        }

        public void VisitValue(short? value, VisitArgs args)
        {
            WriteValueDefault(value, args, (byte)BsonTypeCode.Int32, BinaryInformation.Int32);
        }

        public void VisitValue(int? value, VisitArgs args)
        {
            WriteValueDefault(value, args, (byte)BsonTypeCode.Int32, BinaryInformation.Int32);
        }

        public void VisitValue(long? value, VisitArgs args)
        {
            WriteValueDefault(value, args, (byte)BsonTypeCode.Int64, BinaryInformation.Int64);
        }

        public void VisitValue(ushort? value, VisitArgs args)
        {
            WriteValueDefault(value, args, (byte)BsonTypeCode.Int32, BinaryInformation.Int32);
        }

        public void VisitValue(uint? value, VisitArgs args)
        {
            WriteValueDefault((int?)value, args, (byte)BsonTypeCode.Int32, BinaryInformation.Int32);
        }

        public void VisitValue(ulong? value, VisitArgs args)
        {
            WriteValueDefault((long?)value, args, (byte)BsonTypeCode.Int64, BinaryInformation.Int64);
        }

        public void VisitValue(bool? value, VisitArgs args)
        {
            if (args.Type == LevelType.DictionaryKey) {
                throw new NotSupportedException("A boolean is not supported as dictionary key.");
            }

            WriteValueHeader(args);
            if (value == null) {
                _writeBuffer.WriteByte((byte)BsonTypeCode.Null);
            }
            else {
                _writeBuffer.WriteByte((byte)BsonTypeCode.Boolean);
                _writeBuffer.WriteByte(value.Value.ToBsonByte());
            }
        }

        public void VisitValue(float? value, VisitArgs args)
        {
            WriteValueDefault(value, args, (byte)BsonTypeCode.Double, BinaryInformation.Double);
        }

        public void VisitValue(double? value, VisitArgs args)
        {
            WriteValueDefault(value, args, (byte)BsonTypeCode.Double, BinaryInformation.Double);
        }

        public void VisitValue(decimal? value, VisitArgs args)
        {
            WriteValueDefault(value, args, (byte)BsonTypeCode.Decimal128, BinaryInformation.Decimal);
        }

        public void VisitValue(TimeSpan? value, VisitArgs args)
        {
            WriteValueHeader(args);
            if (value == null) {
                _writeBuffer.WriteByte((byte)BsonTypeCode.Null);
            }
            else {
                _writeBuffer.WriteByte((byte)BsonTypeCode.UInt64);
                _writeBuffer.Write(BinaryInformation.UInt64.Converter.Convert((ulong)value.Value.Ticks));
            }
        }

        public void VisitValue(DateTime? value, VisitArgs args)
        {
            WriteValueHeader(args);
            if (value == null) {
                _writeBuffer.WriteByte((byte)BsonTypeCode.Null);
            }
            else {
                _writeBuffer.WriteByte((byte)BsonTypeCode.UInt64);
                _writeBuffer.Write(BinaryInformation.UInt64.Converter.Convert((ulong)value.Value.Ticks));
            }
        }

        public void VisitValue(string? value, VisitArgs args)
        {
            WriteValueHeader(args);
            WriteString(value, includeLength: args.Type != LevelType.DictionaryKey);
        }

        public void VisitValue(Guid? value, VisitArgs args)
        {
            if (value == null) {
                _writeBuffer.WriteByte((byte)BsonTypeCode.Null);
                WriteValueHeader(args);
            }
            else {
                _writeBuffer.WriteByte((byte)BsonTypeCode.Binary);
                WriteValueHeader(args);
                _writeBuffer.Write(BinaryInformation.Int32.Converter.Convert(16));
                _writeBuffer.WriteByte((byte)BsonBinarySubtypeCode.UUID);
                _writeBuffer.Write(value.Value.ToByteArray());
            }
        }

        public void VisitValue(byte[]? value, VisitArgs args)
        {
            WriteValueHeader(args);
            if (value == null) {
                _writeBuffer.WriteByte((byte)BsonTypeCode.Null);
            }
            else {
                _writeBuffer.WriteByte((byte)BsonTypeCode.Binary);
                _writeBuffer.Write(BinaryInformation.Int32.Converter.Convert(value.Length));
                _writeBuffer.WriteByte((byte)BsonBinarySubtypeCode.Generic);
                _writeBuffer.Write(value);
            }
        }
    }
}