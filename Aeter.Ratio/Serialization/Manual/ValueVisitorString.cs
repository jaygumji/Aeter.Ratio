/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Aeter.Ratio.Serialization.Manual
{
    public class ValueVisitorString : ValueVisitor<string?>
    {
        public override bool TryVisitValue(IReadVisitor visitor, VisitArgs args, out string? value)
        {
            return visitor.TryVisitValue(args, out value);
        }
        public override void VisitValue(IWriteVisitor visitor, VisitArgs args, string? value)
        {
            visitor.VisitValue(value, args);
        }
    }

}