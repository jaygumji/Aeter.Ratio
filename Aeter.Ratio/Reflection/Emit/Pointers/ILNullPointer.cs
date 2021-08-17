/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public sealed class ILNullPointer : ILPointer
    {
        public static ILNullPointer Instance { get; } = new ILNullPointer();

        public override Type Type => null;

        private ILNullPointer() { }

        protected override void Load(ILGenerator il)
        {
            il.Emit(OpCodes.Ldnull);
        }
    }
}