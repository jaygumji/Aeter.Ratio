/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILSetVariable : ILPointer
    {
        private readonly ILVariable variable;
        private readonly ILPointer value;

        public override Type? Type => variable.Type;

        public ILSetVariable(ILVariable variable, ILPointer value)
        {
            this.variable = variable;
            this.value = value;
        }

        protected override void Load(ILGenerator il)
        {
            ((IILPointer)variable).Load(il); // Load variable to be ready for next statement
            ((IILPointer)value).Load(il); // Load pointer value to set
            ((IILVariable)variable).Set(il); // Set the variable
        }
    }
}