/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Aeter.Ratio.Serialization.Manual
{
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

}