/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections.Generic;
using Aeter.Ratio.Serialization;
using Aeter.Ratio.Testing.Fakes.Graphs;
using Xunit;


namespace Aeter.Ratio.Test.Serialization
{
    
    public class ReadValuePropertyTests
    {

        private readonly TravellerTestContext _context = new TravellerTestContext();

        [Fact]
        public void ReadInt16Test()
        {
            _context.AssertReadSingleProperty<Int16Graph>();
        }

        [Fact]
        public void ReadInt32Test()
        {
            _context.AssertReadSingleProperty<Int32Graph>();
        }

        [Fact]
        public void ReadInt64Test()
        {
            _context.AssertReadSingleProperty<Int64Graph>();
        }

        [Fact]
        public void ReadUInt16Test()
        {
            _context.AssertReadSingleProperty<UInt16Graph>();
        }

        [Fact]
        public void ReadUInt32Test()
        {
            _context.AssertReadSingleProperty<UInt32Graph>();
        }

        [Fact]
        public void ReadUInt64Test()
        {
            _context.AssertReadSingleProperty<UInt64Graph>();
        }

        [Fact]
        public void ReadBooleanTest()
        {
            _context.AssertReadSingleProperty<BooleanGraph>();
        }

        [Fact]
        public void ReadSingleTest()
        {
            _context.AssertReadSingleProperty<SingleGraph>();
        }

        [Fact]
        public void ReadDoubleTest()
        {
            _context.AssertReadSingleProperty<DoubleGraph>();
        }

        [Fact]
        public void ReadDecimalTest()
        {
            _context.AssertReadSingleProperty<DecimalGraph>();
        }

        [Fact]
        public void ReadTimeSpanTest()
        {
            _context.AssertReadSingleProperty<TimeSpanGraph>();
        }

        [Fact]
        public void ReadDateTimeTest()
        {
            _context.AssertReadSingleProperty<DateTimeGraph>();
        }

        [Fact]
        public void ReadStringTest()
        {
            _context.AssertReadSingleProperty<StringGraph>();
        }

        [Fact]
        public void ReadGuidTest()
        {
            _context.AssertReadSingleProperty<GuidGraph>();
        }

        [Fact]
        public void ReadBlobTest()
        {
            _context.AssertReadSingleProperty<BlobGraph>();
        }

        [Fact]
        public void ReadEnumTest()
        {
            _context.AssertReadSingleProperty<EnumGraph>();
        }

        [Fact]
        public void ReadComplexTest()
        {
            var stats = _context.AssertRead<ComplexGraph>(4);
            stats.AssertVisitOrderExact(LevelType.Single, LevelType.Value, LevelType.Value, LevelType.Value, LevelType.Value);
        }

        [Fact]
        public void ReadDictionaryTest()
        {
            var stats = _context.AssertRead<DictionaryGraph>(3);
            stats.AssertVisitOrderExact(LevelType.Dictionary, LevelType.DictionaryKey, LevelType.DictionaryValue,
                LevelType.DictionaryKey);
        }

        [Fact]
        public void ReadDictionaryWithComplexValueTest()
        {
            var stats = _context.AssertRead<DictionaryWithComplexValueGraph>(5);
            stats.AssertVisitOrderExact(LevelType.Dictionary, LevelType.DictionaryKey, LevelType.DictionaryValue,
                LevelType.Value, LevelType.Value, LevelType.Value, LevelType.DictionaryKey);
        }

        [Fact]
        public void ReadDictionaryWithComplexKeyAndValueTest()
        {
            var stats = _context.AssertRead<DictionaryWithComplexKeyAndValueGraph>(5);
            stats.AssertVisitOrderExact(LevelType.Dictionary, LevelType.DictionaryKey, LevelType.Value, LevelType.Value,
                LevelType.DictionaryValue, LevelType.Value, LevelType.Value, LevelType.Value,
                LevelType.DictionaryKey);
        }

        [Fact]
        public void ReadDictionaryCount3WithComplexKeyAndValueTest()
        {
            var stats = _context.AssertRead<DictionaryWithComplexKeyAndValueGraph>(15, 3);
            stats.AssertVisitOrderExact(LevelType.Dictionary, LevelType.DictionaryKey, LevelType.Value, LevelType.Value,
                LevelType.DictionaryValue, LevelType.Value, LevelType.Value, LevelType.Value,
                LevelType.DictionaryKey, LevelType.Value, LevelType.Value,
                LevelType.DictionaryValue, LevelType.Value, LevelType.Value, LevelType.Value,
                LevelType.DictionaryKey, LevelType.Value, LevelType.Value,
                LevelType.DictionaryValue, LevelType.Value, LevelType.Value, LevelType.Value,
                LevelType.DictionaryKey);
        }

        [Fact]
        public void ReadDictionaryWithComplexKeyTest()
        {
            var stats = _context.AssertRead<DictionaryWithComplexKeyGraph>(3);
            stats.AssertVisitOrderExact(LevelType.Dictionary, LevelType.DictionaryKey, LevelType.Value, LevelType.Value,
                LevelType.DictionaryValue, LevelType.DictionaryKey);
        }

        [Fact]
        public void ReadDictionaryWithDictionaryKeyTest()
        {
            var stats = _context.AssertRead<DictionaryWithDictionaryKeyGraph>(4);
            stats.AssertVisitOrderExact(LevelType.Dictionary, LevelType.DictionaryInDictionaryKey,
                LevelType.DictionaryKey, LevelType.DictionaryValue, LevelType.DictionaryKey, LevelType.DictionaryValue,
                LevelType.DictionaryInDictionaryKey);
        }

        [Fact]
        public void ReadDictionaryWithDictionaryValueTest()
        {
            var stats = _context.AssertRead<DictionaryWithDictionaryValueGraph>(5);
            stats.AssertVisitOrderExact(LevelType.Dictionary, LevelType.DictionaryKey,
                LevelType.DictionaryInDictionaryValue, LevelType.DictionaryKey, LevelType.DictionaryValue,
                LevelType.DictionaryKey, LevelType.DictionaryKey);
        }

        [Fact]
        public void ReadDictionaryWithDictionaryKeyAndValueTest()
        {
            var stats = _context.AssertRead<DictionaryWithDictionaryKeyAndValueGraph>(6);
            stats.AssertVisitOrderExact(LevelType.Dictionary, LevelType.DictionaryInDictionaryKey,
                LevelType.DictionaryKey, LevelType.DictionaryValue, LevelType.DictionaryKey,
                LevelType.DictionaryInDictionaryValue, LevelType.DictionaryKey, LevelType.DictionaryValue,
                LevelType.DictionaryKey, LevelType.DictionaryInDictionaryKey);
        }

        [Fact]
        public void ReadDictionaryWithCollectionKeyTest()
        {
            var stats = _context.AssertRead<DictionaryWithCollectionKeyGraph>(3);
            stats.AssertVisitOrderExact(LevelType.Dictionary, LevelType.CollectionInDictionaryKey,
                LevelType.CollectionItem, LevelType.CollectionItem, LevelType.DictionaryValue,
                LevelType.CollectionInDictionaryKey);
        }

        [Fact]
        public void ReadDictionaryWithCollectionValueTest()
        {
            var stats = _context.AssertRead<DictionaryWithCollectionValueGraph>(4);
            stats.AssertVisitOrderExact(LevelType.Dictionary, LevelType.DictionaryKey,
                LevelType.CollectionInDictionaryValue, LevelType.CollectionItem, LevelType.CollectionItem,
                LevelType.DictionaryKey);
        }

        [Fact]
        public void ReadDictionaryWithCollectionKeyAndValueTest()
        {
            var stats = _context.AssertRead<DictionaryWithCollectionKeyAndValueGraph>(4);
            stats.AssertVisitOrderExact(LevelType.Dictionary, LevelType.CollectionInDictionaryKey,
                LevelType.CollectionItem, LevelType.CollectionItem, LevelType.CollectionInDictionaryValue,
                LevelType.CollectionItem, LevelType.CollectionItem, LevelType.CollectionInDictionaryKey);
        }

        [Fact]
        public void ReadCollectionTest()
        {
            var stats = _context.AssertRead<CollectionGraph>(2);
            stats.AssertVisitOrderExact(LevelType.Collection, LevelType.CollectionItem, LevelType.CollectionItem);
        }

        [Fact]
        public void ReadCollectionOfComplexTest()
        {
            var stats = _context.AssertRead<CollectionOfComplexGraph>(4);
            stats.AssertVisitOrderExact(LevelType.Collection, LevelType.CollectionItem, LevelType.Value, LevelType.Value,
                LevelType.Value, LevelType.Value, LevelType.CollectionItem);
        }

        [Fact]
        public void ReadCollectionOfDictionaryTest()
        {
            var stats = _context.AssertRead<CollectionOfDictionaryGraph>(3);
            stats.AssertVisitOrderExact(LevelType.Collection, LevelType.DictionaryInCollection, LevelType.DictionaryKey,
                LevelType.DictionaryValue, LevelType.DictionaryKey, LevelType.DictionaryInCollection);
        }

        [Fact]
        public void ReadCollectionOfCollectionTest()
        {
            var stats = _context.AssertRead<CollectionOfCollectionGraph>(2);
            stats.AssertVisitOrderExact(LevelType.Collection, LevelType.CollectionInCollection, LevelType.CollectionItem,
                LevelType.CollectionItem, LevelType.CollectionInCollection);
        }

        [Fact]
        public void ReadJaggedArrayTest()
        {
            var stats = _context.AssertRead<JaggedArrayGraph>(2);
            stats.AssertVisitOrderExact(LevelType.Collection, LevelType.CollectionInCollection, LevelType.CollectionItem,
                LevelType.CollectionItem, LevelType.CollectionInCollection);
        }

        [Fact]
        public void ReadMultidimensionalArrayTest()
        {
            var stats = _context.AssertRead<MultidimensionalArrayGraph>(2);
            stats.AssertVisitOrderExact(LevelType.Collection, LevelType.CollectionInCollection, LevelType.CollectionItem,
                LevelType.CollectionItem, LevelType.CollectionInCollection);
        }

    }
}