/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Linq;
using Aeter.Ratio.Test.Serialization.Fakes;
using Aeter.Ratio.Testing.Fakes.Entities;
using Xunit;


namespace Aeter.Ratio.Test.Serialization.Binary
{
    
    public class PackedDataSerializerDataTests
    {

        private readonly BinarySerializationTestContext _context = new BinarySerializationTestContext();

        [Fact]
        public void WriteAndReadNullableValuesTest()
        {
            var graph = new NullableValuesEntity {
                Id = 1,
                MayBool = null,
                MayDateTime = null,
                MayInt = 44,
                MayTimeSpan = new TimeSpan(22, 30, 10)
            };
            var actual = _context.SerializeAndDeserialize(graph);

            Assert.NotNull(actual);
            Assert.Equal(1, actual.Id);
            Assert.Null(actual.MayBool);
            Assert.Null(actual.MayDateTime);
            Assert.Equal(44, actual.MayInt);
            Assert.Equal(new TimeSpan(22, 30, 10), actual.MayTimeSpan);
        }

        [Fact]
        public void ValueDictionaryTest()
        {
            var graph = new ValueDictionary {
                Test = new Dictionary<string, int> {
                    {"Test1", 1},
                    {"Test2", 2},
                    {"Test3", 3},
                }
            };

            var actual = _context.SerializeAndDeserialize(graph);

            Assert.NotNull(actual);
            Assert.NotNull(actual.Test);
            Assert.Equal(3, actual.Test.Count);

            Assert.True(graph.Test.SequenceEqual(actual.Test, new ValueDictionaryComparer()));
        }


        [Fact]
        public void ComplexDictionaryTest()
        {
            var graph = new ComplexDictionary {
                Test = new Dictionary<Identifier, Category> {
                    {new Identifier {Id = 1, Type = ApplicationType.Api}, new Category {Name = "Warning", Description = "Warning of something", Image = new byte[]{1, 2, 3, 4, 5}}},
                    {new Identifier {Id = 2, Type = ApplicationType.Api}, new Category {Name = "Error", Description = "Error of something", Image = new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9}}},
                    {new Identifier {Id = 3, Type = ApplicationType.Service}, new Category {Name = "Temporary"}}
                }
            };

            var actual = _context.SerializeAndDeserialize(graph);

            Assert.NotNull(actual);
            Assert.NotNull(actual.Test);
            Assert.Equal(3, actual.Test.Count);

            Assert.True(graph.Test.Keys.SequenceEqual(actual.Test.Keys));
            Assert.True(graph.Test.Values.SequenceEqual(actual.Test.Values));
        }

        [Fact]
        public void IdentifierTest()
        {
            var graph = new Identifier { Id = 1, Type = ApplicationType.Api };

            var actual = _context.SerializeAndDeserialize(graph);

            Assert.NotNull(actual);
            Assert.Equal(graph.Id, actual.Id);
            Assert.Equal(graph.Type, actual.Type);
        }

    }

}
