/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Testing.Fakes.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Aeter.Ratio.Test.Serialization.Json
{
    public class JsonDataBlock
    {
        public Guid Id { get; set; }
        public string? String { get; set; }
        public short Int16 { get; set; }
        public int Int32 { get; set; }
        public long Int64 { get; set; }
        public ushort UInt16 { get; set; }
        public uint UInt32 { get; set; }
        public ulong UInt64 { get; set; }
        public float Single { get; set; }
        public double Double { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public decimal Decimal { get; set; }
        public DateTime DateTime { get; set; }
        public byte Byte { get; set; }
        public bool Boolean { get; set; }
        public byte[]? Blob { get; set; }

        public ICollection<string>? Messages { get; set; }
        public IList<DateTime>? Stamps { get; set; }

        public Relation? Relation { get; set; }
        public Relation? DummyRelation { get; set; }
        public List<Relation>? SecondaryRelations { get; set; }

        public Dictionary<string, int>? IndexedValues { get; set; }
        public Dictionary<int, Category>? Categories { get; set; }

        public static JsonDataBlock Filled()
        {
            return new JsonDataBlock {
                Id = Guid.Parse("F5159142-B9A3-45FA-85AE-E0C9E60990A9"),
                Blob = new byte[] { 1, 2, 3 },
                Boolean = true,
                Byte = 42,
                DateTime = new DateTime(2014, 04, 01, 10, 00, 00),
                Decimal = 44754.324M,
                Double = 4357.32,
                Int16 = 20234,
                Int32 = 43554654,
                Int64 = 4349893895849554545,
                Messages = new Collection<string> { "Test1", "Test2", "Test3", "Test4", "Test5" },
                Single = 32.1f,
                String = "Hello World",
                TimeSpan = new TimeSpan(10, 30, 00),
                UInt16 = 64322,
                UInt32 = 3454654454,
                UInt64 = 9859459485984955454,
                Stamps = new[] { new DateTime(2010, 03, 01, 22, 00, 00) },
                Relation = new Relation {
                    Id = Guid.Parse("F68EF7D4-6F62-476B-BC5E-71AD86549A63"),
                    Name = "Connection",
                    Description = "Generic connection between relations",
                    Value = 77
                },
                DummyRelation = null,
                SecondaryRelations = new List<Relation> {
                    new Relation {
                        Id = Guid.Parse("C9EDB616-26EC-44BB-9E70-3F38C7C18C91"),
                        Name = "Line1",
                        Description = "First line of cascade",
                        Value = 187
                    }
                },
                IndexedValues = new Dictionary<string, int> {
                    {"V1", 1},
                    {"V2", 2},
                    {"V3", 3},
                    {"V4", 4}
                },
                Categories = new Dictionary<int, Category> {
                    {1, new Category {Name = "Warning", Description = "Warning of something", Image = new byte[]{1, 2, 3, 4, 5}}},
                    {2, new Category {Name = "Error", Description = "Error of something", Image = new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9}}},
                    {3, new Category {Name = "Temporary"}}
                }
            };
        }

        public void AssertEqualTo(JsonDataBlock expected)
        {
            Assert.Equal(expected.Id, Id);
            Assert.Equal(expected.Int16, Int16);
            Assert.Equal(expected.Int32, Int32);
            Assert.Equal(expected.Int64, Int64);
            Assert.Equal(expected.UInt16, UInt16);
            Assert.Equal(expected.UInt32, UInt32);
            Assert.Equal(expected.UInt64, UInt64);
            Assert.Equal(expected.Single, Single);
            Assert.Equal(expected.Double, Double);
            Assert.Equal(expected.Decimal, Decimal);
            Assert.Equal(expected.TimeSpan, TimeSpan);
            Assert.Equal(expected.DateTime, DateTime);
            Assert.Equal(expected.String, String);
            Assert.Equal(expected.Boolean, Boolean);
            Assert.Equal(expected.Byte, Byte);
            AssertExtensions.Equal(expected.Blob, Blob);

            AssertExtensions.Equal(expected.Messages, Messages);
            AssertExtensions.Equal(expected.Stamps, Stamps);

            Assert.NotNull(Relation);
            Assert.Equal(expected.Relation?.Id, Relation?.Id);
            Assert.Equal(expected.Relation?.Name, Relation?.Name);
            Assert.Equal(expected.Relation?.Description, Relation?.Description);
            Assert.Equal(expected.Relation?.Value, Relation?.Value);
            Assert.Null(DummyRelation);

            AssertExtensions.Equal(expected.IndexedValues?.Keys, IndexedValues?.Keys);
            AssertExtensions.Equal(expected.IndexedValues?.Values, IndexedValues?.Values);

            Assert.NotNull(Categories);
            Assert.Equal(3, Categories?.Count);
            AssertExtensions.Equal(expected.Categories?.Keys, Categories?.Keys);
            AssertExtensions.Equal(expected.Categories?.Values, Categories?.Values);
        }
    }
}
