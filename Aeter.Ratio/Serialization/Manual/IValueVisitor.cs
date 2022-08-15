/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Diagnostics.CodeAnalysis;

namespace Aeter.Ratio.Serialization.Manual
{
    public interface IValueVisitor
    {
        bool TryVisitValue(IReadVisitor visitor, VisitArgs args, [MaybeNullWhen(false)] out object value);
        void VisitValue(IWriteVisitor visitor, VisitArgs args, object value);
    }

    public interface IValueVisitor<T> : IValueVisitor
    {
        bool TryVisitValue(IReadVisitor visitor, VisitArgs args, [MaybeNullWhen(false)] out T value);
        void VisitValue(IWriteVisitor visitor, VisitArgs args, T value);
    }
}