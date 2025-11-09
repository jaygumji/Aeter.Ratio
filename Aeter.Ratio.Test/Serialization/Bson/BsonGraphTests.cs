using Aeter.Ratio.Testing.Fakes.Entities;
using Aeter.Ratio.Testing.Fakes.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Aeter.Ratio.Test.Serialization.Bson
{
    public class BsonGraphTests
    {
        private readonly BsonSerializationTestContext _context;

        public BsonGraphTests(ITestOutputHelper output)
        {
            _context = new BsonSerializationTestContext(output);
        }

        [Fact]
        public void WriteInt16Test()
        {
            _context.AssertBinarySingleProperty(new Int16Graph { Value = 42 });
        }

        [Fact]
        public void WriteInt32Test()
        {
            _context.AssertBinarySingleProperty(new Int32Graph { Value = 42 });
        }

        [Fact]
        public void WriteInt64Test()
        {
            _context.AssertBinarySingleProperty(new Int64Graph { Value = 42 });
        }

        [Fact]
        public void WriteUInt16Test()
        {
            _context.AssertBinarySingleProperty(new UInt16Graph { Value = 42 });
        }

        [Fact]
        public void WriteUInt32Test()
        {
            _context.AssertBinarySingleProperty(new UInt32Graph { Value = 42 });
        }

        [Fact]
        public void WriteUInt64Test()
        {
            _context.AssertBinarySingleProperty(new UInt64Graph { Value = 42 });
        }

        [Fact]
        public void WriteBooleanTest()
        {
            _context.AssertBinarySingleProperty(new BooleanGraph { Value = true });
        }

        [Fact]
        public void WriteSingleTest()
        {
            _context.AssertBinarySingleProperty(new SingleGraph { Value = 42.3f });
        }

        [Fact]
        public void WriteDoubleTest()
        {
            _context.AssertBinarySingleProperty(new DoubleGraph { Value = 42.7d });
        }

        [Fact]
        public void WriteDecimalTest()
        {
            _context.AssertBinarySingleProperty(new DecimalGraph { Value = 42.74343M });
        }

        [Fact]
        public void WriteTimeSpanTest()
        {
            _context.AssertBinarySingleProperty(new TimeSpanGraph { Value = new TimeSpan(12, 30, 00) });
        }

        [Fact]
        public void WriteDateTimeTest()
        {
            _context.AssertBinarySingleProperty(new DateTimeGraph { Value = new DateTime(2001, 01, 07, 15, 30, 24) });
        }

        [Fact]
        public void WriteStringTest()
        {
            _context.AssertBinarySingleProperty(new StringGraph { Value = "Hello World" });
        }

        [Fact]
        public void WriteGuidTest()
        {
            _context.AssertBinarySingleProperty(new GuidGraph { Value = Guid.NewGuid() });
        }

        [Fact]
        public void WriteBlobTest()
        {
            var graph = new BlobGraph { Value = new byte[] { 1, 2, 3 } };
            var actual = _context.SerializeAndDeserialize(graph);

            Assert.NotNull(actual.Value);
            Assert.True(graph.Value.SequenceEqual(actual.Value!));
        }

        [Fact]
        public void WriteEnumTest()
        {
            _context.AssertBinarySingleProperty(new EnumGraph { Value = ApplicationType.Api });
        }

        [Fact]
        public void WriteCollectionStringTest()
        {
            var graph = new CollectionGraph { Value = new List<string> { "Test" } };
            var actual = _context.SerializeAndDeserialize(graph);

            Assert.NotNull(actual.Value);
            Assert.Single(actual.Value);
            Assert.Equal("Test", actual.Value!.First());
        }

        [Fact]
        public void WriteCollectionDateTimeTest()
        {
            var dateTime = new DateTime(2025, 01, 01, 10, 00, 00);
            var graph = new CollectionGraphOfDateTime { Value = new List<DateTime> { dateTime } };
            var actual = _context.SerializeAndDeserialize(graph);

            Assert.NotNull(actual.Value);
            Assert.Single(actual.Value);
            Assert.Equal(dateTime, actual.Value!.First());
        }

        [Fact]
        public void WriteCollectionOfComplexTest()
        {
            var graph = new CollectionOfComplexGraph {
                Value = new List<Relation> { new Relation { Id = Guid.Empty, Name = "Test", Value = 1 } }
            };
            var actual = _context.SerializeAndDeserialize(graph);

            Assert.NotNull(actual.Value);
            Assert.Single(actual.Value);

            for (var i = 0; i < graph.Value.Count; i++) {
                var expectedValue = graph.Value[i];
                var actualValue = actual.Value![i];
                Assert.Equal(expectedValue, actualValue);
            }
        }
    }
}
