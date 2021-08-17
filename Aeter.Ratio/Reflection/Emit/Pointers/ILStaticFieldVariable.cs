/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILStaticFieldVariable : ILVariable
    {
        public FieldInfo Info { get; }
        public override Type Type => Info.FieldType;

        public ILStaticFieldVariable(FieldInfo info)
        {
            Info = info;
        }

        protected override void Load(ILGenerator il)
        {
            il.Emit(OpCodes.Ldsfld, Info);
        }

        protected override void LoadAddress(ILGenerator il)
        {
            il.Emit(OpCodes.Ldsflda, Info);
        }

        protected override void OnSet(ILGenerator il)
        {
            il.Emit(OpCodes.Stsfld, Info);
        }

    }
}