/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public abstract class ILConstantPointer : ILPointer
    {
        public override Type Type { get; }

        protected ILConstantPointer(Type type)
        {
            Type = type;
        }

        protected override void LoadAddress(ILGenerator il)
        {
            Load(il);
        }
    }
}