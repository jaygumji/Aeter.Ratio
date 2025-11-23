/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;

namespace Aeter.Ratio.Reflection.Emit
{
    /// <summary>
    /// Provides convenience conversions from reflection metadata to IL pointers.
    /// </summary>
    public static class TypeToILExtensions
    {
        /// <summary>
        /// Wraps a static field as an <see cref="ILPointer"/>.
        /// </summary>
        public static ILPointer AsILPointer(this FieldInfo field)
        {
            if (field.IsStatic) return new ILStaticFieldVariable(field);
            throw new ArgumentException("An instance field requires an instance parameter");
        }
        /// <summary>
        /// Wraps an instance field as an <see cref="ILPointer"/> bound to <paramref name="instance"/>.
        /// </summary>
        public static ILPointer AsILPointer(this FieldInfo field, ILPointer instance)
        {
            if (!field.IsStatic) return new ILInstanceFieldVariable(instance, field);
            throw new ArgumentException("A static field does not require an instance");
        }
    }
}
