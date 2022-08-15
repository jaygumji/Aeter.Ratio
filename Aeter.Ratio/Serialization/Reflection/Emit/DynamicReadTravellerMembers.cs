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
    public sealed class DynamicReadTravellerMembers
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

        public readonly MethodInfo VisitorTryVisit;
        public readonly MethodInfo VisitorLeave;
        public readonly Dictionary<Type, MethodInfo> VisitorTryVisitValue;

        public readonly Dictionary<Type, NullableMembers> Nullable;

        public readonly MethodInfo EnumeratorMoveNext;
        public readonly MethodInfo DisposableDispose;
        
        public readonly MethodInfo ExceptionNoDictionaryValue;

        public DynamicReadTravellerMembers(ITypeProvider provider)
        {
            var visitArgsType = typeof(VisitArgs);

            VisitArgsCollectionItem = visitArgsType.FindField(nameof(VisitArgs.CollectionItem)).AsILPointer();
            VisitArgsDictionaryKey = visitArgsType.FindField(nameof(VisitArgs.DictionaryKey)).AsILPointer();
            VisitArgsDictionaryValue = visitArgsType.FindField(nameof(VisitArgs.DictionaryValue)).AsILPointer();
            VisitArgsCollectionInCollection = visitArgsType.FindField(nameof(VisitArgs.CollectionInCollection)).AsILPointer();
            VisitArgsDictionaryInCollection = visitArgsType.FindField(nameof(VisitArgs.DictionaryInCollection)).AsILPointer();
            VisitArgsDictionaryInDictionaryKey = visitArgsType.FindField(nameof(VisitArgs.DictionaryInDictionaryKey)).AsILPointer();
            VisitArgsDictionaryInDictionaryValue = visitArgsType.FindField(nameof(VisitArgs.DictionaryInDictionaryValue)).AsILPointer();
            VisitArgsCollectionInDictionaryKey = visitArgsType.FindField(nameof(VisitArgs.CollectionInDictionaryKey)).AsILPointer();
            VisitArgsCollectionInDictionaryValue = visitArgsType.FindField(nameof(VisitArgs.CollectionInDictionaryValue)).AsILPointer();

            var readVisitorType = typeof(IReadVisitor).GetTypeInfo();
            VisitorTryVisit = readVisitorType.FindMethod("TryVisit");
            VisitorLeave = readVisitorType.FindMethod("Leave");

            VisitorTryVisitValue = new Dictionary<Type, MethodInfo>();
            Nullable = new Dictionary<Type, NullableMembers>();

            foreach (var method in readVisitorType.GetMethods()
                .Where(m => m.Name == "TryVisitValue")) {

                Type valueType = method.GetParameters()[1].ParameterType;
                if (valueType.IsByRef) valueType = valueType.GetElementType()!;
                var valueTypeExt = provider.Extend(valueType);

                VisitorTryVisitValue.Add(valueType, method);
                if (valueTypeExt.Container.AsNullable(out var nullable)) {
                    var innerType = nullable.ElementType;
                    VisitorTryVisitValue.Add(innerType, method);

                    var nullableMembers = new NullableMembers(innerType);
                    Nullable.Add(innerType, nullableMembers);
                    Nullable.Add(valueType, nullableMembers);
                }
            }

            EnumeratorMoveNext = typeof(IEnumerator).FindMethod(nameof(IEnumerator.MoveNext));
            DisposableDispose = typeof(IDisposable).FindMethod(nameof(IDisposable.Dispose));

            ExceptionNoDictionaryValue = typeof(InvalidGraphException).FindMethod(nameof(InvalidGraphException.NoDictionaryValue));
        }

    }
}