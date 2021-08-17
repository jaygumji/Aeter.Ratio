/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;

namespace Aeter.Ratio.Reflection.Emit.Pointers
{
    public static class ILPointerExtensions
    {
        public static ILPointer AsNullable(this ILPointer parameter)
        {
            return new ILNullablePointer(parameter);
        }

        public static ILPointer Cast(this ILPointer parameter, Type toType)
        {
            return new ILCastPointer(parameter, toType);
        }

        public static ILPointer Call(this ILPointer instance, MethodInfo method, params ILPointer[] parameters)
        {
            return ILSnippet.Call(instance, method, parameters);
        }

        public static ILPointer Call(this ILPointer instance, string methodName, params ILPointer[] parameters)
        {
            return ILSnippet.Call(instance, methodName, parameters);
        }

        public static ILArrayElementVariable ElementAt(this IILPointer array, int index)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (index < 0) throw new IndexOutOfRangeException($"The index {index} must be a positive integer.");

            return new ILArrayElementVariable(array, index);
        }

        public static ILAddPointer Add(this IILPointer left, ILPointer right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return new ILAddPointer(left, right);
        }

        public static ILEqualPointer Equal(this IILPointer left, ILPointer right)
        {
            if (left == null) left = ILPointer.Null;
            if (right == null) right = ILPointer.Null;

            return new ILEqualPointer(left, right);
        }

        public static ILNegatePointer Negate(this IILPointer value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new ILNegatePointer(value);
        }

    }
}
