/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILCastPointer : ILPointer
    {
        private readonly IILPointer _pointer;
        private readonly Type _toType;

        public ILCastPointer(IILPointer pointer, Type toType)
        {
            _pointer = pointer;
            _toType = toType;
        }

        public override Type Type => _toType;

        protected override void Load(ILGenerator il)
        {
            _pointer.Load(il);
            if (_toType.IsValueType) {
                if (_pointer.Type == null || _pointer.Type == typeof(object)) {
                    il.Emit(OpCodes.Unbox_Any, _toType);
                }
                else {
                    var op = ILTypeGenerator.For(_toType);
                    op.EmitCast(il, _pointer.Type);
                }
            }
            else if (_pointer.Type != _toType) {
                il.Emit(OpCodes.Castclass, _toType);
            }
        }

    }
}