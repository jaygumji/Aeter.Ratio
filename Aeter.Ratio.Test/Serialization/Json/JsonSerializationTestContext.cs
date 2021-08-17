/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aeter.Ratio.Binary;
using Aeter.Ratio.Serialization;
using Aeter.Ratio.Serialization.Json;
using Aeter.Ratio.Testing.Fakes.Graphs;
using Xunit;

namespace Aeter.Ratio.Test.Serialization.Json
{
    public class JsonSerializationTestContext : SerializationTestContext
    {
        private readonly JsonEncoding _encoding;
        private readonly IFieldNameResolver _fieldNameResolver;

        public JsonSerializationTestContext()
        {
            _encoding = JsonEncoding.UTF16LE;
            _fieldNameResolver = new CamelCaseFieldNameResolver();
        }

        public void AssertWriteVisitorCall(string expected, Action<JsonWriteVisitor> action)
        {
            using (var memStream = new MemoryStream()) {
                var buffer = new BinaryWriteBuffer(1024, memStream);
                var writeVisitor = new JsonWriteVisitor(_encoding, _fieldNameResolver, buffer, new Stack<bool>(new [] {true}));
                action(writeVisitor);
                buffer.Flush();
                var actual = _encoding.BaseEncoding.GetString(memStream.ToArray());
                Assert.Equal(expected, actual);
            }
        }

        public void MakeReadVisitorCall(string json, Action<JsonReadVisitor> action)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            using (var memStream = new MemoryStream(bytes)) {
                var buffer = new BinaryReadBuffer(1024, memStream);
                var readVisitor = new JsonReadVisitor(_encoding, _fieldNameResolver, buffer);
                action(readVisitor);
            }
        }

        public void AssertDeserialize<T>(string json, T expected)
            where T : IComparable<T>
        {
            var cmp = new DelegatedComparer<T>((l, r) => l.CompareTo(r));
            AssertDeserialize(json, expected, cmp);
        }

        public void AssertDeserializeSingle<T, TValue>(string json, T expected)
            where T : ISingleValueGraph<TValue>
            where TValue : IComparable<TValue>
        {
            var cmp = new DelegatedComparer<T>((l, r) => {
                if (l == null && r == null) return 0;
                if (l == null) return -1;
                if (r == null) return 1;
                return l.Value.CompareTo(r.Value);
            });
            AssertDeserialize(json, expected, cmp);
        }

        public void AssertDeserializeNullableSingle<T, TValue>(string json, T expected)
            where T : ISingleNullableValueGraph<TValue>
            where TValue : struct, IComparable<TValue>
        {
            var cmp = new DelegatedComparer<T>((l, r) => {
                if (l?.Value == null && r?.Value == null) return 0;
                if (l?.Value == null) return -1;
                if (r?.Value == null) return 1;
                return l.Value.Value.CompareTo(r.Value.Value);
            });
            AssertDeserialize(json, expected, cmp);
        }

        public void AssertDeserialize<T>(string json, T expected, CompareHandler<T> comparer)
        {
            var cmp = new DelegatedComparer<T>(comparer);
            AssertDeserialize(json, expected, cmp);
        }

        public void AssertDeserialize<T>(string json, T expected, IEqualityComparer<T> comparer)
        {
            var serializer = new JsonSerializer<T>();
            var actual = serializer.Deserialize(json);
            Assert.Equal(expected, actual, comparer);
        }

        public void AssertSerialize<T>(string expected, T graph)
        {
            var bytes = Serialize(graph);
            var jsonString = _encoding.BaseEncoding.GetString(bytes);
            Assert.Equal(expected, jsonString);
        }

        protected override ITypedSerializer<T> CreateSerializer<T>()
        {
            return new JsonSerializer<T> {
                Encoding = _encoding,
                FieldNameResolver = _fieldNameResolver
            };
        }
    }
}