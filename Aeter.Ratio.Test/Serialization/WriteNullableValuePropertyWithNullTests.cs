/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Testing.Fakes.Graphs;
using Xunit;

namespace Aeter.Ratio.Test.Serialization.Binary
{
    
    public class WriteNullableValuePropertyWithNullTests
    {

        private readonly TravellerTestContext _context = new TravellerTestContext();

        [Fact]
        public void WriteNullableInt16Test()
        {
            _context.AssertWriteSingleProperty(new NullableInt16Graph { Value = null });
        }

        [Fact]
        public void WriteNullableInt32Test()
        {
            _context.AssertWriteSingleProperty(new NullableInt32Graph { Value = null });
        }

        [Fact]
        public void WriteNullableInt64Test()
        {
            _context.AssertWriteSingleProperty(new NullableInt64Graph { Value = null });
        }

        [Fact]
        public void WriteNullableUInt16Test()
        {
            _context.AssertWriteSingleProperty(new NullableUInt16Graph { Value = null });
        }

        [Fact]
        public void WriteNullableUInt32Test()
        {
            _context.AssertWriteSingleProperty(new NullableUInt32Graph { Value = null });
        }

        [Fact]
        public void WriteNullableUInt64Test()
        {
            _context.AssertWriteSingleProperty(new NullableUInt64Graph { Value = null });
        }

        [Fact]
        public void WriteNullableBooleanTest()
        {
            _context.AssertWriteSingleProperty(new NullableBooleanGraph { Value = null });
        }

        [Fact]
        public void WriteNullableSingleTest()
        {
            _context.AssertWriteSingleProperty(new NullableSingleGraph { Value = null });
        }

        [Fact]
        public void WriteNullableDoubleTest()
        {
            _context.AssertWriteSingleProperty(new NullableDoubleGraph { Value = null });
        }

        [Fact]
        public void WriteNullableDecimalTest()
        {
            _context.AssertWriteSingleProperty(new NullableDecimalGraph { Value = null });
        }

        [Fact]
        public void WriteNullableTimeSpanTest()
        {
            _context.AssertWriteSingleProperty(new NullableTimeSpanGraph { Value = null });
        }

        [Fact]
        public void WriteNullableDateTimeTest()
        {
            _context.AssertWriteSingleProperty(new NullableDateTimeGraph { Value = null });
        }

        [Fact]
        public void WriteNullableGuidTest()
        {
            _context.AssertWriteSingleProperty(new NullableGuidGraph { Value = null });
        }

        [Fact]
        public void WriteNullableEnumTest()
        {
            _context.AssertWriteSingleProperty(new NullableEnumGraph { Value = null });
        }

    }
}