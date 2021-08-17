/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Serialization;

namespace Aeter.Ratio.Test.Serialization.Fakes
{
    public interface IWriteStatistics
    {
        int VisitCount { get; }
        int LeaveCount { get; }
        int VisitByteCount { get; }
        int VisitInt16Count { get; }
        int VisitInt32Count { get; }
        int VisitInt64Count { get; }
        int VisitUInt16Count { get; }
        int VisitUInt32Count { get; }
        int VisitUInt64Count { get; }
        int VisitBooleanCount { get; }
        int VisitSingleCount { get; }
        int VisitDoubleCount { get; }
        int VisitDecimalCount { get; }
        int VisitTimeSpanCount { get; }
        int VisitDateTimeCount { get; }
        int VisitStringCount { get; }
        int VisitGuidCount { get; }
        int VisitBlobCount { get; }
        int VisitValueCount { get; }
        void AssertVisitOrderExact(params LevelType[] expectedLevels);
        void AssertVisitOrderBeginsWith(params LevelType[] expectedLevels);
    }
}