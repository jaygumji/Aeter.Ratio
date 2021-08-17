/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Testing.Fakes.Graphs;
using Xunit;


namespace Aeter.Ratio.Test.Serialization
{
    
    public class ReadNullableValuePropertyTests
    {

        private readonly TravellerTestContext _context = new TravellerTestContext();

        [Fact]
        public void ReadInt16Test()
        {
            _context.AssertReadSingleProperty<NullableInt16Graph>();
        }

        [Fact]
        public void ReadInt32Test()
        {
            _context.AssertReadSingleProperty<NullableInt32Graph>();
        }

        [Fact]
        public void ReadInt64Test()
        {
            _context.AssertReadSingleProperty<NullableInt64Graph>();
        }

        [Fact]
        public void ReadUInt16Test()
        {
            _context.AssertReadSingleProperty<NullableUInt16Graph>();
        }

        [Fact]
        public void ReadUInt32Test()
        {
            _context.AssertReadSingleProperty<NullableUInt32Graph>();
        }

        [Fact]
        public void ReadUInt64Test()
        {
            _context.AssertReadSingleProperty<NullableUInt64Graph>();
        }

        [Fact]
        public void ReadBooleanTest()
        {
            _context.AssertReadSingleProperty<NullableBooleanGraph>();
        }

        [Fact]
        public void ReadSingleTest()
        {
            _context.AssertReadSingleProperty<NullableSingleGraph>();
        }

        [Fact]
        public void ReadDoubleTest()
        {
            _context.AssertReadSingleProperty<NullableDoubleGraph>();
        }

        [Fact]
        public void ReadDecimalTest()
        {
            _context.AssertReadSingleProperty<NullableDecimalGraph>();
        }

        [Fact]
        public void ReadTimeSpanTest()
        {
            _context.AssertReadSingleProperty<NullableTimeSpanGraph>();
        }

        [Fact]
        public void ReadDateTimeTest()
        {
            _context.AssertReadSingleProperty<NullableDateTimeGraph>();
        }

        [Fact]
        public void ReadGuidTest()
        {
            _context.AssertReadSingleProperty<NullableGuidGraph>();
        }

        [Fact]
        public void ReadEnumTest()
        {
            _context.AssertReadSingleProperty<NullableEnumGraph>();
        }

    }
}