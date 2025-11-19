/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Aeter.Ratio.Binary.Linq
{
    /// <summary>
    /// Lightweight IQueryable implementation that forwards query evaluation to <see cref="BinaryEntityStoreQueryProvider"/>.
    /// </summary>
    internal sealed class BinaryEntityStoreQueryable<T> : IOrderedQueryable<T>
    {
        /// <summary>
        /// Creates a queryable bound to the supplied provider.
        /// </summary>
        internal BinaryEntityStoreQueryable(BinaryEntityStoreQueryProvider provider)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = Expression.Constant(this);
        }

        internal BinaryEntityStoreQueryable(BinaryEntityStoreQueryProvider provider, Expression expression)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type)) {
                throw new ArgumentOutOfRangeException(nameof(expression), $"Expression type '{expression.Type}' is not assignable to IQueryable<{typeof(T).Name}>.");
            }
        }

        public Type ElementType => typeof(T);
        public Expression Expression { get; }
        public IQueryProvider Provider { get; }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            var result = Provider.Execute(Expression);
            return ((IEnumerable<T>)result).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var result = Provider.Execute(Expression);
            return ((IEnumerable)result).GetEnumerator();
        }
    }
}
