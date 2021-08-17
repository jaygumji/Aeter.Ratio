/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILArrayElementVariable : ILVariable
    {
        private readonly IILPointer _array;
        private readonly int _index;

        public override Type Type { get; }

        public ILArrayElementVariable(IILPointer array, int index)
        {
            _array = array;
            _index = index;
            Type = GetTypeFrom(array);
        }

        private static Type GetTypeFrom(IILPointer array)
        {
            var type = array.Type;
            if (!type.IsArray) throw new ArgumentException("The supplied type must be an array, type was " + type.FullName);
            return type.GetElementType();
        }

        protected override void Load(ILGenerator il)
        {
            il.Load(_array);
            il.Load(_index);

            if (Type.GetTypeInfo().IsClass) {
                il.Emit(OpCodes.Ldelem_Ref);
            }
            else if (Type == typeof(byte)) {
                il.Emit(OpCodes.Ldelem_U1);
            }
            else if (Type == typeof(short)) {
                il.Emit(OpCodes.Ldelem_I2);
            }
            else if (Type == typeof(int)) {
                il.Emit(OpCodes.Ldelem_I4);
            }
            else if (Type == typeof(long) || Type == typeof(ulong)) {
                il.Emit(OpCodes.Ldelem_I8);
            }
            else if (Type == typeof(float)) {
                il.Emit(OpCodes.Ldelem_R4);
            }
            else if (Type == typeof(double)) {
                il.Emit(OpCodes.Ldelem_R8);
            }
            else if (Type == typeof(ushort)) {
                il.Emit(OpCodes.Ldelem_U2);
            }
            else if (Type == typeof(uint)) {
                il.Emit(OpCodes.Ldelem_U4);
            }
            else {
                il.Emit(OpCodes.Ldelem, Type);
            }
        }

        protected override void LoadAddress(ILGenerator il)
        {
            il.Load(_array);
            il.Load(_index);
            il.Emit(OpCodes.Ldelema);
        }

        protected override void OnSet(ILGenerator il)
        {
            var value = il.NewLocal(Type);
            il.Set(value);
            il.Load(_array);
            il.Load(_index);
            il.Load(value);

            if (Type.GetTypeInfo().IsClass) {
                il.Emit(OpCodes.Stelem_Ref);
            }
            else if (Type == typeof(byte)) {
                il.Emit(OpCodes.Stelem_I1);
            }
            else if (Type == typeof(short) || Type == typeof(ushort)) {
                il.Emit(OpCodes.Stelem_I2);
            }
            else if (Type == typeof(int) || Type == typeof(uint)) {
                il.Emit(OpCodes.Stelem_I4);
            }
            else if (Type == typeof(long) || Type == typeof(ulong)) {
                il.Emit(OpCodes.Stelem_I8);
            }
            else if (Type == typeof(float)) {
                il.Emit(OpCodes.Stelem_R4);
            }
            else if (Type == typeof(double)) {
                il.Emit(OpCodes.Stelem_R8);
            }
            else {
                il.Emit(OpCodes.Stelem, Type);
            }
        }

    }
}