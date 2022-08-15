/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.Serialization;
using Aeter.Ratio.Serialization.Bson;
using Aeter.Ratio.Testing.Fakes.Graphs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Aeter.Ratio.Test.Serialization.Bson
{
    public class BsonSerializationTestContext : SerializationTestContext
    {
        private readonly BsonEncoding _encoding;
        private readonly IFieldNameResolver _fieldNameResolver;

        public BsonSerializationTestContext()
        {
            _encoding = BsonEncoding.UTF8;
            _fieldNameResolver = new CamelCaseFieldNameResolver();
        }

        public void AssertDeserialize<T>(byte[] bson, T expected)
            where T : IComparable<T>
        {
            var cmp = new DelegatedComparer<T>((l, r) => l!.CompareTo(r));
            AssertDeserialize(bson, expected, cmp);
        }

        public void AssertDeserializeSingle<T, TValue>(byte[] bson, T expected)
            where T : ISingleValueGraph<TValue>
            where TValue : IComparable<TValue>
        {
            var cmp = new DelegatedComparer<T>((l, r) => {
                if (l == null && r == null) return 0;
                if (l == null) return -1;
                if (r == null) return 1;
                return l.Value!.CompareTo(r.Value);
            });
            AssertDeserialize(bson, expected, cmp);
        }

        public void AssertDeserializeNullableSingle<T, TValue>(byte[] bson, T expected)
            where T : ISingleNullableValueGraph<TValue>
            where TValue : struct, IComparable<TValue>
        {
            var cmp = new DelegatedComparer<T>((l, r) => {
                if (l?.Value == null && r?.Value == null) return 0;
                if (l?.Value == null) return -1;
                if (r?.Value == null) return 1;
                return l.Value.Value.CompareTo(r.Value.Value);
            });
            AssertDeserialize(bson, expected, cmp);
        }

        public void AssertDeserialize<T>(byte[] bson, T expected, CompareHandler<T> comparer)
        {
            var cmp = new DelegatedComparer<T>(comparer);
            AssertDeserialize(bson, expected, cmp);
        }

        public void AssertDeserialize<T>(byte[] bson, T expected, IEqualityComparer<T> comparer)
        {
            var serializer = new BsonSerializer<T>();
            var actual = serializer.Deserialize(bson);
            Assert.Equal(expected, actual, comparer);
        }

        public void AssertSerialize<T>(byte[] expected, T graph)
        {
            var bytes = Serialize(graph);
            Assert.Equal(expected, bytes);
        }

        protected override ITypedSerializer<T> CreateSerializer<T>()
        {
            return new BsonSerializer<T> {
                Encoding = _encoding,
                FieldNameResolver = _fieldNameResolver
            };
        }
    }
}