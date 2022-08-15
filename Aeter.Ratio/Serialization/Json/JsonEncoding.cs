/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Aeter.Ratio.Binary;
using Aeter.Ratio.Binary.Converters;

namespace Aeter.Ratio.Serialization.Json
{
    public class JsonEncoding
    {

        public static readonly JsonEncoding UTF16BE = new JsonEncoding(Encoding.BigEndianUnicode, new EncodingBinaryFormat(
            minSize: 2,
            maxSize: 4,
            sizeIncrement: 2,
            expandCodes: new byte[] { 0xd8 },
            markerOffset: 0
        ));
        public static readonly JsonEncoding UTF16LE = new JsonEncoding(Encoding.Unicode, new EncodingBinaryFormat(
            minSize: 2,
            maxSize: 4,
            sizeIncrement: 2,
            expandCodes: new byte[] {0xd8},
            markerOffset: 1
        ));
        public static readonly JsonEncoding UTF8 = new JsonEncoding(Encoding.UTF8, new EncodingBinaryFormat(
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

        public static readonly IFormatProvider DateTimeFormat = CultureInfo.InvariantCulture;

        public readonly Base64Converter Base64;
        public readonly Encoding BaseEncoding;
        public readonly IEncodingBinaryFormat BinaryFormat;

        public readonly byte[] ObjectBegin;
        public readonly byte[] ObjectEnd;

        public readonly byte[] ArrayBegin;
        public readonly byte[] ArrayEnd;

        public readonly byte[] Quote;
        public readonly byte[] Assignment;
        public readonly byte[] Comma;
        public readonly byte[] ReverseSolidus;
        public readonly byte[] Solidus;
        public readonly byte[] Backspace;
        public readonly byte[] Formfeed;
        public readonly byte[] Newline;
        public readonly byte[] CarriageReturn;
        public readonly byte[] HorizontalTab;

        public readonly byte[] Undefined;
        public readonly byte[] Null;
        public readonly byte[] True;
        public readonly byte[] False;

        public readonly byte[] Minus;
        public readonly byte[] Zero;
        public readonly byte[] One;
        public readonly byte[] Two;
        public readonly byte[] Three;
        public readonly byte[] Four;
        public readonly byte[] Five;
        public readonly byte[] Six;
        public readonly byte[] Seven;
        public readonly byte[] Eight;
        public readonly byte[] Nine;
        public readonly byte[] Point;
        public readonly byte[] Space;

        public JsonEncoding(Encoding baseEncoding, IEncodingBinaryFormat binaryFormat)
        {
            BaseEncoding = baseEncoding;
            BinaryFormat = binaryFormat;
            ObjectBegin = baseEncoding.GetBytes("{");
            ObjectEnd = baseEncoding.GetBytes("}");
            ArrayBegin = baseEncoding.GetBytes("[");
            ArrayEnd = baseEncoding.GetBytes("]");
            Quote = baseEncoding.GetBytes("\"");
            Assignment = baseEncoding.GetBytes(":");
            Comma = baseEncoding.GetBytes(",");
            ReverseSolidus = baseEncoding.GetBytes("\\");
            Solidus = baseEncoding.GetBytes("/");
            Backspace = baseEncoding.GetBytes("\b");
            Formfeed = baseEncoding.GetBytes("\f");
            Newline = baseEncoding.GetBytes("\n");
            CarriageReturn = baseEncoding.GetBytes("\r");
            HorizontalTab = baseEncoding.GetBytes("\t");
            Undefined = baseEncoding.GetBytes("undefined");
            Null = baseEncoding.GetBytes("null");
            True = baseEncoding.GetBytes("true");
            False = baseEncoding.GetBytes("false");
            Minus = baseEncoding.GetBytes("-");
            Zero = baseEncoding.GetBytes("0");
            One = baseEncoding.GetBytes("1");
            Two = baseEncoding.GetBytes("2");
            Three = baseEncoding.GetBytes("3");
            Four = baseEncoding.GetBytes("4");
            Five = baseEncoding.GetBytes("5");
            Six = baseEncoding.GetBytes("6");
            Seven = baseEncoding.GetBytes("7");
            Eight = baseEncoding.GetBytes("8");
            Nine = baseEncoding.GetBytes("9");
            Point = baseEncoding.GetBytes(".");
            Space = baseEncoding.GetBytes(" ");

            Base64 = new Base64Converter(baseEncoding);
        }

        public bool RequiresEscape(char c, [MaybeNullWhen(false)] out byte[] charBytes)
        {
            switch (c) {
                case '\\':
                    charBytes = ReverseSolidus;
                    return true;
                case '\"':
                    charBytes = Quote;
                    return true;
                case '/':
                    charBytes = Solidus;
                    return true;
                case '\b':
                    charBytes = Backspace;
                    return true;
                case '\f':
                    charBytes = Formfeed;
                    return true;
                case '\n':
                    charBytes = Newline;
                    return true;
                case '\r':
                    charBytes = CarriageReturn;
                    return true;
                case '\t':
                    charBytes = HorizontalTab;
                    return true;
            }
            charBytes = null;
            return false;
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