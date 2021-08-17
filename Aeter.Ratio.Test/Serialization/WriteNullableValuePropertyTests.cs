/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using Aeter.Ratio.Testing.Fakes.Entities;
using Aeter.Ratio.Testing.Fakes.Graphs;
using Xunit;


namespace Aeter.Ratio.Test.Serialization
{
    
    public class WriteNullableValuePropertyTests
    {

        private readonly TravellerTestContext _context = new TravellerTestContext();

        [Fact]
        public void WriteNullableInt16Test()
        {
            _context.AssertWriteSingleProperty(new NullableInt16Graph { Value = 42 });
        }

        [Fact]
        public void WriteNullableInt32Test()
        {
            _context.AssertWriteSingleProperty(new NullableInt32Graph { Value = 42 });
        }

        [Fact]
        public void WriteNullableInt64Test()
        {
            _context.AssertWriteSingleProperty(new NullableInt64Graph { Value = 42 });
        }

        [Fact]
        public void WriteNullableUInt16Test()
        {
            _context.AssertWriteSingleProperty(new NullableUInt16Graph { Value = 42 });
        }

        [Fact]
        public void WriteNullableUInt32Test()
        {
            _context.AssertWriteSingleProperty(new NullableUInt32Graph { Value = 42 });
        }

        [Fact]
        public void WriteNullableUInt64Test()
        {
            _context.AssertWriteSingleProperty(new NullableUInt64Graph { Value = 42 });
        }

        [Fact]
        public void WriteNullableBooleanTest()
        {
            _context.AssertWriteSingleProperty(new NullableBooleanGraph { Value = true });
        }

        [Fact]
        public void WriteNullableSingleTest()
        {
            _context.AssertWriteSingleProperty(new NullableSingleGraph { Value = 42.3f });
        }

        [Fact]
        public void WriteNullableDoubleTest()
        {
            _context.AssertWriteSingleProperty(new NullableDoubleGraph { Value = 42.7d });
        }

        [Fact]
        public void WriteNullableDecimalTest()
        {
            _context.AssertWriteSingleProperty(new NullableDecimalGraph { Value = 42.5434M });
        }

        [Fact]
        public void WriteNullableTimeSpanTest()
        {
            _context.AssertWriteSingleProperty(new NullableTimeSpanGraph { Value = new TimeSpan(12,30,00) });
        }

        [Fact]
        public void WriteNullableDateTimeTest()
        {
            _context.AssertWriteSingleProperty(new NullableDateTimeGraph { Value = new DateTime(2001, 01, 07, 15, 30, 24) });
        }

        [Fact]
        public void WriteNullableGuidTest()
        {
            _context.AssertWriteSingleProperty(new NullableGuidGraph { Value = Guid.Empty });
        }

        [Fact]
        public void WriteNullableEnumTest()
        {
            _context.AssertWriteSingleProperty(new NullableEnumGraph { Value = ApplicationType.Api });
        }

    }

}