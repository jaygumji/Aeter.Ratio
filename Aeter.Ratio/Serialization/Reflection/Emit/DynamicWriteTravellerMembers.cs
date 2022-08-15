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

namespace Aeter.Ratio.Serialization.Reflection.Emit
{
    public sealed class DynamicWriteTravellerMembers
    {

        public readonly ILPointer VisitArgsCollectionItem;
        public readonly ILPointer VisitArgsDictionaryKey;
        public readonly ILPointer VisitArgsDictionaryValue;
        public readonly ILPointer VisitArgsCollectionInCollection;
        public readonly ILPointer VisitArgsDictionaryInCollection;
        public readonly ILPointer VisitArgsDictionaryInDictionaryKey;
        public readonly ILPointer VisitArgsDictionaryInDictionaryValue;
        public readonly ILPointer VisitArgsCollectionInDictionaryKey;
        public readonly ILPointer VisitArgsCollectionInDictionaryValue;

        public readonly MethodInfo VisitorVisit;
        public readonly MethodInfo VisitorLeave;
        public readonly Dictionary<Type, MethodInfo> VisitorVisitValue;

        public readonly Dictionary<Type, ConstructorInfo> NullableConstructors; 

        public readonly MethodInfo EnumeratorMoveNext;
        public readonly MethodInfo DisposableDispose;

        public readonly MethodInfo ArrayGetLength;

        public DynamicWriteTravellerMembers(ITypeProvider provider)
        {
            var visitArgsType = typeof (VisitArgs);
            VisitArgsCollectionItem = visitArgsType.FindField(nameof(VisitArgs.CollectionItem)).AsILPointer();
            VisitArgsDictionaryKey = visitArgsType.FindField(nameof(VisitArgs.DictionaryKey)).AsILPointer();
            VisitArgsDictionaryValue = visitArgsType.FindField(nameof(VisitArgs.DictionaryValue)).AsILPointer();
            VisitArgsCollectionInCollection = visitArgsType.FindField(nameof(VisitArgs.CollectionInCollection)).AsILPointer();
            VisitArgsDictionaryInCollection = visitArgsType.FindField(nameof(VisitArgs.DictionaryInCollection)).AsILPointer();
            VisitArgsDictionaryInDictionaryKey = visitArgsType.FindField(nameof(VisitArgs.DictionaryInDictionaryKey)).AsILPointer();
            VisitArgsDictionaryInDictionaryValue = visitArgsType.FindField(nameof(VisitArgs.DictionaryInDictionaryValue)).AsILPointer();
            VisitArgsCollectionInDictionaryKey = visitArgsType.FindField(nameof(VisitArgs.CollectionInDictionaryKey)).AsILPointer();
            VisitArgsCollectionInDictionaryValue = visitArgsType.FindField(nameof(VisitArgs.CollectionInDictionaryValue)).AsILPointer();

            var writeVisitorType = typeof (IWriteVisitor);
            VisitorVisit = writeVisitorType.FindMethod("Visit");
            VisitorLeave = writeVisitorType.FindMethod("Leave");

            VisitorVisitValue = new Dictionary<Type, MethodInfo>();
            NullableConstructors = new Dictionary<Type, ConstructorInfo>();
            var nullableType = typeof (Nullable<>);
            foreach (var method in writeVisitorType.GetMethods()
                .Where(m => m.Name == "VisitValue")) {

                var valueType = method.GetParameters()[0].ParameterType;
                var valueTypeExt = provider.Extend(valueType);

                VisitorVisitValue.Add(valueType, method);
                if (valueTypeExt.Container.AsNullable(out var nullable)) {
                    var innerType = nullable.ElementType;
                    VisitorVisitValue.Add(innerType, method);

                    NullableConstructors.Add(innerType, nullableType.MakeGenericAndGetConstructor(innerType));
                }
            }

            EnumeratorMoveNext = TypeFindMemberExtensions.FindMethod(typeof(IEnumerator), nameof(IEnumerator.MoveNext));
            DisposableDispose = TypeFindMemberExtensions.FindMethod(typeof (IDisposable), nameof(IDisposable.Dispose));
            ArrayGetLength = TypeFindMemberExtensions.FindMethod(typeof (Array), nameof(Array.GetLength));
        }

    }
}