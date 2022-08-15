/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Globalization;
using System.Text;
using Aeter.Ratio.Binary.Converters;

namespace Aeter.Ratio.Serialization.Bson
{
    public class BsonEncoding
    {
        public static readonly BsonEncoding UTF8 = new BsonEncoding(Encoding.UTF8, new EncodingBinaryFormat(
            minSize: 1,
            maxSize: 4,
            sizeIncrement: 1,
            expandCodes: new byte[] { 0xc2, 0xe0, 0xf0 },
            markerOffset: 0
        ));

        public static readonly IFormatProvider NumberFormat =
            new NumberFormatInfo {
                NaNSymbol = "NaN",
                NegativeSign = "-",
                NumberDecimalDigits = 10,
                NumberDecimalSeparator = "."
            };

        public const byte ZeroTermination = 0x00;

        public static readonly IFormatProvider DateTimeFormat = CultureInfo.InvariantCulture;

        public readonly Base64Converter Base64;
        public readonly Encoding BaseEncoding;
        public readonly IEncodingBinaryFormat BinaryFormat;

        public BsonEncoding(Encoding baseEncoding, IEncodingBinaryFormat binaryFormat)
        {
            BaseEncoding = baseEncoding;
            BinaryFormat = binaryFormat;
            Base64 = new Base64Converter(baseEncoding);
        }

        public int GetCharacterSize(byte[] buffer, int offset)
        {
            if (offset + BinaryFormat.MinSize >= buffer.Length) {
                throw new IndexOutOfRangeException("The buffer does not contain the full character code.");
            }

            if (BinaryFormat.ExpandCodes == null || BinaryFormat.ExpandCodes.Length == 0) {
                return BinaryFormat.MinSize;
            }

            var markerOffset = offset + BinaryFormat.MarkerOffset;
            var length = BinaryFormat.MinSize;
            if (buffer[markerOffset] < BinaryFormat.ExpandCodes[0]) {
                return length;
            }
            length += BinaryFormat.SizeIncrement;
            if (BinaryFormat.ExpandCodes.Length == 1 || buffer[markerOffset] < BinaryFormat.ExpandCodes[1]) {
                return length;
            }
            length += BinaryFormat.SizeIncrement;
            if (BinaryFormat.ExpandCodes.Length == 2 || buffer[markerOffset] < BinaryFormat.ExpandCodes[2]) {
                return length;
            }
            length += BinaryFormat.SizeIncrement;
            return length;
        }

    }
}