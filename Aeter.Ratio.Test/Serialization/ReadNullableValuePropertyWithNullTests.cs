/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Testing.Fakes.Graphs;
using Xunit;


namespace Aeter.Ratio.Test.Serialization
{
    
    public class ReadNullableValuePropertyWithNullTests
    {

        private readonly TravellerTestContext _context = new TravellerTestContext();

        [Fact]
        public void ReadInt16Test()
        {
            _context.AssertReadSinglePropertyWithNull<NullableInt16Graph>();
        }

        [Fact]
        public void ReadInt32Test()
        {
            _context.AssertReadSinglePropertyWithNull<NullableInt32Graph>();
        }

        [Fact]
        public void ReadInt64Test()
        {
            _context.AssertReadSinglePropertyWithNull<NullableInt64Graph>();
        }

        [Fact]
        public void ReadUInt16Test()
        {
            _context.AssertReadSinglePropertyWithNull<NullableUInt16Graph>();
        }

        [Fact]
        public void ReadUInt32Test()
        {
            _context.AssertReadSinglePropertyWithNull<NullableUInt32Graph>();
        }

        [Fact]
        public void ReadUInt64Test()
        {
            _context.AssertReadSinglePropertyWithNull<NullableUInt64Graph>();
        }

        [Fact]
        public void ReadBooleanTest()
        {
            _context.AssertReadSinglePropertyWithNull<NullableBooleanGraph>();
        }

        [Fact]
        public void ReadSingleTest()
        {
            _context.AssertReadSinglePropertyWithNull<NullableSingleGraph>();
        }

        [Fact]
        public void ReadDoubleTest()
        {
            _context.AssertReadSinglePropertyWithNull<NullableDoubleGraph>();
        }

        [Fact]
        public void ReadDecimalTest()
        {
            _context.AssertReadSinglePropertyWithNull<NullableDecimalGraph>();
        }

        [Fact]
        public void ReadTimeSpanTest()
        {
            _context.AssertReadSinglePropertyWithNull<NullableTimeSpanGraph>();
        }

        [Fact]
        public void ReadDateTimeTest()
        {
            _context.AssertReadSinglePropertyWithNull<NullableDateTimeGraph>();
        }

        [Fact]
        public void ReadGuidTest()
        {
            _context.AssertReadSinglePropertyWithNull<NullableGuidGraph>();
        }

        [Fact]
        public void ReadEnumTest()
        {
            _context.AssertReadSinglePropertyWithNull<NullableEnumGraph>();
        }

    }
}