/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Aeter.Ratio.Db.Linq
{
    public class EnigmaQueryProvider : IQueryProvider
    {
        private readonly IEnigmaQueryExecutor _executor;

        public EnigmaQueryProvider(IEnigmaQueryExecutor executor)
        {
            _executor = executor;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
            //var elementType = expression.Type.Wrap().Container.AsCollection().ElementType;
            //try
            //{
            //    return (IQueryable)Activator.CreateInstance(typeof(EnigmaQueryableData<>).MakeGenericType(elementType), new object[] { this, expression });
            //}
            //catch (System.Reflection.TargetInvocationException tie)
            //{
            //    throw tie.InnerException;
            //}
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            throw new NotImplementedException();
            //return new EnigmaQueryableSet<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _executor.Execute(expression, false)!;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)_executor.Execute(expression,
                FactoryTypeProvider.Instance.Extend(typeof(TResult)).Classification == TypeClassification.Collection)!;
        }
    }

}
