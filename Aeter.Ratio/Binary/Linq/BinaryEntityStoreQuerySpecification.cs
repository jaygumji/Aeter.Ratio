/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Aeter.Ratio.Binary.Linq
{
    internal sealed class BinaryEntityStoreQuerySpecification
    {
        private readonly IReadOnlyList<LambdaExpression> predicates;
        private readonly IReadOnlyList<BinaryEntityStoreIndexFilter> indexFilters;

        private BinaryEntityStoreQuerySpecification(Type elementType, IReadOnlyList<LambdaExpression> predicates, IReadOnlyList<BinaryEntityStoreIndexFilter> indexFilters)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            this.predicates = predicates ?? throw new ArgumentNullException(nameof(predicates));
            this.indexFilters = indexFilters ?? throw new ArgumentNullException(nameof(indexFilters));
        }

        public Type ElementType { get; }
        public IReadOnlyList<BinaryEntityStoreIndexFilter> IndexFilters => indexFilters;

        public static BinaryEntityStoreQuerySpecification Create(BinaryEntityStoreQueryProvider provider, Expression expression)
        {
            ArgumentNullException.ThrowIfNull(provider);
            ArgumentNullException.ThrowIfNull(expression);

            var analyzer = new BinaryEntityStoreQueryAnalyzer(provider);
            analyzer.Visit(expression);

            var elementType = analyzer.ElementType ?? throw new InvalidOperationException("Unable to determine the element type for the supplied query expression.");
            return new BinaryEntityStoreQuerySpecification(elementType, analyzer.Predicates, analyzer.IndexCandidates);
        }

        public Func<T, bool>? CreatePredicate<T>()
        {
            if (predicates.Count == 0) {
                return null;
            }

            var parameter = Expression.Parameter(typeof(T), "entity");
            Expression? body = null;
            foreach (var predicate in predicates) {
                var rewritten = BinaryEntityStoreParameterReplacer.Replace(predicate.Body, predicate.Parameters[0], parameter);
                body = body is null ? rewritten : Expression.AndAlso(body, rewritten);
            }

            if (body is null) {
                return null;
            }

            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
            return lambda.Compile();
        }
    }

    internal sealed class BinaryEntityStoreQueryAnalyzer : ExpressionVisitor
    {
        private readonly BinaryEntityStoreQueryProvider provider;
        private readonly List<LambdaExpression> predicates = new();
        private readonly List<BinaryEntityStoreIndexFilter> indexCandidates = new();

        public BinaryEntityStoreQueryAnalyzer(BinaryEntityStoreQueryProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public Type? ElementType { get; private set; }
        public IReadOnlyList<LambdaExpression> Predicates => predicates;
        public IReadOnlyList<BinaryEntityStoreIndexFilter> IndexCandidates => indexCandidates;

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryable query && query.Provider == provider) {
                ElementType ??= query.ElementType;
            }

            return base.VisitConstant(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable)) {
                foreach (var lambda in ExtractPredicateArguments(node)) {
                    predicates.Add(lambda);
                    if (BinaryEntityStoreIndexFilter.TryCreate(lambda, out var filter)) {
                        indexCandidates.Add(filter);
                    }
                }
            }

            return base.VisitMethodCall(node);
        }

        private static IEnumerable<LambdaExpression> ExtractPredicateArguments(MethodCallExpression node)
        {
            foreach (var argument in node.Arguments) {
                if (argument is UnaryExpression { NodeType: ExpressionType.Quote, Operand: LambdaExpression lambda } &&
                    lambda.ReturnType == typeof(bool)) {
                    yield return lambda;
                }
            }
        }
    }

    internal readonly struct BinaryEntityStoreIndexFilter
    {
        public BinaryEntityStoreIndexFilter(string path, object value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentNullException.ThrowIfNull(value);
            Path = path;
            Value = value;
        }

        public string Path { get; }
        public object Value { get; }

        public static bool TryCreate(LambdaExpression predicate, out BinaryEntityStoreIndexFilter filter)
        {
            filter = default;
            if (predicate.Parameters.Count != 1) {
                return false;
            }

            var parameter = predicate.Parameters[0];
            var body = BinaryEntityStoreExpressionHelpers.StripConvert(predicate.Body);

            if (body is BinaryExpression binary && binary.NodeType == ExpressionType.Equal) {
                if (TryCreateFromBinary(parameter, binary.Left, binary.Right, out filter)) {
                    return true;
                }

                if (TryCreateFromBinary(parameter, binary.Right, binary.Left, out filter)) {
                    return true;
                }
            }

            return false;
        }

        private static bool TryCreateFromBinary(ParameterExpression parameter, Expression memberExpression, Expression valueExpression, out BinaryEntityStoreIndexFilter filter)
        {
            filter = default;
            var member = BinaryEntityStoreExpressionHelpers.StripConvert(memberExpression);
            var value = BinaryEntityStoreExpressionHelpers.StripConvert(valueExpression);

            if (!BinaryEntityStoreExpressionHelpers.TryGetMemberPath(member, parameter, out var path)) {
                return false;
            }

            if (!BinaryEntityStoreExpressionHelpers.TryEvaluate(value, parameter, out var constant) || constant is null) {
                return false;
            }

            filter = new BinaryEntityStoreIndexFilter(path, constant);
            return true;
        }
    }

    internal static class BinaryEntityStoreParameterReplacer
    {
        public static Expression Replace(Expression body, ParameterExpression source, ParameterExpression target)
        {
            ArgumentNullException.ThrowIfNull(body);
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(target);
            return new ParameterReplaceVisitor(source, target).Visit(body);
        }

        private sealed class ParameterReplaceVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression source;
            private readonly ParameterExpression target;

            public ParameterReplaceVisitor(ParameterExpression source, ParameterExpression target)
            {
                this.source = source;
                this.target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == source) {
                    return target;
                }
                return base.VisitParameter(node);
            }
        }
    }

    internal static class BinaryEntityStoreExpressionHelpers
    {
        public static Expression StripConvert(Expression expression)
        {
            ArgumentNullException.ThrowIfNull(expression);
            while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked) {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }

        public static bool TryGetMemberPath(Expression expression, ParameterExpression parameter, out string path)
        {
            path = string.Empty;
            var members = new Stack<string>();
            var current = StripConvert(expression);
            while (current is MemberExpression member) {
                members.Push(member.Member.Name);
                var inner = member.Expression is null ? null : StripConvert(member.Expression);
                if (inner == parameter) {
                    path = string.Join(".", members);
                    return true;
                }
                current = inner;
            }

            return false;
        }

        public static bool TryEvaluate(Expression expression, ParameterExpression parameter, out object? value)
        {
            value = null;
            if (ContainsParameter(expression, parameter)) {
                return false;
            }

            try {
                var lambda = Expression.Lambda(expression);
                value = lambda.Compile().DynamicInvoke();
                return true;
            }
            catch {
                value = null;
                return false;
            }
        }

        private static bool ContainsParameter(Expression expression, ParameterExpression parameter)
        {
            var finder = new ParameterFinder(parameter);
            finder.Visit(expression);
            return finder.Found;
        }

        private sealed class ParameterFinder : ExpressionVisitor
        {
            private readonly ParameterExpression target;
            public bool Found { get; private set; }

            public ParameterFinder(ParameterExpression target)
            {
                this.target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == target) {
                    Found = true;
                }
                return base.VisitParameter(node);
            }
        }
    }
}
