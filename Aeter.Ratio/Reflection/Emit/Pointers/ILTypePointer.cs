/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILTypePointer : ILPointer
    {

        private static readonly MethodInfo MethodGetTypeFromHandleToken =
            typeof(Type).FindMethod("GetTypeFromHandle", typeof(RuntimeTypeHandle));

        private readonly Type _type;
        public override Type Type => typeof(Type);

        public ILTypePointer(Type type)
        {
            _type = type;
        }

        protected override void Load(ILGenerator il)
        {
            il.Emit(OpCodes.Ldtoken, _type);
            il.EmitCall(OpCodes.Call, MethodGetTypeFromHandleToken, null);
        }

        protected override void LoadAddress(ILGenerator il)
        {
            Load(il);
        }
    }
}