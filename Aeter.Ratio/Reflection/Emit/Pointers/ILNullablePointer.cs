/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILNullablePointer : ILPointer
    {

        private readonly IILPointer _pointer;
        public override Type? Type => _pointer.Type;

        public ILNullablePointer(IILPointer pointer)
        {
            if (pointer.Type == null) throw new NotSupportedException("The pointer does not have a well defined pointer type");

            _pointer = pointer;
        }

        protected override void Load(ILGenerator il)
        {
            _pointer.Load(il);

            var type = _pointer.Type!;

            if (!type.IsValueType) return;
            if (type.IsGenericType) {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    return;
                }
            }
            else {
                if (type.IsEnum) {
                    type = Enum.GetUnderlyingType(type);
                }
            }

            var nullableType = type.AsNullable();
            var constructor = nullableType.FindConstructor(type);

            il.Emit(OpCodes.Newobj, constructor);
        }

    }
}