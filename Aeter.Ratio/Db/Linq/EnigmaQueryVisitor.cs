/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Linq.Expressions;

namespace Aeter.Ratio.Db.Linq
{
    public class EnigmaQueryVisitor : ExpressionVisitor
    {
        private EnigmaQueryStack _stack;

        public EnigmaQueryVisitor(EnigmaQueryStack stack)
        {
            _stack = stack;
        }

        public override Expression? Visit(Expression? node)
        {
            return base.Visit(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return base.VisitUnary(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            return base.VisitBinary(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return base.VisitMember(node);
        }
    }
}