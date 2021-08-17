/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Db.Linq;
using System.Linq.Expressions;

namespace Aeter.Ratio.Test.Db.Linq
{
    public class FakeEnigmaQueryExecutor : IEnigmaQueryExecutor
    {

        public object? Execute(Expression expression, bool isCollection)
        {
            return null;
        }
    }
}