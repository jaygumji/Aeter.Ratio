/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.IO;
using Aeter.Ratio.Serialization;
using Aeter.Ratio.Serialization.Bson;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Aeter.Ratio.Test.Serialization.Bson
{
    public class BsonReaderWriterTests
    {
        private const byte ByteValue = 42;
        private const short Int16Value = -1234;
        private const int Int32Value = 123456789;
        private const long Int64Value = 987654321098;
        private const ushort UInt16Value = 54321;
        private const uint UInt32Value = 2_000_000_000;
        private const ulong UInt64Value = 8_000_000_000_000_000_000UL;
        private const bool BoolValue = true;
        private const double DoubleValue = 42.42;
        private const decimal DecimalValue = 98765.4321m;
        private const string StringValue = "Hello BSON";
        private const string NullStringField = "NullText";
        private static readonly Guid GuidValue = Guid.Parse("5f6a7b8c-9d0e-4f1a-936b-bc4d2fb7a1cd");
        private static readonly byte[] BinaryValue = { 0, 1, 2, 3, 4 };
        private static readonly byte[] SampleDocument = CreateSampleDocumentBytes();

        [Fact]
        public void BsonReader_ReadsPrimitiveNodes()
        {
            using var stream = new MemoryStream(SampleDocument);
            using var buffer = new BinaryReadBuffer(1024, BinaryStream.MemoryStream(stream));
            var reader = new BsonReader(buffer, BsonEncoding.UTF8);
            var size = reader.ReadInt32();
            Assert.True(size > 0);

            var document = new BsonDocument(size);
            var parsed = 4 + 1; // Include size data and termination
            reader.ReadDocument(document, ref parsed, out _);

            AssertBsonInt32(document, "byteValue", ByteValue);
            AssertBsonInt32(document, "int16Value", Int16Value);
            AssertBsonInt32(document, "int32Value", Int32Value);
            AssertBsonInt64(document, "int64Value", Int64Value);
            AssertBsonInt32(document, "uInt16Value", UInt16Value);
            AssertBsonInt64(document, "uInt32Value", (int)UInt32Value);
            AssertBsonInt64(document, "uInt64Value", (long)UInt64Value);

            var boolNode = GetNode(document, "boolValue");
            var bsonBool = Assert.IsType<BsonBoolean>(boolNode);
            Assert.True(bsonBool.Value);

            var doubleNode = GetNode(document, "doubleValue");
            var bsonDouble = Assert.IsType<BsonDouble>(doubleNode);
            Assert.Equal(DoubleValue, bsonDouble.Value);

            var decimalNode = GetNode(document, "decimalValue");
            var bsonDecimal = Assert.IsType<BsonDecimal128>(decimalNode);
            Assert.Equal(DecimalValue, bsonDecimal.Value);

            var stringNode = GetNode(document, "stringValue");
            var bsonString = Assert.IsType<BsonString>(stringNode);
            Assert.Equal(StringValue, bsonString.Value);

            var guidNode = GetNode(document, "guidValue");
            var guidBinary = Assert.IsType<BsonBinary>(guidNode);
            Assert.Equal(BsonBinarySubtypeCode.UUID, guidBinary.Subtype);
            Assert.Equal(GuidValue.ToByteArray(), guidBinary.Value);

            var binaryNode = GetNode(document, "binaryValue");
            var blob = Assert.IsType<BsonBinary>(binaryNode);
            Assert.Equal(BsonBinarySubtypeCode.Generic, blob.Subtype);
            Assert.Equal(BinaryValue, blob.Value);

            var nullNode = GetNode(document, "nullText");
            Assert.IsType<BsonNull>(nullNode);
        }

        [Fact]
        public void BsonReadVisitor_ReadsPrimitiveValues()
        {
            using var stream = new MemoryStream(SampleDocument);
            using var buffer = new BinaryReadBuffer(1024, BinaryStream.MemoryStream(stream));
            var visitor = new BsonReadVisitor(BsonEncoding.UTF8, new CamelCaseFieldNameResolver(), buffer);
            var rootArgs = VisitArgs.CreateRoot(LevelType.Dictionary);

            Assert.Equal(ValueState.Found, visitor.TryVisit(rootArgs));

            AssertByte(visitor, "ByteValue", ByteValue);
            AssertInt16(visitor, "Int16Value", Int16Value);
            AssertInt32(visitor, "Int32Value", Int32Value);
            AssertInt64(visitor, "Int64Value", Int64Value);
            AssertUInt16(visitor, "UInt16Value", UInt16Value);
            AssertUInt32(visitor, "UInt32Value", UInt32Value);
            AssertUInt64(visitor, "UInt64Value", UInt64Value);
            AssertBool(visitor, "BoolValue", BoolValue);
            AssertDouble(visitor, "DoubleValue", DoubleValue);
            AssertDecimal(visitor, "DecimalValue", DecimalValue);
            AssertString(visitor, "StringValue", StringValue);
            AssertGuid(visitor, "GuidValue", GuidValue);
            AssertBinary(visitor, "BinaryValue", BinaryValue);

            var nullArgs = new VisitArgs(NullStringField, LevelType.Single);
            Assert.Equal(ValueState.Null, visitor.TryVisit(nullArgs));
            Assert.True(visitor.TryVisitValue(nullArgs, out string? nullValue));
            Assert.Null(nullValue);

            visitor.Leave(rootArgs);
        }

        private static byte[] CreateSampleDocumentBytes()
        {
            using var stream = new MemoryStream();
            using var buffer = new BinaryWriteBuffer(2048, BinaryStream.MemoryStream(stream));
            var visitor = new BsonWriteVisitor(BsonEncoding.UTF8, new CamelCaseFieldNameResolver(), buffer);
            var rootArgs = VisitArgs.CreateRoot(LevelType.Dictionary);
            var root = new object();

            visitor.Visit(root, rootArgs);

            visitor.VisitValue(ByteValue, new VisitArgs("ByteValue", LevelType.Single));
            visitor.VisitValue(Int16Value, new VisitArgs("Int16Value", LevelType.Single));
            visitor.VisitValue(Int32Value, new VisitArgs("Int32Value", LevelType.Single));
            visitor.VisitValue(Int64Value, new VisitArgs("Int64Value", LevelType.Single));
            visitor.VisitValue(UInt16Value, new VisitArgs("UInt16Value", LevelType.Single));
            visitor.VisitValue(UInt32Value, new VisitArgs("UInt32Value", LevelType.Single));
            visitor.VisitValue(UInt64Value, new VisitArgs("UInt64Value", LevelType.Single));
            visitor.VisitValue(BoolValue, new VisitArgs("BoolValue", LevelType.Single));
            visitor.VisitValue(DoubleValue, new VisitArgs("DoubleValue", LevelType.Single));
            visitor.VisitValue(DecimalValue, new VisitArgs("DecimalValue", LevelType.Single));
            visitor.VisitValue(StringValue, new VisitArgs("StringValue", LevelType.Single));
            visitor.VisitValue((string?)null, new VisitArgs(NullStringField, LevelType.Single));
            visitor.VisitValue(GuidValue, new VisitArgs("GuidValue", LevelType.Single));
            visitor.VisitValue(BinaryValue.ToArray(), new VisitArgs("BinaryValue", LevelType.Single));

            visitor.Leave(root, rootArgs);
            buffer.Flush();
            return stream.ToArray();
        }

        private static IBsonNode GetNode(BsonDocument document, string fieldName)
        {
            Assert.True(document.TryGet(fieldName, out var node));
            return node!;
        }

        private static void AssertBsonInt32(BsonDocument document, string fieldName, int expected)
        {
            var node = Assert.IsType<BsonInt32>(GetNode(document, fieldName));
            Assert.Equal(expected, node.Value);
        }

        private static void AssertBsonInt64(BsonDocument document, string fieldName, long expected)
        {
            var node = Assert.IsType<BsonInt64>(GetNode(document, fieldName));
            Assert.Equal(expected, node.Value);
        }

        private static void AssertByte(BsonReadVisitor visitor, string name, byte expected)
        {
            var args = new VisitArgs(name, LevelType.Single);
            Assert.Equal(ValueState.Found, visitor.TryVisit(args));
            Assert.True(visitor.TryVisitValue(args, out byte? value));
            Assert.Equal(expected, value);
        }

        private static void AssertInt16(BsonReadVisitor visitor, string name, short expected)
        {
            var args = new VisitArgs(name, LevelType.Single);
            Assert.Equal(ValueState.Found, visitor.TryVisit(args));
            Assert.True(visitor.TryVisitValue(args, out short? value));
            Assert.Equal(expected, value);
        }

        private static void AssertInt32(BsonReadVisitor visitor, string name, int expected)
        {
            var args = new VisitArgs(name, LevelType.Single);
            Assert.Equal(ValueState.Found, visitor.TryVisit(args));
            Assert.True(visitor.TryVisitValue(args, out int? value));
            Assert.Equal(expected, value);
        }

        private static void AssertInt64(BsonReadVisitor visitor, string name, long expected)
        {
            var args = new VisitArgs(name, LevelType.Single);
            Assert.Equal(ValueState.Found, visitor.TryVisit(args));
            Assert.True(visitor.TryVisitValue(args, out long? value));
            Assert.Equal(expected, value);
        }

        private static void AssertUInt16(BsonReadVisitor visitor, string name, ushort expected)
        {
            var args = new VisitArgs(name, LevelType.Single);
            Assert.Equal(ValueState.Found, visitor.TryVisit(args));
            Assert.True(visitor.TryVisitValue(args, out ushort? value));
            Assert.Equal(expected, value);
        }

        private static void AssertUInt32(BsonReadVisitor visitor, string name, uint expected)
        {
            var args = new VisitArgs(name, LevelType.Single);
            Assert.Equal(ValueState.Found, visitor.TryVisit(args));
            Assert.True(visitor.TryVisitValue(args, out uint? value));
            Assert.Equal(expected, value);
        }

        private static void AssertUInt64(BsonReadVisitor visitor, string name, ulong expected)
        {
            var args = new VisitArgs(name, LevelType.Single);
            Assert.Equal(ValueState.Found, visitor.TryVisit(args));
            Assert.True(visitor.TryVisitValue(args, out ulong? value));
            Assert.Equal(expected, value);
        }

        private static void AssertBool(BsonReadVisitor visitor, string name, bool expected)
        {
            var args = new VisitArgs(name, LevelType.Single);
            Assert.Equal(ValueState.Found, visitor.TryVisit(args));
            Assert.True(visitor.TryVisitValue(args, out bool? value));
            Assert.Equal(expected, value);
        }

        private static void AssertDouble(BsonReadVisitor visitor, string name, double expected)
        {
            var args = new VisitArgs(name, LevelType.Single);
            Assert.Equal(ValueState.Found, visitor.TryVisit(args));
            Assert.True(visitor.TryVisitValue(args, out double? value));
            Assert.Equal(expected, value);
        }

        private static void AssertDecimal(BsonReadVisitor visitor, string name, decimal expected)
        {
            var args = new VisitArgs(name, LevelType.Single);
            Assert.Equal(ValueState.Found, visitor.TryVisit(args));
            Assert.True(visitor.TryVisitValue(args, out decimal? value));
            Assert.Equal(expected, value);
        }

        private static void AssertString(BsonReadVisitor visitor, string name, string expected)
        {
            var args = new VisitArgs(name, LevelType.Single);
            Assert.Equal(ValueState.Found, visitor.TryVisit(args));
            Assert.True(visitor.TryVisitValue(args, out string? value));
            Assert.Equal(expected, value);
        }

        private static void AssertGuid(BsonReadVisitor visitor, string name, Guid expected)
        {
            var args = new VisitArgs(name, LevelType.Single);
            Assert.Equal(ValueState.Found, visitor.TryVisit(args));
            Assert.True(visitor.TryVisitValue(args, out Guid? value));
            Assert.Equal(expected, value);
        }

        private static void AssertBinary(BsonReadVisitor visitor, string name, byte[] expected)
        {
            var args = new VisitArgs(name, LevelType.Single);
            Assert.Equal(ValueState.Found, visitor.TryVisit(args));
            Assert.True(visitor.TryVisitValue(args, out byte[]? value));
            Assert.Equal(expected, value);
        }
    }
}
