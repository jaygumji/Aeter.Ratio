/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aeter.Ratio.Reflection;

namespace Aeter.Ratio.Serialization.Reflection.Emit
{
    public class CollectionMembers
    {
        public readonly Type VariableType;
        public readonly Type ElementType;
        public readonly MethodInfo Add;
        public readonly ConstructorInfo Constructor;
        public readonly ExtendedType ElementTypeExt;
        public readonly MethodInfo ToArray;

        public CollectionMembers(ExtendedType collectionType)
        {
            ArrayContainerTypeInfo arrayTypeInfo;
            if (collectionType.TryGetArrayTypeInfo(out arrayTypeInfo)) {
                if (arrayTypeInfo.Ranks > 3)
                    throw new NotSupportedException("The serialization engine is limited to 3 ranks in arrays");
                if (arrayTypeInfo.Ranks == 3) {
                    var baseType = typeof (ICollection<>);
                    ElementType = baseType.MakeGenericType(baseType.MakeGenericType(arrayTypeInfo.ElementType));
                    ToArray = typeof (ArrayProvider).GetTypeInfo().GetMethod("To3DArray").MakeGenericMethod(arrayTypeInfo.ElementType);
                }
                else if (arrayTypeInfo.Ranks == 2) {
                    ElementType = typeof (ICollection<>).MakeGenericType(arrayTypeInfo.ElementType);
                    ToArray = typeof(ArrayProvider).GetTypeInfo().GetMethod("To2DArray").MakeGenericMethod(arrayTypeInfo.ElementType);
                }
                else {
                    ElementType = arrayTypeInfo.ElementType;
                    ToArray = typeof(ArrayProvider).GetTypeInfo().GetMethod("ToArray").MakeGenericMethod(arrayTypeInfo.ElementType);
                }
            }
            else {
                ElementType = collectionType.Container.AsCollection().ElementType;
            }

            ElementTypeExt = collectionType.Provider.Extend(ElementType);
            VariableType = typeof (ICollection<>).MakeGenericType(ElementType);

            Add = VariableType.GetTypeInfo().GetMethod("Add", new[] { ElementType });
            var instanceType = collectionType.Info.IsInterface || collectionType.Ref.IsArray
                ? typeof(List<>).MakeGenericType(ElementType)
                : collectionType.Ref;

            Constructor = instanceType.GetTypeInfo().GetConstructor(Type.EmptyTypes);
            if (Constructor == null) throw InvalidGraphException.NoParameterLessConstructor(collectionType.Ref);
        }
    }
}