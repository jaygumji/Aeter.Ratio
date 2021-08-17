/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILInt32ConstantPointer : ILConstantPointer
    {
        private readonly int _value;

        public ILInt32ConstantPointer(int value) : base(typeof(int))
        {
            _value = value;
        }

        protected override void Load(ILGenerator il)
        {
            if (_value == 0) {
                il.Emit(OpCodes.Ldc_I4_0);
            }
            else if (_value == 1) {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else if (_value == 2) {
                il.Emit(OpCodes.Ldc_I4_2);
            }
            else if (_value == 3) {
                il.Emit(OpCodes.Ldc_I4_3);
            }
            else if (_value == 4) {
                il.Emit(OpCodes.Ldc_I4_4);
            }
            else if (_value == 5) {
                il.Emit(OpCodes.Ldc_I4_5);
            }
            else if (_value == 6) {
                il.Emit(OpCodes.Ldc_I4_6);
            }
            else if (_value == 7) {
                il.Emit(OpCodes.Ldc_I4_7);
            }
            else if (_value == 8) {
                il.Emit(OpCodes.Ldc_I4_8);
            }
            else if (_value == -1) {
                il.Emit(OpCodes.Ldc_I4_M1);
            }
            else {
                il.Emit(OpCodes.Ldc_I4, _value);
            }
        }
    }

}