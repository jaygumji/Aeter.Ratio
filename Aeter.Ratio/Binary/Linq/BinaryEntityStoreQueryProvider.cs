/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Aeter.Ratio.Binary.Linq
{
    /// <summary>
    /// Query provider that rewrites IQueryable expressions into Enumerable invocations
    /// so that BinaryEntityStore queries can execute in memory after materializing a slice of the store.
    /// </summary>
    internal sealed class BinaryEntityStoreQueryProvider : IQueryProvider
    {
        private readonly BinaryEntityStoreQueryContext context;
        private static readonly MethodInfo CreateEnumerableMethod = typeof(BinaryEntityStoreQueryContext)
            .GetMethod(nameof(BinaryEntityStoreQueryContext.CreateEnumerableCore), BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate BinaryEntityStoreQueryContext.CreateEnumerableCore.");

        public BinaryEntityStoreQueryProvider(BinaryEntityStoreQueryContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IQueryable CreateQuery(Expression expression)
        {
            ArgumentNullException.ThrowIfNull(expression);
            var elementType = expression.Type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
            var queryType = typeof(BinaryEntityStoreQueryable<>).MakeGenericType(elementType);
            return (IQueryable)Activator.CreateInstance(queryType, BindingFlags.Instance | BindingFlags.NonPublic, binder: null, args: new object[] { this, expression }, culture: null)!;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            ArgumentNullException.ThrowIfNull(expression);
            return new BinaryEntityStoreQueryable<TElement>(this, expression);
        }

        internal IQueryable CreateQuery(Type elementType)
        {
            ArgumentNullException.ThrowIfNull(elementType);
            var queryType = typeof(BinaryEntityStoreQueryable<>).MakeGenericType(elementType);
            return (IQueryable)Activator.CreateInstance(queryType, BindingFlags.Instance | BindingFlags.NonPublic, binder: null, args: new object[] { this }, culture: null)!;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            ArgumentNullException.ThrowIfNull(expression);
            return (TResult)Execute(expression)!;
        }

        /// <summary>
        /// Executes the specified query expression, returning the final result (scalar or sequence).
        /// </summary>
        public object? Execute(Expression expression)
        {
            ArgumentNullException.ThrowIfNull(expression);
            var specification = BinaryEntityStoreQuerySpecification.Create(this, expression);
            var enumerableResult = CreateEnumerable(specification);
            var rewritten = BinaryEntityStoreEnumerableExecutor.Rewrite(expression, this, enumerableResult);
            var result = BinaryEntityStoreEnumerableExecutor.Execute(rewritten);
            return EnsureResultType(expression.Type, result);
        }

        /// <summary>
        /// Materializes the current query specification into an enumerable that feeds the LINQ pipeline.
        /// </summary>
        private BinaryEntityStoreEnumerableResult CreateEnumerable(BinaryEntityStoreQuerySpecification specification)
        {
            var generic = CreateEnumerableMethod.MakeGenericMethod(specification.ElementType);
            var enumerable = (IEnumerable)generic.Invoke(context, new object[] { specification })!;
            return new BinaryEntityStoreEnumerableResult(specification.ElementType, enumerable);
        }

        private static object? EnsureResultType(Type expectedType, object? result)
        {
            if (result is null || expectedType.IsInstanceOfType(result)) {
                return result;
            }

            if (IsQueryable(expectedType) && result is IEnumerable enumerable) {
                var elementType = expectedType.GetGenericArguments()[0];
                var cast = CastEnumerable(enumerable, elementType);
                return Queryable.AsQueryable(cast);
            }

            return result;
        }

        private static bool IsQueryable(Type type)
            => type.IsGenericType && typeof(IQueryable).IsAssignableFrom(type);

        private static IEnumerable CastEnumerable(IEnumerable source, Type elementType)
        {
            var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast), BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(elementType);
            return (IEnumerable)castMethod.Invoke(null, new object[] { source })!;
        }

        private readonly struct BinaryEntityStoreEnumerableResult
        {
            public BinaryEntityStoreEnumerableResult(Type elementType, IEnumerable source)
            {
                ElementType = elementType;
                Source = source;
            }

            public Type ElementType { get; }
            public IEnumerable Source { get; }
        }

        /// <summary>
        /// Expression visitor that swaps Queryable operators with Enumerable counterparts and compiles the resulting delegate.
        /// </summary>
        private sealed class BinaryEntityStoreEnumerableExecutor : ExpressionVisitor
        {
            private static readonly IDictionary<string, List<MethodInfo>> EnumerableMethods = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .GroupBy(m => m.Name)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

            private BinaryEntityStoreEnumerableExecutor(BinaryEntityStoreQueryProvider provider, BinaryEntityStoreEnumerableResult enumerableResult)
            {
                this.provider = provider;
                this.enumerableResult = enumerableResult;
            }

            public static Expression Rewrite(Expression expression, BinaryEntityStoreQueryProvider provider, BinaryEntityStoreEnumerableResult enumerableResult)
                => new BinaryEntityStoreEnumerableExecutor(provider, enumerableResult).Visit(expression);

            public static object? Execute(Expression expression)
            {
                var lambda = Expression.Lambda(expression);
                var compiled = lambda.Compile();
                return compiled.DynamicInvoke();
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Value is IQueryable query && query.Provider == provider) {
                    var enumerated = CastToElementType(enumerableResult.Source, enumerableResult.ElementType, query.ElementType);
                    return Expression.Constant(enumerated, typeof(IEnumerable<>).MakeGenericType(query.ElementType));
                }

                return base.VisitConstant(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType == typeof(Queryable)) {
                    var arguments = new Expression[node.Arguments.Count];
                    arguments[0] = Visit(node.Arguments[0]);

                    for (var i = 1; i < node.Arguments.Count; i++) {
                        var visited = Visit(node.Arguments[i]);
                        if (visited is UnaryExpression { NodeType: ExpressionType.Quote } quote) {
                            var lambda = (LambdaExpression)quote.Operand;
                            visited = Expression.Constant(lambda.Compile(), lambda.Type);
                        }
                        arguments[i] = visited;
                    }

                    var enumerableMethod = ResolveEnumerableMethod(node.Method, arguments);
                    return Expression.Call(enumerableMethod, arguments);
                }

                return base.VisitMethodCall(node);
            }

            private static MethodInfo ResolveEnumerableMethod(MethodInfo queryableMethod, IReadOnlyList<Expression> arguments)
            {
                if (!EnumerableMethods.TryGetValue(queryableMethod.Name, out var candidates)) {
                    throw new InvalidOperationException($"Unable to translate method '{queryableMethod.Name}' to an Enumerable counterpart.");
                }

                foreach (var candidate in candidates) {
                    var method = candidate;
                    if (method.IsGenericMethodDefinition) {
                        if (!queryableMethod.IsGenericMethod) {
                            continue;
                        }
                        var genericArgs = queryableMethod.GetGenericArguments();
                        if (method.GetGenericArguments().Length != genericArgs.Length) {
                            continue;
                        }
                        method = method.MakeGenericMethod(genericArgs);
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length != arguments.Count) {
                        continue;
                    }

                    var compatible = true;
                    for (var i = 0; i < parameters.Length; i++) {
                        if (!parameters[i].ParameterType.IsAssignableFrom(arguments[i].Type)) {
                            compatible = false;
                            break;
                        }
                    }

                    if (compatible) {
                        return method;
                    }
                }

                throw new InvalidOperationException($"Unable to translate method '{queryableMethod.Name}' to an Enumerable counterpart.");
            }

            private static IEnumerable CastToElementType(IEnumerable source, Type currentType, Type requiredType)
            {
                if (currentType == requiredType) {
                    return source;
                }

                var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast), BindingFlags.Public | BindingFlags.Static)!
                    .MakeGenericMethod(requiredType);
                return (IEnumerable)castMethod.Invoke(null, new object[] { source })!;
            }

            private readonly BinaryEntityStoreQueryProvider provider;
            private readonly BinaryEntityStoreEnumerableResult enumerableResult;
        }
    }
}
