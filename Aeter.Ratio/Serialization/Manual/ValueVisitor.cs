/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Diagnostics.CodeAnalysis;

namespace Aeter.Ratio.Serialization.Manual
{
    public abstract class ValueVisitor<T> : IValueVisitor<T>
    {
        bool IValueVisitor.TryVisitValue(IReadVisitor visitor, VisitArgs args, [MaybeNullWhen(false)] out object value)
        {
            var res = TryVisitValue(visitor, args, out var typedValue);
            value = typedValue;
            return res;
        }

        void IValueVisitor.VisitValue(IWriteVisitor visitor, VisitArgs args, object value)
        {
            VisitValue(visitor, args, (T)value);
        }

        public abstract bool TryVisitValue(IReadVisitor visitor, VisitArgs args, [MaybeNullWhen(false)] out T value);

        public abstract void VisitValue(IWriteVisitor visitor, VisitArgs args, T value);
    }

    public static class ValueVisitor
    {
        public static IValueVisitor<T> Create<T>()
        {
            var type = typeof(T);
            return (IValueVisitor<T>)Create(type);
        }

        public static IValueVisitor Create(Type type)
        {
            if (type == typeof(byte)) return new ValueVisitorByte();
            if (type == typeof(short)) return new ValueVisitorInt16();
            if (type == typeof(int)) return new ValueVisitorInt32();
            if (type == typeof(long)) return new ValueVisitorInt64();
            if (type == typeof(ushort)) return new ValueVisitorUInt16();
            if (type == typeof(uint)) return new ValueVisitorUInt32();
            if (type == typeof(ulong)) return new ValueVisitorUInt64();
            if (type == typeof(bool)) return new ValueVisitorBoolean();
            if (type == typeof(float)) return new ValueVisitorSingle();
            if (type == typeof(double)) return new ValueVisitorDouble();
            if (type == typeof(decimal)) return new ValueVisitorDecimal();
            if (type == typeof(TimeSpan)) return new ValueVisitorTimeSpan();
            if (type == typeof(DateTime)) return new ValueVisitorDateTime();
            if (type == typeof(string)) return new ValueVisitorString();
            if (type == typeof(Guid)) return new ValueVisitorGuid();
            if (type == typeof(byte[])) return new ValueVisitorBlob();

            throw new ArgumentException("Unknown value type " + type.FullName);
        }
    }
}