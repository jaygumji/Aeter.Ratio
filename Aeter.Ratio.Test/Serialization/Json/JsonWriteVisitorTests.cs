/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using Aeter.Ratio.Serialization;
using Xunit;

namespace Aeter.Ratio.Test.Serialization.Json
{
    public class JsonWriteVisitorTests
    {

        private readonly JsonSerializationTestContext _context = new JsonSerializationTestContext();

        [Fact]
        public void VisitEnterRoot()
        {
            _context.AssertWriteVisitorCall("{", v => v.Visit(new {}, VisitArgs.CreateRoot(LevelType.Single)));
        }

        [Fact]
        public void VisitEnterCollection()
        {
            _context.AssertWriteVisitorCall("\"col\":[", v => v.Visit(new int[] {}, new VisitArgs("Col", LevelType.Collection)));
        }

        [Fact]
        public void VisitEnterCollectionInCollection()
        {
            _context.AssertWriteVisitorCall("[", v => v.Visit(new int[] { }, VisitArgs.CollectionInCollection));
        }

        [Fact]
        public void VisitEnterCollectionInDictionaryKey()
        {
            Assert.Throws<NotSupportedException>(() => {
                _context.AssertWriteVisitorCall("[", v => v.Visit(new int[] { }, VisitArgs.CollectionInDictionaryKey));
            });
        }

        [Fact]
        public void VisitEnterCollectionInDictionaryValue()
        {
            _context.AssertWriteVisitorCall("[", v => v.Visit(new int[] { }, VisitArgs.CollectionInDictionaryValue));
        }

        [Fact]
        public void VisitEnterCollectionItem()
        {
            _context.AssertWriteVisitorCall("[1,2", v => {
                v.Visit(new int[] {}, VisitArgs.CollectionInCollection);
                v.VisitValue(1, VisitArgs.CollectionItem);
                v.VisitValue(2, VisitArgs.CollectionItem);
            });
        }

        [Fact]
        public void VisitEnterDictionary()
        {
            _context.AssertWriteVisitorCall("\"dict\":{", v => v.Visit(new int[] { }, new VisitArgs("Dict", LevelType.Dictionary)));
        }

        [Fact]
        public void VisitEnterDictionaryKeyWithSingle()
        {
            Assert.Throws<NotSupportedException>(() => {
                _context.AssertWriteVisitorCall("{", v => v.Visit(new { }, VisitArgs.DictionaryKey));
            });
        }

        [Fact]
        public void VisitEnterDictionaryKeyWithInt32()
        {
            _context.AssertWriteVisitorCall("\"42\":", v => v.VisitValue(42, VisitArgs.DictionaryKey));
        }

        [Fact]
        public void VisitEnterDictionaryValue()
        {
            _context.AssertWriteVisitorCall("{", v => v.Visit(new { }, VisitArgs.DictionaryValue));
        }

        [Fact]
        public void VisitEnterDictionaryInCollection()
        {
            _context.AssertWriteVisitorCall("{", v => v.Visit(new int[] { }, VisitArgs.DictionaryInCollection));
        }

        [Fact]
        public void VisitEnterDictionaryInDictionaryKey()
        {
            Assert.Throws<NotSupportedException>(() => {
                _context.AssertWriteVisitorCall("{", v => v.Visit(new int[] { }, VisitArgs.DictionaryInDictionaryKey));
            });
        }
        [Fact]
        public void VisitEnterDictionaryInDictionaryValue()
        {
            _context.AssertWriteVisitorCall("{", v => v.Visit(new int[] { }, VisitArgs.DictionaryInDictionaryValue));
        }

        [Fact]
        public void VisitEnterSingle()
        {
            _context.AssertWriteVisitorCall("\"single\":{", v => v.Visit(new {}, new VisitArgs("Single", LevelType.Single)));
        }

        [Fact]
        public void VisitEnterSingleNull()
        {
            _context.AssertWriteVisitorCall("\"single\":null", v => v.Visit(null, new VisitArgs("Single", LevelType.Single)));
        }

        [Fact]
        public void VisitValueInt16()
        {
            _context.AssertWriteVisitorCall("\"value\":1", v => v.VisitValue((short?)1, new VisitArgs("Value", LevelType.Value)));
        }

        [Fact]
        public void VisitValueInt32()
        {
            _context.AssertWriteVisitorCall("\"value\":1", v => v.VisitValue((int?)1, new VisitArgs("Value", LevelType.Value)));
        }

        [Fact]
        public void VisitValueInt64()
        {
            _context.AssertWriteVisitorCall("\"value\":1", v => v.VisitValue((long?)1, new VisitArgs("Value", LevelType.Value)));
        }

        [Fact]
        public void VisitValueUIn16()
        {
            _context.AssertWriteVisitorCall("\"value\":1", v => v.VisitValue((ushort?)1, new VisitArgs("Value", LevelType.Value)));
        }

        [Fact]
        public void VisitValueUInt32()
        {
            _context.AssertWriteVisitorCall("\"value\":1", v => v.VisitValue((uint?)1, new VisitArgs("Value", LevelType.Value)));
        }

        [Fact]
        public void VisitValueUInt64()
        {
            _context.AssertWriteVisitorCall("\"value\":1", v => v.VisitValue((ulong?)1, new VisitArgs("Value", LevelType.Value)));
        }

        [Fact]
        public void VisitValueFloat()
        {
            _context.AssertWriteVisitorCall("\"value\":1.1", v => v.VisitValue((float?)1.1, new VisitArgs("Value", LevelType.Value)));
        }

        [Fact]
        public void VisitValueDouble()
        {
            _context.AssertWriteVisitorCall("\"value\":1.123", v => v.VisitValue((double?)1.123, new VisitArgs("Value", LevelType.Value)));
        }

        [Fact]
        public void VisitValueDecimal()
        {
            _context.AssertWriteVisitorCall("\"value\":12.12345", v => v.VisitValue((decimal?)12.12345, new VisitArgs("Value", LevelType.Value)));
        }

        [Fact]
        public void VisitValueGuid()
        {
            var guid = Guid.NewGuid();
            _context.AssertWriteVisitorCall($"\"value\":\"{guid}\"", v => v.VisitValue(guid, new VisitArgs("Value", LevelType.Value)));
        }

        [Fact]
        public void VisitValueTimeSpan()
        {
            _context.AssertWriteVisitorCall($"\"value\":\"10.12:30:42\"", v => v.VisitValue(new TimeSpan(10, 12, 30, 42), new VisitArgs("Value", LevelType.Value)));
        }

        [Fact]
        public void VisitValueDateTime()
        {
            _context.AssertWriteVisitorCall($"\"value\":\"2020-02-20T20:20:20.2020000\"", v => v.VisitValue(new DateTime(2020, 02, 20, 20, 20, 20, 202), new VisitArgs("Value", LevelType.Value)));
        }

        [Fact]
        public void VisitValueBool()
        {
            _context.AssertWriteVisitorCall("\"value\":true", v => v.VisitValue(true, new VisitArgs("Value", LevelType.Value)));
        }

    }
}