/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection;
using Aeter.Ratio.Reflection.Emit;
using Aeter.Ratio.Reflection.Emit.Pointers;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Serialization.Reflection.Emit
{
    public class DynamicWriteTravellerBuilder
    {
        private static readonly DynamicWriteTravellerMembers Members = new DynamicWriteTravellerMembers(FactoryTypeProvider.Instance);

        private readonly ILPointer _visitorVariable;
        private readonly SerializableType _target;
        private readonly BuildTravellerMethodArgs _args;
        private readonly ITypeProvider _typeProvider;
        private readonly ILGenerator _il;

        public DynamicWriteTravellerBuilder(MethodBuilder builder, SerializableType target, BuildTravellerMethodArgs args, ITypeProvider typeProvider)
        {
            _target = target;
            _args = args;
            _typeProvider = typeProvider;
            _il = builder.GetILGenerator();
            _visitorVariable = new ILArgPointer(typeof(IWriteVisitor), 1);
        }

        public void BuildTravelWriteMethod()
        {
            var graphArgument = new ILArgPointer(_target.Type, 2);
            foreach (var property in _target.Properties) {
                GeneratePropertyCode(graphArgument, property);
            }
            _il.Emit(OpCodes.Ret);
        }

        private void GeneratePropertyCode(ILPointer graphPointer, SerializableProperty target)
        {
            var extPropertyType = target.Ext;
            var argsField = _args.GetArgsField(target);
            var argsFieldVariable = ILArgPointer.This.Field(argsField);
            if (target.Ext.IsValueOrNullableOfValue()) {
                var valueType = target.Ext.IsEnum()
                    ? target.Ext.GetUnderlyingEnumType()
                    : target.Ref.PropertyType;

                var propertyParameter = ILPointer.Property(graphPointer, target.Ref).AsNullable();

                _il.InvokeMethod(_visitorVariable, Members.VisitorVisitValue[valueType], propertyParameter, argsFieldVariable);
            }
            else if (extPropertyType.Classification == TypeClassification.Dictionary) {
                var container = extPropertyType.Container.AsDictionary();

                var dictionaryType = container.DictionaryInterfaceType;
                var cLocal = _il.NewLocal(dictionaryType);
                _il.Set(cLocal, ILPointer.Property(graphPointer, target.Ref).Cast(dictionaryType));

                _il.InvokeMethod(_visitorVariable, Members.VisitorVisit, cLocal, argsFieldVariable);

                _il.IfNotEqual(cLocal, null)
                    .Then(() => GenerateDictionaryCode(cLocal, container.ElementType))
                    .End();

                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, cLocal, argsFieldVariable);
            }
            else if (extPropertyType.Classification == TypeClassification.Collection) {
                var container = extPropertyType.Container.AsCollection();

                var collectionType = extPropertyType.Ref.IsArray && extPropertyType.Container.AsArray().Ranks > 1
                    ? extPropertyType.Ref
                    : container.CollectionInterfaceType;

                var cLocal = _il.NewLocal(collectionType);

                _il.Set(cLocal, ILPointer.Property(graphPointer, target.Ref).Cast(collectionType));

                _il.InvokeMethod(_visitorVariable, Members.VisitorVisit, cLocal, argsFieldVariable);

                _il.IfNotEqual(cLocal, null)
                    .Then(() => GenerateEnumerateCollectionContentCode(extPropertyType, cLocal))
                    .End();

                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, cLocal, argsFieldVariable);
            }
            else {
                var singleLocal = _il.NewLocal(target.Ref.PropertyType);
                _il.Set(singleLocal, ILPointer.Property(graphPointer, target.Ref));

                _il.InvokeMethod(_visitorVariable, Members.VisitorVisit, singleLocal, argsFieldVariable);

                var checkIfNullLabel = _il.NewLabel();
                checkIfNullLabel.TransferIfNull(singleLocal);

                GenerateChildCall(singleLocal);

                checkIfNullLabel.Mark();

                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, singleLocal, argsFieldVariable);
            }
        }

        private void GenerateDictionaryCode(ILVariable dictionary, Type elementType)
        {
            var elementTypeInfo = elementType.GetTypeInfo();
            _il.Enumerate(dictionary, it => {
                GenerateEnumerateContentCode(ILPointer.Property(it, elementTypeInfo.GetProperty("Key")), LevelType.DictionaryKey);
                GenerateEnumerateContentCode(ILPointer.Property(it, elementTypeInfo.GetProperty("Value")), LevelType.DictionaryValue);
            });
        }

        private void GenerateEnumerateContentCode(ILPointer valueParam, LevelType level)
        {
            var type = valueParam.Type;
            var extType = _typeProvider.Extend(type);

            var visitArgs = GetContentVisitArgs(extType, level);

            if (extType.IsValueOrNullableOfValue()) {
                _il.InvokeMethod(_visitorVariable, Members.VisitorVisitValue[type], valueParam.AsNullable(), visitArgs);
            }
            else if (extType.Classification == TypeClassification.Dictionary) {
                var container = extType.Container.AsDictionary();
                var elementType = container.ElementType;

                var dictionaryType = container.DictionaryInterfaceType;

                var dictionaryLocal = _il.NewLocal(dictionaryType);
                _il.Set(dictionaryLocal, valueParam.Cast(dictionaryType));

                _il.InvokeMethod(_visitorVariable, Members.VisitorVisit, dictionaryLocal, visitArgs);

                var elementTypeInfo = elementType.GetTypeInfo();
                _il.Enumerate(dictionaryLocal, it => {
                    GenerateEnumerateContentCode(ILPointer.Property(it, elementTypeInfo.GetProperty("Key")), LevelType.DictionaryKey);
                    GenerateEnumerateContentCode(ILPointer.Property(it, elementTypeInfo.GetProperty("Value")), LevelType.DictionaryValue);
                });

                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, dictionaryLocal, visitArgs);
            }
            else if (extType.Classification == TypeClassification.Collection) {
                var container = extType.Container.AsCollection();
                var collectionType = type.IsArray && extType.Container.AsArray().Ranks > 1
                    ? type
                    : container.CollectionInterfaceType;

                var collectionLocal = _il.NewLocal(collectionType);
                _il.Set(collectionLocal, valueParam.Cast(collectionType));

                _il.InvokeMethod(_visitorVariable, Members.VisitorVisit, collectionLocal, visitArgs);

                GenerateEnumerateCollectionContentCode(extType, collectionLocal);

                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, collectionLocal, visitArgs);
            }
            else {
                _il.InvokeMethod(_visitorVariable, Members.VisitorVisit, valueParam, visitArgs);

                GenerateChildCall(valueParam);

                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, valueParam, visitArgs);
            }
        }

        private void GenerateEnumerateCollectionContentCode(ExtendedType target, ILPointer collectionParameter)
        {
            if (target.TryGetArrayTypeInfo(out var arrayTypeInfo) && arrayTypeInfo.Ranks > 1) {
                if (arrayTypeInfo.Ranks > 3) {
                    throw new NotSupportedException("The serialization engine is limited to 3 ranks in arrays");
                }

                _il.ForLoop(0, new ILCallMethodSnippet(collectionParameter, Members.ArrayGetLength, 0), 1,
                    r0 => {
                        _il.InvokeMethod(_visitorVariable, Members.VisitorVisit, collectionParameter, Members.VisitArgsCollectionInCollection);
                        _il.ForLoop(0, new ILCallMethodSnippet(collectionParameter, Members.ArrayGetLength, 1), 1,
                            r1 => {
                                if (arrayTypeInfo.Ranks > 2) {
                                    _il.InvokeMethod(_visitorVariable, Members.VisitorVisit, collectionParameter, Members.VisitArgsCollectionInCollection);

                                    _il.ForLoop(0, new ILCallMethodSnippet(collectionParameter, Members.ArrayGetLength, 1), 1,
                                        r2 => GenerateEnumerateContentCode(
                                            new ILCallMethodSnippet(collectionParameter, target.Info.GetMethod("Get"), r0, r1, r2),
                                            LevelType.CollectionItem));

                                    _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, collectionParameter, Members.VisitArgsCollectionInCollection);
                                }
                                else {
                                    GenerateEnumerateContentCode(new ILCallMethodSnippet(collectionParameter, target.Info.GetMethod("Get"), r0, r1), LevelType.CollectionItem);
                                }
                            });
                        _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, collectionParameter, Members.VisitArgsCollectionInCollection);
                    });
            }
            else {
                _il.Enumerate(collectionParameter,
                    it => GenerateEnumerateContentCode(it, LevelType.CollectionItem));
            }
        }

        private static ILPointer GetContentVisitArgs(ExtendedType type, LevelType level)
        {
            if (!type.IsValueOrNullableOfValue()) {
                if (type.Classification == TypeClassification.Dictionary) {
                    if (level == LevelType.DictionaryKey)
                        return Members.VisitArgsDictionaryInDictionaryKey;
                    if (level == LevelType.DictionaryValue)
                        return Members.VisitArgsDictionaryInDictionaryValue;
                    return Members.VisitArgsDictionaryInCollection;
                }

                if (type.Classification == TypeClassification.Collection) {
                    if (level == LevelType.DictionaryKey)
                        return Members.VisitArgsCollectionInDictionaryKey;

                    if (level == LevelType.DictionaryValue)
                        return Members.VisitArgsCollectionInDictionaryValue;

                    return Members.VisitArgsCollectionInCollection;
                }
            }

            if (level == LevelType.DictionaryKey)
                return Members.VisitArgsDictionaryKey;

            if (level == LevelType.DictionaryValue)
                return Members.VisitArgsDictionaryValue;

            return Members.VisitArgsCollectionItem;
        }

        private void GenerateChildCall(ILPointer child)
        {
            var childTravellerInfo = _args.GetTraveller(child.Type);

            var field = ILPointer.Field(ILArgPointer.This, childTravellerInfo.Field);
            _il.InvokeMethod(field, childTravellerInfo.TravelWriteMethod, _visitorVariable, child);
        }
    }
}