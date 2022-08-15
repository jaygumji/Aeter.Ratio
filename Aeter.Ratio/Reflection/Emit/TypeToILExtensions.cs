/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aeter.Ratio.Reflection;
using Aeter.Ratio.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public static class TypeToILExtensions
    {
        public static ILPointer AsILPointer(this FieldInfo field)
        {
            if (field.IsStatic) return new ILStaticFieldVariable(field);
            throw new ArgumentException("An instance field requires an instance parameter");
        }
        public static ILPointer AsILPointer(this FieldInfo field, ILPointer instance)
        {
            if (!field.IsStatic) return new ILInstanceFieldVariable(instance, field);
            throw new ArgumentException("A static field does not require an instance");
        }
    }
}
