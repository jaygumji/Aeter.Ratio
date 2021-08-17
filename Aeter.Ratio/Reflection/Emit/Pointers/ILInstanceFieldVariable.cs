/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILInstanceFieldVariable : ILVariable
    {
        public ILPointer Instance { get; }
        public FieldInfo Info { get; }
        public override Type Type => Info.FieldType;

        public ILInstanceFieldVariable(ILPointer instance, FieldInfo info)
        {
            Instance = instance;
            Info = info;
        }

        protected override void Load(ILGenerator il)
        {
            if (Instance.Type.GetTypeInfo().IsValueType) {
                il.LoadAddress(Instance);
            }
            else {
                il.Load(Instance);
            }

            il.Emit(OpCodes.Ldfld, Info);
        }

        protected override void LoadAddress(ILGenerator il)
        {
            if (Instance.Type.GetTypeInfo().IsValueType) {
                il.LoadAddress(Instance);
            }
            else {
                il.Load(Instance);
            }

            il.Emit(OpCodes.Ldflda, Info);
        }

        protected override void OnPreSet(ILGenerator il)
        {
            if (Instance.Type.GetTypeInfo().IsValueType) {
                il.LoadAddress(Instance);
            }
            else {
                il.Load(Instance);
            }
        }

        protected override void OnSet(ILGenerator il)
        {
            il.Emit(OpCodes.Stfld, Info);
        }
    }
}