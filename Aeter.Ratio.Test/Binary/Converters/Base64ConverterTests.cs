/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aeter.Ratio.Binary.Converters;
using Xunit;

namespace Aeter.Ratio.Test.Binary.Converters
{
    public class Base64ConverterTests
    {

        [Fact]
        public void EncodeUtf8()
        {
            var converter = Base64Converter.UTF8;
            var source = Enumerable.Range(byte.MinValue, byte.MaxValue)
                .Select(i => (byte)i)
                .ToArray();
            var target = new byte[converter.GetEncodedSizeOf(source)];
            converter.Encode(source, 0, source.Length, target, 0);

            var base64 = Convert.ToBase64String(source);
            var actual = Encoding.UTF8.GetString(target);

            Assert.Equal(base64, actual);
        }

        [Fact]
        public void EncodeUtf16LE()
        {
            var converter = Base64Converter.UTF16LE;
            var source = Enumerable.Range(byte.MinValue, byte.MaxValue)
                .Select(i => (byte)i)
                .ToArray();
            var target = new byte[converter.GetEncodedSizeOf(source)];
            converter.Encode(source, 0, source.Length, target, 0);

            var base64 = Convert.ToBase64String(source);
            var actual = Encoding.Unicode.GetString(target);

            Assert.Equal(base64, actual);
        }

        [Fact]
        public void EncodeUtf16BE()
        {
            var converter = Base64Converter.UTF16BE;
            var source = Enumerable.Range(byte.MinValue, byte.MaxValue)
                .Select(i => (byte)i)
                .ToArray();
            var target = new byte[converter.GetEncodedSizeOf(source)];
            converter.Encode(source, 0, source.Length, target, 0);

            var base64 = Convert.ToBase64String(source);
            var actual = Encoding.BigEndianUnicode.GetString(target);

            Assert.Equal(base64, actual);
        }

        [Fact]
        public void EncodeUtf8WithPadding1()
        {
            var converter = Base64Converter.UTF8;
            var source = Enumerable.Range(1, 5)
                .Select(i => (byte)i)
                .ToArray();
            var target = new byte[converter.GetEncodedSizeOf(source)];
            converter.Encode(source, 0, source.Length, target, 0);

            var base64 = Convert.ToBase64String(source);
            var actual = Encoding.UTF8.GetString(target);

            Assert.Equal(base64, actual);
        }

        [Fact]
        public void DecodeUtf8()
        {
            var converter = Base64Converter.UTF8;
            var expected = Enumerable.Range(byte.MinValue, byte.MaxValue)
                .Select(i => (byte)i)
                .ToArray();

            var source = Encoding.UTF8.GetBytes(Convert.ToBase64String(expected));

            var target = new byte[converter.GetDecodedSizeOf(source)];
            converter.Decode(source, 0, source.Length, target, 0);

            Assert.Equal(expected, target, EqualityComparer<byte>.Default);
        }

        [Fact]
        public void DecodeUtf16LE()
        {
            var converter = Base64Converter.UTF16LE;
            var expected = Enumerable.Range(byte.MinValue, byte.MaxValue)
                .Select(i => (byte)i)
                .ToArray();

            var source = Encoding.Unicode.GetBytes(Convert.ToBase64String(expected));

            var target = new byte[converter.GetDecodedSizeOf(source)];
            converter.Decode(source, 0, source.Length, target, 0);

            Assert.Equal(expected, target, EqualityComparer<byte>.Default);
        }

        [Fact]
        public void DecodeUtf16BE()
        {
            var converter = Base64Converter.UTF16BE;
            var expected = Enumerable.Range(byte.MinValue, byte.MaxValue)
                .Select(i => (byte)i)
                .ToArray();

            var source = Encoding.BigEndianUnicode.GetBytes(Convert.ToBase64String(expected));

            var target = new byte[converter.GetDecodedSizeOf(source)];
            converter.Decode(source, 0, source.Length, target, 0);

            Assert.Equal(expected, target, EqualityComparer<byte>.Default);
        }

        [Fact]
        public void DecodeUtf8WithPadding1()
        {
            var converter = Base64Converter.UTF8;
            var expected = Enumerable.Range(1, 5)
                .Select(i => (byte)i)
                .ToArray();

            var source = Encoding.UTF8.GetBytes(Convert.ToBase64String(expected));

            var target = new byte[converter.GetDecodedSizeOf(source)];
            converter.Decode(source, 0, source.Length, target, 0);

            Assert.Equal(expected, target, EqualityComparer<byte>.Default);
        }

    }
}
