/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public abstract class ILVariable : ILPointer, IILVariable
    {

        void IILVariable.PreSet(ILGenerator il)
        {
            OnPreSet(il);
        }

        void IILVariable.Set(ILGenerator il)
        {
            OnSet(il);
        }

        protected virtual void OnPreSet(ILGenerator il)
        {
        }

        protected virtual void OnSet(ILGenerator il)
        {
            throw new InvalidOperationException("Not possible to use this variable in a set operation, " + GetType().Name);
        }

        public static implicit operator ILVariable(LocalBuilder local)
        {
            return new ILLocalVariable(local);
        }

        public ILSetVariable Set(ILPointer value) => new(this, value);

    }
}