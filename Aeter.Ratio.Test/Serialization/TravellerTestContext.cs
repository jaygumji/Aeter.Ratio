/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.DependencyInjection;
using Aeter.Ratio.Reflection;
using Aeter.Ratio.Serialization;
using Aeter.Ratio.Serialization.Reflection;
using Aeter.Ratio.Test.Serialization.Fakes;
using Xunit;

namespace Aeter.Ratio.Test.Serialization
{
    public class TravellerTestContext
    {
        public static readonly GraphTravellerProvider Provider = new GraphTravellerProvider(
            new DynamicGraphTravellerFactory(new DependencyInjectionContainer(), new SerializableTypeProvider(new SerializationReflectionInspector(), FactoryTypeProvider.Instance)));

        public IGraphTraveller<T> CreateTraveller<T>()
        {
            var traveller = Provider.Get<T>();
            return traveller;
        }

        public void AssertWriteSingleProperty<T>(T graph)
        {
            var stats = AssertWrite(1, graph);
            stats.AssertVisitOrderExact(LevelType.Value);
        }

        public void AssertReadSingleProperty<T>() where T : new()
        {
            var stats = AssertRead<T>(1);
            stats.AssertVisitOrderExact(LevelType.Value);
        }

        public void AssertReadSinglePropertyWithNull<T>() where T : new()
        {
            var stats = AssertRead<T>(1, -1, readOnlyNull: true);
            stats.AssertVisitOrderExact(LevelType.Value);
        }

        public IWriteStatistics AssertWrite<T>(int expectedValueWriteCount, T graph)
        {
            var visitor = new FakeWriteVisitor();
            var traveller = CreateTraveller<T>();
            traveller.Travel(visitor, graph);

            //_travellerContext.Save();
            visitor.Statistics.AssertHiearchy();
            Assert.Equal(expectedValueWriteCount, visitor.Statistics.VisitValueCount);

            return visitor.Statistics;
        }

        public IReadStatistics AssertRead<T>(int expectedValueReadCount) where T : new()
        {
            return AssertRead<T>(expectedValueReadCount, -1);
        }

        public IReadStatistics AssertRead<T>(int expectedValueReadCount, int allowedVisitCount) where T : new()
        {
            return AssertRead<T>(expectedValueReadCount, allowedVisitCount, readOnlyNull: false);
        }

        public IReadStatistics AssertRead<T>(int expectedValueReadCount, int allowedVisitCount, bool readOnlyNull) where T : new()
        {
            var visitor = new FakeReadVisitor { AllowedVisitCount = allowedVisitCount, ReadOnlyNull = readOnlyNull };
            var traveller = CreateTraveller<T>();

            var graph = new T();
            traveller.Travel(visitor, graph);

            visitor.Statistics.AssertHiearchy();
            Assert.Equal(expectedValueReadCount, visitor.Statistics.VisitValueCount);

            return visitor.Statistics;
        }

    }
}