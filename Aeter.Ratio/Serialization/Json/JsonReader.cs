/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.IO;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Aeter.Ratio.Serialization.Json
{
    public class JsonReader(BinaryReadBuffer buffer, JsonEncoding encoding)
    {
        private readonly JsonNumberReader _numberReader = JsonNumberReader.Create(buffer, encoding);

        public JsonReader(Stream stream) : this(stream, JsonEncoding.UTF8) { }
        public JsonReader(Stream stream, JsonEncoding encoding) : this(new BinaryReadBuffer(4096, BinaryStream.Passthrough(stream)), encoding) { }
        public JsonReader(IBinaryReadStream stream) : this(stream, JsonEncoding.UTF8) { }
        public JsonReader(IBinaryReadStream stream, JsonEncoding encoding) : this(new BinaryReadBuffer(4096, stream), encoding) { }

        private bool IsLiteral(byte first, byte[] literalBytes, bool advance = true)
        {
            buffer.RequestSpace(encoding.BinaryFormat.MaxSize);
            if (first != literalBytes[0]) {
                return false;
            }
            if (literalBytes.Length == 1) {
                if (advance) buffer.Advance(1);
                return true;
            }
            if (literalBytes.Length == 2) {
                var b = buffer.PeekByte(1);
                if (b != literalBytes[1]) {
                    return false;
                }
                if (advance) buffer.Advance(2);
                return true;
            }
            for (var i = 1; i < literalBytes.Length; i++) {
                if (buffer.PeekByte(i) != literalBytes[i]) {
                    return false;
                }
            }
            if (advance) buffer.Advance(literalBytes.Length);
            return true;
        }

        private JsonLiteral ReadLiteral(bool advance)
        {
            var first = buffer.PeekByte();

            while (IsLiteral(first, encoding.Space, advance: true)
                   || IsLiteral(first, encoding.CarriageReturn, advance: true)
                   || IsLiteral(first, encoding.Newline, advance: true)
                   || IsLiteral(first, encoding.HorizontalTab, advance: true)) {

                first = buffer.PeekByte();
            }

            if (IsLiteral(first, encoding.ObjectBegin, advance)) {
                return JsonLiteral.ObjectBegin;
            }
            if (IsLiteral(first, encoding.ObjectEnd, advance)) {
                return JsonLiteral.ObjectEnd;
            }
            if (IsLiteral(first, encoding.ArrayBegin, advance)) {
                return JsonLiteral.ArrayBegin;
            }
            if (IsLiteral(first, encoding.ArrayEnd, advance)) {
                return JsonLiteral.ArrayEnd;
            }
            if (IsLiteral(first, encoding.Assignment, advance)) {
                return JsonLiteral.Assignment;
            }
            if (IsLiteral(first, encoding.Quote, advance)) {
                return JsonLiteral.Quote;
            }
            if (IsLiteral(first, encoding.Comma, advance)) {
                return JsonLiteral.Comma;
            }
            if (IsLiteral(first, encoding.Minus, advance: false)) {
                return JsonLiteral.Number;
            }
            if (IsLiteral(first, encoding.Zero, advance: false)) {
                return JsonLiteral.Number;
            }
            if (IsLiteral(first, encoding.One, advance: false)) {
                return JsonLiteral.Number;
            }
            if (IsLiteral(first, encoding.Two, advance: false)) {
                return JsonLiteral.Number;
            }
            if (IsLiteral(first, encoding.Three, advance: false)) {
                return JsonLiteral.Number;
            }
            if (IsLiteral(first, encoding.Four, advance: false)) {
                return JsonLiteral.Number;
            }
            if (IsLiteral(first, encoding.Five, advance: false)) {
                return JsonLiteral.Number;
            }
            if (IsLiteral(first, encoding.Six, advance: false)) {
                return JsonLiteral.Number;
            }
            if (IsLiteral(first, encoding.Seven, advance: false)) {
                return JsonLiteral.Number;
            }
            if (IsLiteral(first, encoding.Eight, advance: false)) {
                return JsonLiteral.Number;
            }
            if (IsLiteral(first, encoding.Nine, advance: false)) {
                return JsonLiteral.Number;
            }
            if (IsLiteral(first, encoding.Null, advance)) {
                return JsonLiteral.Null;
            }
            if (IsLiteral(first, encoding.True, advance)) {
                return JsonLiteral.True;
            }
            if (IsLiteral(first, encoding.False, advance)) {
                return JsonLiteral.False;
            }
            if (IsLiteral(first, encoding.Undefined, advance)) {
                return JsonLiteral.Undefined;
            }
            throw UnexpectedJsonException.From("literal or value", buffer, encoding);
        }

        public JsonLiteral ReadLiteral()
        {
            return ReadLiteral(advance: true);
        }

        public JsonLiteral PeekLiteral()
        {
            return ReadLiteral(advance: false);
        }

        private bool IsNextCharacter(byte[] charCode, int offset)
        {
            if (buffer.Span[offset] != charCode[0]) {
                return false;
            }
            if (charCode.Length == 1) {
                return true;
            }
            if (buffer.Span[offset + 1] != charCode[1]) {
                return false;
            }
            if (charCode.Length == 2) {
                return true;
            }
            if (buffer.Span[offset + 2] != charCode[2]) {
                return false;
            }
            if (charCode.Length == 3) {
                return true;
            }
            return buffer.Span[offset + 3] == charCode[3];
        }

        private void AppendString(StringBuilder b, int offset)
        {
            if (buffer.Position == offset) return;
            var length = offset - buffer.Position;
            var str = encoding.BaseEncoding.GetString(buffer.Span.Slice(buffer.Position, length));
            b.Append(str);
            buffer.Advance(length);
        }

        public string ReadString()
        {
            return ReadString(expectStartToken: true);
        }

        private string ReadString(bool expectStartToken)
        {
            if (expectStartToken) {
                if (IsNextCharacter(encoding.Quote, buffer.Position)) {
                    buffer.Advance(encoding.Quote.Length);
                }
                else {
                    throw UnexpectedJsonException.From("\"", buffer, encoding);
                }
            }

            var b = new StringBuilder();
            var offset = buffer.Position;

            do {
                if ((buffer.Length - offset) < encoding.BinaryFormat.MaxSize*2) {
                    AppendString(b, offset);
                    buffer.RequestSpace(encoding.BinaryFormat.MaxSize*2);
                    offset = buffer.Position;
                }
                if (IsNextCharacter(encoding.Quote, offset)) {
                    AppendString(b, offset);
                    buffer.Advance(encoding.Quote.Length);
                    return b.ToString();
                }
                if (IsNextCharacter(encoding.ReverseSolidus, offset)) {
                    AppendString(b, offset);
                    offset += encoding.ReverseSolidus.Length;
                    if (IsNextCharacter(encoding.Backspace, offset)) {
                        b.Append('\b');
                    }
                    else if (IsNextCharacter(encoding.ReverseSolidus, offset)) {
                        b.Append('\\');
                    }
                    else if (IsNextCharacter(encoding.Quote, offset)) {
                        b.Append('\"');
                    }
                    else if (IsNextCharacter(encoding.CarriageReturn, offset)) {
                        b.Append('\r');
                    }
                    else if (IsNextCharacter(encoding.Formfeed, offset)) {
                        b.Append('\f');
                    }
                    else if (IsNextCharacter(encoding.HorizontalTab, offset)) {
                        b.Append('\t');
                    }
                    else if (IsNextCharacter(encoding.Newline, offset)) {
                        b.Append('\n');
                    }
                    else if (IsNextCharacter(encoding.Solidus, offset)) {
                        b.Append('/');
                    }
                    else {
                        throw UnexpectedJsonException.From("escaped character.", buffer, encoding);
                    }

                }
                offset += encoding.GetCharacterSize(buffer.Span, offset);
            } while (true);

        }

        private JsonObject ReadObject(bool expectStartToken)
        {
            var obj = new JsonObject();

            while (ReadField(out var fieldName, out var node)) {
                obj.Add(fieldName, node);
            }

            return obj;
        }

        private JsonNumber ReadNumber()
        {
            var numberSize = encoding.Zero.Length;
            buffer.RequestSpace(16 * numberSize);
            ulong val = 0;
            double dec = 0, decMultiplier = 1;
            byte next = 0;
            var isDecimal = false;
            var isNegative = false;
            while (_numberReader.ReadNext(ref next)) {
                if (next == JsonNumberReader.Negative) {
                    isNegative = true;
                    continue;
                }
                if (next == JsonNumberReader.Decimal) {
                    isDecimal = true;
                    continue;
                }

                if (isDecimal) {
                    dec = dec * 10 + next;
                    decMultiplier *= 10;
                }
                else {
                    val = val * 10 + next;
                }
            }
            decimal number = val;
            if (isDecimal && dec > 0) {
                number += (decimal)(dec / decMultiplier);
            }
            if (isNegative) {
                number *= -1;
            }
            return new JsonNumber(number);
        }

        private JsonArray ReadArray(bool expectStartToken)
        {
            var arr = new JsonArray();

            var literal = ReadLiteral();
            if (expectStartToken && literal != JsonLiteral.ArrayBegin) {
                throw UnexpectedJsonException.InArray(literal);
            }

            while (literal != JsonLiteral.ArrayEnd) {
                switch (literal) {
                    case JsonLiteral.ObjectBegin:
                        var obj = ReadObject(expectStartToken: false);
                        arr.Add(obj);
                        continue;
                    case JsonLiteral.ArrayBegin:
                        var arrInArr = ReadArray(expectStartToken: false);
                        arr.Add(arrInArr);
                        continue;
                    case JsonLiteral.Quote:
                        var value = ReadString(expectStartToken: false);
                        arr.Add(new JsonString(value));
                        continue;
                    case JsonLiteral.Number:
                        break;
                    case JsonLiteral.Null:
                        break;
                    case JsonLiteral.True:
                        break;
                    case JsonLiteral.False:
                        break;
                    case JsonLiteral.Undefined:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                literal = ReadLiteral();
                if (literal == JsonLiteral.Comma) {
                    literal = ReadLiteral();
                    if (literal == JsonLiteral.ArrayEnd) {
                        throw UnexpectedJsonException.InArray(JsonLiteral.Comma);
                    }
                }
                else if (literal != JsonLiteral.ArrayEnd) {
                    throw UnexpectedJsonException.InArray(literal);
                }
            }

            return arr;
        }

        public bool ReadFieldName([MaybeNullWhen(false)] out string name)
        {
            var literal = ReadLiteral();
            if (literal == JsonLiteral.Comma) {
                literal = ReadLiteral();
            }
            if (literal == JsonLiteral.ObjectEnd) {
                name = null;
                return false;
            }
            if (literal != JsonLiteral.Quote) {
                throw UnexpectedJsonException.InObject(literal);
            }
            name = ReadString(expectStartToken: false);
            return true;
        }

        public bool ReadFieldValue(out IJsonNode node)
        {
            var literal = ReadLiteral();
            if (literal != JsonLiteral.Assignment) {
                throw UnexpectedJsonException.InObject(literal);
            }

            literal = ReadLiteral();
            switch (literal) {
                case JsonLiteral.ObjectBegin:
                    node = ReadObject(expectStartToken: false);
                    break;
                case JsonLiteral.ArrayBegin:
                    node = ReadArray(expectStartToken: false);
                    break;
                case JsonLiteral.Quote:
                    var value = ReadString(expectStartToken: false);
                    node = new JsonString(value);
                    break;
                case JsonLiteral.Number:
                    node = ReadNumber();
                    break;
                case JsonLiteral.Null:
                    node = JsonNull.Instance;
                    break;
                case JsonLiteral.True:
                    node = JsonBool.True;
                    break;
                case JsonLiteral.False:
                    node = JsonBool.False;
                    break;
                case JsonLiteral.Undefined:
                    node = JsonUndefined.Instance;
                    break;
                default:
                    throw UnexpectedJsonException.InObject(literal);
            }
            return true;
        }

        public bool ReadField([MaybeNullWhen(false)] out string name, [MaybeNullWhen(false)] out IJsonNode node)
        {
            if (!ReadFieldName(out name)) {
                node = null;
                return false;
            }
            return ReadFieldValue(out node);
        }

        public IJsonNode ReadValue(JsonLiteral literal)
        {
            switch (literal) {
                case JsonLiteral.Quote:
                    return new JsonString(ReadString(expectStartToken: false));
                case JsonLiteral.Number:
                    return ReadNumber();
                case JsonLiteral.Null:
                    return JsonNull.Instance;
                case JsonLiteral.True:
                    return JsonBool.True;
                case JsonLiteral.False:
                    return JsonBool.False;
                case JsonLiteral.Undefined:
                    return JsonUndefined.Instance;
                default:
                    throw UnexpectedJsonException.From("value token", buffer, encoding);
            }
        }
    }
}
