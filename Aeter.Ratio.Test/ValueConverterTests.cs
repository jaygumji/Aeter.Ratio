/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using Xunit;

namespace Aeter.Ratio.Test
{
    public class ValueConverterTests
    {
        private static readonly Guid SampleGuid = Guid.Parse("8e5609a4-5b27-4d8a-927c-6c61d7d7d77a");
        private static readonly DateTime SampleDateTime = DateTime.SpecifyKind(new DateTime(2023, 03, 15, 10, 30, 45), DateTimeKind.Utc);
        private static readonly DateOnly SampleDateOnly = new DateOnly(2023, 03, 15);
        private static readonly TimeOnly SampleTimeOnly = new TimeOnly(10, 30, 45);

        public static IEnumerable<object[]> RoundTripData()
        {
            yield return new object[] { 42, typeof(int) };
            yield return new object[] { 42L, typeof(long) };
            yield return new object[] { 3.14m, typeof(decimal) };
            yield return new object[] { 2.71828, typeof(double) };
            yield return new object[] { true, typeof(bool) };
            yield return new object[] { 'A', typeof(char) };
            yield return new object[] { SampleGuid, typeof(Guid) };
            yield return new object[] { SampleDateTime, typeof(DateTime) };
            yield return new object[] { SampleDateOnly, typeof(DateOnly) };
            yield return new object[] { SampleTimeOnly, typeof(TimeOnly) };
        }

        [Theory]
        [MemberData(nameof(RoundTripData))]
        public void TextAndChangeType_RoundTrips(object value, Type targetType)
        {
            var text = ValueConverter.Text(value);

            var converted = ValueConverter.ChangeType(text, targetType);

            Assert.Equal(value, converted);
        }

        [Fact]
        public void TextAndChangeType_RoundTripsByteArray()
        {
            var bytes = new byte[] { 1, 2, 3, 4, 5 };
            var text = ValueConverter.Text(bytes);

            var converted = (byte[])ValueConverter.ChangeType(text, typeof(byte[]))!;

            Assert.Equal(bytes, converted);
        }

        [Fact]
        public void ChangeType_DateTimeToDateOnly()
        {
            var result = ValueConverter.ChangeType(SampleDateTime, typeof(DateOnly));

            Assert.Equal(DateOnly.FromDateTime(SampleDateTime), result);
        }

        [Fact]
        public void ChangeType_DateTimeToTimeOnly()
        {
            var result = ValueConverter.ChangeType(SampleDateTime, typeof(TimeOnly));

            Assert.Equal(TimeOnly.FromDateTime(SampleDateTime), result);
        }

        [Fact]
        public void ChangeType_TimeSpanToTimeOnly()
        {
            var timeSpan = new TimeSpan(10, 30, 0);

            var result = ValueConverter.ChangeType(timeSpan, typeof(TimeOnly));

            Assert.Equal(TimeOnly.FromTimeSpan(timeSpan), result);
        }

        [Fact]
        public void Text_UsesImplicitStringOperator()
        {
            var custom = new CustomStringValue("hello");

            var text = ValueConverter.Text(custom);

            Assert.Equal("hello", text);
        }

        [Fact]
        public void ChangeType_UsesImplicitOperator()
        {
            var result = ValueConverter.ChangeType("world", typeof(CustomStringValue));

            var converted = Assert.IsType<CustomStringValue>(result);
            Assert.Equal("world", converted.Value);
        }

        [Fact]
        public void ToBase16String_ReturnsNullForNullInput()
        {
            Assert.Null(ValueConverter.ToBase16String(null));
        }

        [Fact]
        public void ToBase16String_ReturnsEmptyStringForEmptyArray()
        {
            var result = ValueConverter.ToBase16String(Array.Empty<byte>());

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ToBase16String_EncodesBytesAsUppercaseHex()
        {
            var bytes = new byte[] { 0x00, 0xAF, 0x10, 0x5B };

            var result = ValueConverter.ToBase16String(bytes);

            Assert.Equal("00AF105B", result);
        }

        private sealed class CustomStringValue
        {
            public CustomStringValue(string value)
            {
                Value = value;
            }

            public string Value { get; }

            public static implicit operator string(CustomStringValue value) => value.Value;
            public static implicit operator CustomStringValue(string value) => new CustomStringValue(value);
        }
    }
}
