/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit.Pointers
{
    public class ILAddPointer : ILPointer
    {
        private readonly IILPointer _left;
        private readonly IILPointer _right;

        public override Type Type => _left.Type;

        public ILAddPointer(IILPointer left, IILPointer right)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _right = right ?? throw new ArgumentNullException(nameof(right));
            if (left.Type != right.Type) {
                throw new ArgumentException("The types between the values doesn't match");
            }
        }

        protected override void Load(ILGenerator il)
        {
            _left.Load(il);
            _right.Load(il);
            il.Emit(OpCodes.Add);
        }
    }
}
