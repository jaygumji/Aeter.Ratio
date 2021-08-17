/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILNullablePointer : ILPointer
    {

        private readonly IILPointer _pointer;
        public override Type Type => _pointer.Type;

        public ILNullablePointer(IILPointer pointer)
        {
            if (pointer.Type == null) throw new NotSupportedException("The pointer does not have a well defined pointer type");

            _pointer = pointer;
        }

        protected override void Load(ILGenerator il)
        {
            _pointer.Load(il);

            var type = _pointer.Type;
            var typeInfo = type.GetTypeInfo();

            if (!typeInfo.IsValueType) return;
            if (typeInfo.IsGenericType) {
                if (typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    return;
                }
            }
            else {
                if (typeInfo.IsEnum) {
                    type = Enum.GetUnderlyingType(type);
                }
            }

            var nullableType = type.AsNullable();
            var nullableTypeInfo = nullableType.GetTypeInfo();
            var constructor = nullableTypeInfo.GetConstructor(new[] { type });

            il.Emit(OpCodes.Newobj, constructor);
        }

    }
}