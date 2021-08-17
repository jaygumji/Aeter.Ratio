/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Linq.Expressions;

namespace Aeter.Ratio.Db.Linq
{

    public class EnigmaQueryExecutor : IEnigmaQueryExecutor
    {
        private readonly IEnigmaConnection _connection;

        public EnigmaQueryExecutor(IEnigmaConnection connection)
        {
            _connection = connection;
        }

        public object Execute(Expression expression, bool isCollection)
        {
            var stack = new EnigmaQueryStack();

            var visitor = new EnigmaQueryVisitor(stack);
            visitor.Visit(expression);

            return null;
        }
    }
}