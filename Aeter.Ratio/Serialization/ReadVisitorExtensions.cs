using System;
using System.Collections.Generic;
using System.Globalization;

namespace Aeter.Ratio.Serialization
{
    public static class ReadVisitorExtensions
    {
        public static IReadOnlyList<object?> FindValue(this IReadVisitor visitor, string path, Type type)
        {
            ArgumentNullException.ThrowIfNull(visitor);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentNullException.ThrowIfNull(type);

            var segments = ParsePath(path);
            if (segments.Length == 0) {
                return Array.Empty<object?>();
            }

            var rootArgs = VisitArgs.CreateRoot(LevelType.Single);
            var rootState = visitor.TryVisit(rootArgs);
            if (rootState != ValueState.Found) {
                return Array.Empty<object?>();
            }

            var results = new List<object?>();
            try {
                Traverse(visitor, segments, 0, type, results);
            }
            finally {
                visitor.Leave(rootArgs);
            }

            return results.Count == 0 ? Array.Empty<object?>() : results;
        }

        private static void Traverse(IReadVisitor visitor, PathSegment[] segments, int index, Type targetType, List<object?> results)
        {
            var segment = segments[index];
            var isLast = index == segments.Length - 1;

            if (segment.IsCollection) {
                VisitCollectionSegment(visitor, segments, index, targetType, results, segment, isLast);
                return;
            }

            if (isLast) {
                var valueArgs = new VisitArgs(segment.Name, LevelType.Value);
                if (TryReadValue(visitor, valueArgs, targetType, out var value)) {
                    results.Add(value);
                }
                return;
            }

            var nestedArgs = new VisitArgs(segment.Name, LevelType.Single);
            var state = visitor.TryVisit(nestedArgs);
            if (state != ValueState.Found) {
                return;
            }

            try {
                Traverse(visitor, segments, index + 1, targetType, results);
            }
            finally {
                visitor.Leave(nestedArgs);
            }
        }

        private static void VisitCollectionSegment(IReadVisitor visitor, PathSegment[] segments, int index, Type targetType, List<object?> results, PathSegment segment, bool isLast)
        {
            var collectionArgs = new VisitArgs(segment.Name, LevelType.Collection);
            var state = visitor.TryVisit(collectionArgs);
            if (state != ValueState.Found) {
                return;
            }

            try {
                if (isLast) {
                    CollectCollectionValues(visitor, targetType, results, segment);
                }
                else {
                    TraverseCollectionItems(visitor, segments, index + 1, targetType, results, segment);
                }
            }
            finally {
                visitor.Leave(collectionArgs);
            }
        }

        private static void CollectCollectionValues(IReadVisitor visitor, Type targetType, List<object?> results, PathSegment segment)
        {
            var itemArgs = VisitArgs.CollectionItem;
            uint currentIndex = 0;
            while (TryReadValue(visitor, itemArgs.ForIndex(currentIndex), targetType, out var value)) {
                if (!segment.HasIndex || currentIndex == segment.Index) {
                    results.Add(value);
                    if (segment.HasIndex) {
                        break;
                    }
                }

                currentIndex++;
            }
        }

        private static void TraverseCollectionItems(IReadVisitor visitor, PathSegment[] segments, int nextIndex, Type targetType, List<object?> results, PathSegment segment)
        {
            uint currentIndex = 0;
            while (true) {
                var curArgs = VisitArgs.CollectionItem.ForIndex(currentIndex);
                var state = visitor.TryVisit(curArgs);
                if (state == ValueState.NotFound) {
                    break;
                }

                var shouldProcess = !segment.HasIndex || currentIndex == segment.Index;
                if (state == ValueState.Found) {
                    if (shouldProcess) {
                        try {
                            Traverse(visitor, segments, nextIndex, targetType, results);
                        }
                        finally {
                            visitor.Leave(curArgs);
                        }

                        if (segment.HasIndex) {
                            break;
                        }
                    }
                    else {
                        visitor.Leave(curArgs);
                    }
                }

                if (segment.HasIndex && shouldProcess && state != ValueState.Found) {
                    break;
                }

                currentIndex++;
            }
        }

        private static PathSegment[] ParsePath(string path)
        {
            var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var segments = new PathSegment[parts.Length];
            for (var i = 0; i < parts.Length; i++) {
                var part = parts[i];
                var bracketIndex = part.IndexOf('[');
                if (bracketIndex < 0) {
                    segments[i] = new PathSegment(part, CollectionSelection.None, -1);
                    continue;
                }

                var name = part[..bracketIndex].Trim();
                var closeIndex = part.IndexOf(']', bracketIndex + 1);
                if (closeIndex < 0) {
                    throw new FormatException($"Invalid path segment '{part}'. Missing closing ']'.");
                }
                if (closeIndex != part.Length - 1) {
                    throw new FormatException($"Invalid path segment '{part}'. Unexpected characters after ']'.");
                }

                var spec = part.Substring(bracketIndex + 1, closeIndex - bracketIndex - 1).Trim();
                if (spec.Length == 0) {
                    segments[i] = new PathSegment(name, CollectionSelection.All, -1);
                    continue;
                }

                if (!int.TryParse(spec, NumberStyles.None, CultureInfo.InvariantCulture, out var index) || index < 0) {
                    throw new FormatException($"Invalid index '{spec}' in path segment '{part}'.");
                }

                segments[i] = new PathSegment(name, CollectionSelection.Index, index);
            }

            return segments;
        }

        private static bool TryReadValue(IReadVisitor visitor, VisitArgs args, Type requestedType, out object? value)
        {
            var underlying = Nullable.GetUnderlyingType(requestedType);
            var targetType = underlying ?? requestedType;

            if (targetType == typeof(byte)) {
                if (!visitor.TryVisitValue(args, out byte? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(short)) {
                if (!visitor.TryVisitValue(args, out short? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(int)) {
                if (!visitor.TryVisitValue(args, out int? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(long)) {
                if (!visitor.TryVisitValue(args, out long? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(ushort)) {
                if (!visitor.TryVisitValue(args, out ushort? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(uint)) {
                if (!visitor.TryVisitValue(args, out uint? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(ulong)) {
                if (!visitor.TryVisitValue(args, out ulong? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(bool)) {
                if (!visitor.TryVisitValue(args, out bool? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(float)) {
                if (!visitor.TryVisitValue(args, out float? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(double)) {
                if (!visitor.TryVisitValue(args, out double? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(decimal)) {
                if (!visitor.TryVisitValue(args, out decimal? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(TimeSpan)) {
                if (!visitor.TryVisitValue(args, out TimeSpan? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(DateTime)) {
                if (!visitor.TryVisitValue(args, out DateTime? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(Guid)) {
                if (!visitor.TryVisitValue(args, out Guid? result)) {
                    value = null;
                    return false;
                }
                value = BoxValue(result, requestedType);
                return true;
            }
            if (targetType == typeof(string)) {
                if (!visitor.TryVisitValue(args, out string? result)) {
                    value = null;
                    return false;
                }
                value = result;
                return true;
            }
            if (targetType == typeof(byte[])) {
                if (!visitor.TryVisitValue(args, out byte[]? result)) {
                    value = null;
                    return false;
                }
                value = result;
                return true;
            }

            throw new NotSupportedException($"Type '{requestedType}' is not supported by FindValue.");
        }

        private static object? BoxValue<T>(T? value, Type requestedType)
            where T : struct
        {
            if (!value.HasValue) {
                return null;
            }

            if (requestedType == typeof(T)) {
                return value.Value;
            }

            return value;
        }

        private enum CollectionSelection
        {
            None,
            All,
            Index
        }

        private readonly struct PathSegment
        {
            public PathSegment(string name, CollectionSelection selection, int index)
            {
                Name = name;
                Selection = selection;
                Index = index;
            }

            public string Name { get; }
            public CollectionSelection Selection { get; }
            public int Index { get; }
            public bool IsCollection => Selection != CollectionSelection.None;
            public bool HasIndex => Selection == CollectionSelection.Index;
        }
    }
}
