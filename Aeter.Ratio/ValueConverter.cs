/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Globalization;
using System.Reflection;

namespace Aeter.Ratio
{
    public static class ValueConverter
    {
        public static string Text(object value)
        {
            if (value is null) {
                return string.Empty;
            }

            if (value is string s) {
                return s;
            }

            if (value is byte[] bytes) {
                return Convert.ToBase64String(bytes);
            }

            if (value is Guid guid) {
                return guid.ToString("D", CultureInfo.InvariantCulture);
            }

            if (value is DateTime dateTime) {
                return dateTime.ToString("o", CultureInfo.InvariantCulture);
            }

            if (value is DateOnly dateOnly) {
                return dateOnly.ToString("O", CultureInfo.InvariantCulture);
            }

            if (value is TimeOnly timeOnly) {
                return timeOnly.ToString("O", CultureInfo.InvariantCulture);
            }

            var type = value.GetType();

            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                    return Convert.ToBoolean(value, CultureInfo.InvariantCulture).ToString();
                case TypeCode.Char:
                    return Convert.ToChar(value, CultureInfo.InvariantCulture).ToString();
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            }

            var implicitString = FindImplicitConversionOperator(type, typeof(string));
            if (implicitString != null) {
                var result = implicitString.Invoke(null, new[] { value });
                if (result is string implicitValue) {
                    return implicitValue;
                }
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static MethodInfo? FindImplicitConversionOperator(Type sourceType, Type targetType)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

            static MethodInfo? Scan(Type typeToScan, Type sourceType, Type targetType, BindingFlags flags)
            {
                foreach (var method in typeToScan.GetMethods(flags)) {
                    if (method.Name != "op_Implicit") {
                        continue;
                    }

                    if (!targetType.IsAssignableFrom(method.ReturnType)) {
                        continue;
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length != 1) {
                        continue;
                    }

                    if (parameters[0].ParameterType.IsAssignableFrom(sourceType)) {
                        return method;
                    }
                }

                return null;
            }

            return Scan(sourceType, sourceType, targetType, flags) ??
                   Scan(targetType, sourceType, targetType, flags);
        }

        public static T? ChangeType<T>(object? value)
        {
            var converted = ChangeType(value, typeof(T));
            if (converted is null) {
                return default;
            }

            return (T?)converted;
        }

        public static object? ChangeType(object? value, Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var targetType = Nullable.GetUnderlyingType(type) ?? type;

            if (value is null) {
                if (!targetType.IsValueType || Nullable.GetUnderlyingType(type) != null) {
                    return null;
                }
                throw new InvalidCastException($"Cannot convert null to non-nullable type {type}.");
            }

            var sourceType = value.GetType();

            if (targetType.IsAssignableFrom(sourceType)) {
                return value;
            }

            var implicitOperator = FindImplicitConversionOperator(sourceType, targetType);
            if (implicitOperator != null) {
                return implicitOperator.Invoke(null, new[] { value })!;
            }

            if (value is string stringValue) {
                if (targetType == typeof(string)) {
                    return stringValue;
                }

                if (TryParseFromString(stringValue, targetType, out var parsed)) {
                    return parsed!;
                }
            }

            if (targetType == typeof(string)) {
                return Text(value);
            }

            if (targetType == typeof(DateOnly) && value is DateTime dateTime) {
                return DateOnly.FromDateTime(dateTime);
            }

            if (targetType == typeof(TimeOnly)) {
                if (value is DateTime dt) {
                    return TimeOnly.FromDateTime(dt);
                }

                if (value is TimeSpan timeSpan) {
                    return TimeOnly.FromTimeSpan(timeSpan);
                }
            }

            if (targetType == typeof(byte[]) && value is string base64String) {
                return Convert.FromBase64String(base64String);
            }

            if (targetType.IsEnum) {
                if (value is string enumText) {
                    return Enum.Parse(targetType, enumText, ignoreCase: true);
                }

                var underlyingType = Enum.GetUnderlyingType(targetType);
                var convertedValue = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
                return Enum.ToObject(targetType, convertedValue!);
            }

            if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(targetType)) {
                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }

            throw new InvalidCastException($"Cannot convert from {sourceType} to {targetType}.");
        }

        private static bool TryParseFromString(string input, Type targetType, out object? result)
        {
            result = null;

            switch (Type.GetTypeCode(targetType)) {
                case TypeCode.Boolean:
                    if (bool.TryParse(input, out var boolValue)) {
                        result = boolValue;
                        return true;
                    }
                    return false;
                case TypeCode.Char:
                    if (input.Length == 1) {
                        result = input[0];
                        return true;
                    }
                    return false;
                case TypeCode.SByte:
                    if (sbyte.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sbyteValue)) {
                        result = sbyteValue;
                        return true;
                    }
                    return false;
                case TypeCode.Byte:
                    if (byte.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var byteValue)) {
                        result = byteValue;
                        return true;
                    }
                    return false;
                case TypeCode.Int16:
                    if (short.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var shortValue)) {
                        result = shortValue;
                        return true;
                    }
                    return false;
                case TypeCode.UInt16:
                    if (ushort.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ushortValue)) {
                        result = ushortValue;
                        return true;
                    }
                    return false;
                case TypeCode.Int32:
                    if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue)) {
                        result = intValue;
                        return true;
                    }
                    return false;
                case TypeCode.UInt32:
                    if (uint.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uintValue)) {
                        result = uintValue;
                        return true;
                    }
                    return false;
                case TypeCode.Int64:
                    if (long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue)) {
                        result = longValue;
                        return true;
                    }
                    return false;
                case TypeCode.UInt64:
                    if (ulong.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ulongValue)) {
                        result = ulongValue;
                        return true;
                    }
                    return false;
                case TypeCode.Single:
                    if (float.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var floatValue)) {
                        result = floatValue;
                        return true;
                    }
                    return false;
                case TypeCode.Double:
                    if (double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var doubleValue)) {
                        result = doubleValue;
                        return true;
                    }
                    return false;
                case TypeCode.Decimal:
                    if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue)) {
                        result = decimalValue;
                        return true;
                    }
                    return false;
                case TypeCode.DateTime:
                    if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTimeValue)) {
                        result = dateTimeValue;
                        return true;
                    }
                    return false;
                case TypeCode.String:
                    result = input;
                    return true;
            }

            if (targetType == typeof(Guid)) {
                if (Guid.TryParse(input, out var guidValue)) {
                    result = guidValue;
                    return true;
                }
                return false;
            }

            if (targetType == typeof(byte[])) {
                result = Convert.FromBase64String(input);
                return true;
            }

            if (targetType == typeof(DateOnly)) {
                if (DateOnly.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnlyValue)) {
                    result = dateOnlyValue;
                    return true;
                }
                return false;
            }

            if (targetType == typeof(TimeOnly)) {
                if (TimeOnly.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeOnlyValue)) {
                    result = timeOnlyValue;
                    return true;
                }
                return false;
            }

            if (targetType.IsEnum) {
                result = Enum.Parse(targetType, input, ignoreCase: true);
                return true;
            }

            return false;
        }

        public static string? ToBase16String(byte[]? bytes)
        {
            if (bytes == null) {
                return null;
            }

            if (bytes.Length == 0) {
                return string.Empty;
            }

            return string.Create(bytes.Length * 2, bytes, static (chars, source) => {
                const string Alphabet = "0123456789ABCDEF";
                var index = 0;
                foreach (var b in source) {
                    chars[index++] = Alphabet[b >> 4];
                    chars[index++] = Alphabet[b & 0x0F];
                }
            });
        }
    }
}
