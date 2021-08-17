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
            var visitArgsTypeInfo = visitArgsType.GetTypeInfo();
            VisitArgsCollectionItem = new ILStaticFieldVariable(visitArgsTypeInfo.GetField("CollectionItem"));
            VisitArgsDictionaryKey = new ILStaticFieldVariable(visitArgsTypeInfo.GetField("DictionaryKey"));
            VisitArgsDictionaryValue = new ILStaticFieldVariable(visitArgsTypeInfo.GetField("DictionaryValue"));
            VisitArgsCollectionInCollection = new ILStaticFieldVariable(visitArgsTypeInfo.GetField("CollectionInCollection"));
            VisitArgsDictionaryInCollection = new ILStaticFieldVariable(visitArgsTypeInfo.GetField("DictionaryInCollection"));
            VisitArgsDictionaryInDictionaryKey = new ILStaticFieldVariable(visitArgsTypeInfo.GetField("DictionaryInDictionaryKey"));
            VisitArgsDictionaryInDictionaryValue = new ILStaticFieldVariable(visitArgsTypeInfo.GetField("DictionaryInDictionaryValue"));
            VisitArgsCollectionInDictionaryKey = new ILStaticFieldVariable(visitArgsTypeInfo.GetField("CollectionInDictionaryKey"));
            VisitArgsCollectionInDictionaryValue = new ILStaticFieldVariable(visitArgsTypeInfo.GetField("CollectionInDictionaryValue"));

            var writeVisitorType = typeof (IWriteVisitor);
            var writeVisitorTypeInfo = writeVisitorType.GetTypeInfo();
            VisitorVisit = writeVisitorTypeInfo.GetMethod("Visit");
            VisitorLeave = writeVisitorTypeInfo.GetMethod("Leave");

            VisitorVisitValue = new Dictionary<Type, MethodInfo>();
            NullableConstructors = new Dictionary<Type, ConstructorInfo>();
            var nullableType = typeof (Nullable<>);
            foreach (var method in writeVisitorTypeInfo.GetMethods()
                .Where(m => m.Name == "VisitValue")) {

                var valueType = method.GetParameters()[0].ParameterType;
                var valueTypeExt = provider.Extend(valueType);

                VisitorVisitValue.Add(valueType, method);
                if (valueTypeExt.Classification == TypeClassification.Nullable) {
                    var innerType = valueTypeExt.Container.AsNullable().ElementType;
                    VisitorVisitValue.Add(innerType, method);

                    var nullableTypeInfo = nullableType.MakeGenericType(innerType).GetTypeInfo();
                    NullableConstructors.Add(innerType, nullableTypeInfo.GetConstructor(new []{innerType}));
                }
            }

            EnumeratorMoveNext = typeof (IEnumerator).GetTypeInfo().GetMethod("MoveNext");
            DisposableDispose = typeof (IDisposable).GetTypeInfo().GetMethod("Dispose");
            ArrayGetLength = typeof (Array).GetTypeInfo().GetMethod("GetLength");
        }

    }
}