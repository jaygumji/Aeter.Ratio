/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Reflection;
using Aeter.Ratio.Reflection;

namespace Aeter.Ratio.Serialization.Reflection.Emit
{
    public class DictionaryMembers
    {
        public readonly Type VariableType;
        public readonly Type KeyType;
        public readonly Type ValueType;
        public readonly Type ElementType;
        public readonly MethodInfo Add;
        public readonly ConstructorInfo Constructor;

        public DictionaryMembers(ExtendedType dictionaryType)
        {
            var container = dictionaryType.Container.AsDictionary()!;
            KeyType = container.KeyType;
            ValueType = container.ValueType;
            ElementType = container.ElementType;
            VariableType = typeof (IDictionary<,>).MakeGenericType(KeyType, ValueType);

            Add = VariableType.FindMethod("Add", new[] {KeyType, ValueType});
            var instanceType = dictionaryType.Info.IsInterface
                ? typeof (Dictionary<,>).MakeGenericType(KeyType, ValueType)
                : dictionaryType.Ref;
            Constructor = instanceType.FindConstructor();
            if (Constructor == null) throw InvalidGraphException.NoParameterLessConstructor(dictionaryType.Ref);
        }

    }
}