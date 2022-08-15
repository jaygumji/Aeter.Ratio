/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Serialization
{
    public interface IWriteVisitor
    {
        void Visit(object? level, VisitArgs args);
        void Leave(object? level, VisitArgs args);

        void VisitValue(Byte? value, VisitArgs args);
        void VisitValue(Int16? value, VisitArgs args);
        void VisitValue(Int32? value, VisitArgs args);
        void VisitValue(Int64? value, VisitArgs args);
        void VisitValue(UInt16? value, VisitArgs args);
        void VisitValue(UInt32? value, VisitArgs args);
        void VisitValue(UInt64? value, VisitArgs args);
        void VisitValue(Boolean? value, VisitArgs args);
        void VisitValue(Single? value, VisitArgs args);
        void VisitValue(Double? value, VisitArgs args);
        void VisitValue(Decimal? value, VisitArgs args);
        void VisitValue(TimeSpan? value, VisitArgs args);
        void VisitValue(DateTime? value, VisitArgs args);
        void VisitValue(String? value, VisitArgs args);
        void VisitValue(Guid? value, VisitArgs args);
        void VisitValue(byte[]? value, VisitArgs args);
    }
}