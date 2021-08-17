/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit.Pointers
{
    public class ILEqualPointer : ILPointer
    {
        private readonly IILPointer _left;
        private readonly IILPointer _right;
        public override Type Type => typeof(bool);

        public ILEqualPointer(IILPointer left, IILPointer right)
        {
            _left = left;
            _right = right;
        }

        protected override void Load(ILGenerator il)
        {
            _left.Load(il);
            _right.Load(il);
            il.Emit(OpCodes.Ceq);
        }
    }
}
