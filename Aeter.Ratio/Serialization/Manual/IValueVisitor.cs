/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Serialization.Manual
{
    public interface IValueVisitor
    {
        bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out object value);
        void VisitValue(IWriteVisitor visitor, VisitArgs args, object value);
    }

    public interface IValueVisitor<T> : IValueVisitor
    {
        bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out T value);
        void VisitValue(IWriteVisitor visitor, VisitArgs args, T value);
    }

    public abstract class ValueVisitor<T> : IValueVisitor<T>
    {
        bool IValueVisitor.TryVisitValue(IReadVisitor visitor, VisitArgs args, out object value)
        {
            var res = TryVisitValue(visitor, args, out var typedValue);
            value = typedValue;
            return res;
        }

        void IValueVisitor.VisitValue(IWriteVisitor visitor, VisitArgs args, object value)
        {
            VisitValue(visitor, args, (T)value);
        }

        public abstract bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out T value);

        public abstract void VisitValue(IWriteVisitor visitor, VisitArgs args, T value);
    }

    public static class ValueVisitor
    {
        public static IValueVisitor<T> Create<T>()
        {
            var type = typeof(T);
            return (IValueVisitor<T>) Create(type);
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

    public class ValueVisitorByte : ValueVisitor<byte>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out byte value)
        {
            if (visitor.TryVisitValue(args, out byte? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, byte value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorInt16 : ValueVisitor<short>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out short value)
        {
            if (visitor.TryVisitValue(args, out short? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, short value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorInt32 : ValueVisitor<int>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out int value)
        {
            if (visitor.TryVisitValue(args, out int? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, int value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorInt64 : ValueVisitor<long>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out long value)
        {
            if (visitor.TryVisitValue(args, out long? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, long value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorUInt16 : ValueVisitor<ushort>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out ushort value)
        {
            if (visitor.TryVisitValue(args, out ushort? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, ushort value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorUInt32 : ValueVisitor<uint>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out uint value)
        {
            if (visitor.TryVisitValue(args, out uint? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, uint value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorUInt64 : ValueVisitor<ulong>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out ulong value)
        {
            if (visitor.TryVisitValue(args, out ulong? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, ulong value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorBoolean : ValueVisitor<bool>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out bool value)
        {
            if (visitor.TryVisitValue(args, out bool? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, bool value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorSingle : ValueVisitor<float>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out float value)
        {
            if (visitor.TryVisitValue(args, out float? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, float value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorDouble : ValueVisitor<double>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out double value)
        {
            if (visitor.TryVisitValue(args, out double? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, double value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorDecimal : ValueVisitor<decimal>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out decimal value)
        {
            if (visitor.TryVisitValue(args, out decimal? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, decimal value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorTimeSpan : ValueVisitor<TimeSpan>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out TimeSpan value)
        {
            if (visitor.TryVisitValue(args, out TimeSpan? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, TimeSpan value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorDateTime : ValueVisitor<DateTime>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out DateTime value)
        {
            if (visitor.TryVisitValue(args, out DateTime? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, DateTime value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorString : ValueVisitor<string>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out string value)
        {
            return visitor.TryVisitValue(args, out value);
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, string value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorGuid : ValueVisitor<Guid>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out Guid value)
        {
            if (visitor.TryVisitValue(args, out Guid? nullable))
            {
                value = nullable ?? default;
                return true;
            }
            value = default;
            return false;
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, Guid value)
        {
            visitor.VisitValue(value, args);
        }
    }
    public class ValueVisitorBlob : ValueVisitor<byte[]>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out byte[] value)
        {
            return visitor.TryVisitValue(args, out value);
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, byte[] value)
        {
            visitor.VisitValue(value, args);
        }
    }

}