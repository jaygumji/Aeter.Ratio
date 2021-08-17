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
            var visitArgsType = typeof(VisitArgs).GetTypeInfo();

            VisitArgsCollectionItem = new ILStaticFieldVariable(visitArgsType.GetField("CollectionItem"));
            VisitArgsDictionaryKey = new ILStaticFieldVariable(visitArgsType.GetField("DictionaryKey"));
            VisitArgsDictionaryValue = new ILStaticFieldVariable(visitArgsType.GetField("DictionaryValue"));
            VisitArgsCollectionInCollection = new ILStaticFieldVariable(visitArgsType.GetField("CollectionInCollection"));
            VisitArgsDictionaryInCollection = new ILStaticFieldVariable(visitArgsType.GetField("DictionaryInCollection"));
            VisitArgsDictionaryInDictionaryKey = new ILStaticFieldVariable(visitArgsType.GetField("DictionaryInDictionaryKey"));
            VisitArgsDictionaryInDictionaryValue = new ILStaticFieldVariable(visitArgsType.GetField("DictionaryInDictionaryValue"));
            VisitArgsCollectionInDictionaryKey = new ILStaticFieldVariable(visitArgsType.GetField("CollectionInDictionaryKey"));
            VisitArgsCollectionInDictionaryValue = new ILStaticFieldVariable(visitArgsType.GetField("CollectionInDictionaryValue"));

            var readVisitorType = typeof(IReadVisitor).GetTypeInfo();
            VisitorTryVisit = readVisitorType.GetMethod("TryVisit");
            VisitorLeave = readVisitorType.GetMethod("Leave");

            VisitorTryVisitValue = new Dictionary<Type, MethodInfo>();
            Nullable = new Dictionary<Type, NullableMembers>();

            foreach (var method in readVisitorType.GetMethods()
                .Where(m => m.Name == "TryVisitValue")) {

                var valueType = method.GetParameters()[1].ParameterType;
                if (valueType.IsByRef) valueType = valueType.GetElementType();
                var valueTypeExt = provider.Extend(valueType);

                VisitorTryVisitValue.Add(valueType, method);
                if (valueTypeExt.Classification == TypeClassification.Nullable) {
                    var innerType = valueTypeExt.Container.AsNullable().ElementType;
                    VisitorTryVisitValue.Add(innerType, method);

                    var nullableMembers = new NullableMembers(innerType);
                    Nullable.Add(innerType, nullableMembers);
                    Nullable.Add(valueType, nullableMembers);
                }
            }

            EnumeratorMoveNext = typeof(IEnumerator).GetTypeInfo().GetMethod("MoveNext");
            DisposableDispose = typeof(IDisposable).GetTypeInfo().GetMethod("Dispose");

            ExceptionNoDictionaryValue = typeof (InvalidGraphException).GetTypeInfo().GetMethod("NoDictionaryValue");
        }

    }
}