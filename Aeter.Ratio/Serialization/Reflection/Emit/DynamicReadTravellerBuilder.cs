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
    public class DynamicReadTravellerBuilder
    {
        private static readonly DynamicReadTravellerMembers Members = new DynamicReadTravellerMembers(FactoryTypeProvider.Instance);

        private readonly SerializableType _target;
        private readonly BuildTravellerMethodArgs _args;
        private readonly ILGenerator _il;
        private readonly ILArgPointer _visitorVariable;
        private readonly ITypeProvider _typeProvider;

        public DynamicReadTravellerBuilder(MethodBuilder builder, SerializableType target, BuildTravellerMethodArgs args, ITypeProvider typeProvider)
        {
            _target = target;
            _args = args;
            _typeProvider = typeProvider;
            _il = builder.GetILGenerator();
            _visitorVariable = new ILArgPointer(typeof(IReadVisitor), 1);
        }

        public void BuildTravelReadMethod()
        {
            var graphArgument = new ILArgPointer(_target.Type, 2);
            foreach (var property in _target.Properties) {
                GeneratePropertyCode(graphArgument, property);
            }
            _il.Emit(OpCodes.Ret);
        }

        private void GeneratePropertyCode(ILArgPointer graphArg, SerializableProperty target)
        {
            var extPropertyType = target.Ext;
            var argsField = _args.GetArgsField(target);
            var argsFieldVariable = ILPointer.Field(ILArgPointer.This, argsField);
            if (extPropertyType.IsValueOrNullableOfValue()) {
                var isNullable = extPropertyType.Classification == TypeClassification.Nullable;
                var isEnum = extPropertyType.IsEnum();
                var isValueType = extPropertyType.Info.IsValueType;

                var mediatorPropertyType = isEnum ? extPropertyType.GetUnderlyingEnumType() : extPropertyType.Ref;
                var valueType = !isNullable && isValueType ? Members.Nullable[mediatorPropertyType].NullableType : mediatorPropertyType;

                var valueLocal = _il.NewLocal(valueType);

                _il.InvokeMethod(_visitorVariable, Members.VisitorTryVisitValue[valueType], argsFieldVariable, valueLocal);

                if (isValueType && !isNullable) {
                    var labelValueNotFound = _il.NewLabel();
                    labelValueNotFound.TransferLongIfFalse();

                    _il.Load(valueLocal
                        .Call(Members.Nullable[mediatorPropertyType].GetHasValue)
                        .Negate());

                    var nullableHasValueLabel = _il.NewLabel();
                    nullableHasValueLabel.TransferLong();

                    labelValueNotFound.Mark();
                    _il.Load(true);
                    nullableHasValueLabel.Mark();
                }
                else {
                    _il.Negate();
                }

                var skipSetValueLabel = _il.NewLabel();
                skipSetValueLabel.TransferLongIfTrue();

                var valueToAdd = isValueType && !isNullable
                    ? valueLocal.Call(Members.Nullable[mediatorPropertyType].GetValue)
                    : valueLocal;

                _il.Set(graphArg, target.Ref, valueToAdd);

                skipSetValueLabel.Mark();
            }
            else if (extPropertyType.Classification == TypeClassification.Dictionary) {
                var stateLocal = _il.NewLocal(typeof(ValueState));
                _il.Set(stateLocal, _visitorVariable.Call(Members.VisitorTryVisit, argsFieldVariable));

                var endLabel = _il.NewLabel();
                endLabel.TransferLongIfTrue(stateLocal.Equal((int)ValueState.NotFound));

                _il.AreEqual(stateLocal, (int)ValueState.Found);

                var nullLabel = _il.NewLabel();
                nullLabel.TransferLongIfFalse();

                var dictionaryLocal = GenerateDictionaryEnumerateCode(target.Ref.PropertyType, target.Ref.Name);

                _il.Set(graphArg, target.Ref, dictionaryLocal.Cast(target.Ref.PropertyType));

                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, argsFieldVariable);

                endLabel.TransferLong();

                nullLabel.Mark();
                _il.Set(graphArg, target.Ref, null);

                endLabel.Mark();
            }
            else if (extPropertyType.Classification == TypeClassification.Collection) {
                _il.InvokeMethod(_visitorVariable, Members.VisitorTryVisit, argsFieldVariable);
                var stateLocal = _il.NewLocal(typeof(ValueState));
                _il.Set(stateLocal);

                _il.AreEqual(stateLocal, (int)ValueState.NotFound);

                var endLabel = _il.NewLabel();
                endLabel.TransferLongIfTrue();

                _il.AreEqual(stateLocal, (int)ValueState.Found);

                var nullLabel = _il.NewLabel();
                nullLabel.TransferLongIfFalse();

                var collectionParam = GenerateCollectionContent(extPropertyType, target.Ref.Name);

                _il.Set(graphArg, target.Ref, collectionParam);
                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, argsFieldVariable);
                endLabel.TransferLong();

                nullLabel.Mark();
                _il.Set(graphArg, target.Ref, null);

                endLabel.Mark();
            }
            else {
                _il.InvokeMethod(_visitorVariable, Members.VisitorTryVisit, argsFieldVariable);
                var stateLocal = _il.NewLocal(typeof(ValueState));
                _il.Set(stateLocal);

                _il.IfNotEqual(stateLocal, (int)ValueState.NotFound)
                    .Then(() => {
                        _il.IfEqual(stateLocal, (int)ValueState.Found)
                            .Then(() => {
                                var singleLocal = _il.NewLocal(extPropertyType.Ref);
                                GenerateCreateAndChildCallCode(singleLocal);

                                _il.Set(graphArg, target.Ref, singleLocal);
                                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, argsFieldVariable);
                            }).Else(() => {
                                _il.Set(graphArg, target.Ref, null);
                            }).End();
                    }).End();
            }
        }

        private ILPointer GenerateCollectionContent(ExtendedType target, string refName)
        {
            var collectionMembers = new CollectionMembers(target);
            var isValueType = collectionMembers.ElementType.GetTypeInfo().IsValueType;

            var collectionLocal = _il.NewLocal(collectionMembers.VariableType);
            var collection = ILPointer.New(collectionMembers.Constructor)
                .Cast(collectionMembers.VariableType);
            _il.Set(collectionLocal, collection);

            var valueLocal = DeclareCollectionItemLocal(collectionMembers.ElementType); ;

            if (collectionMembers.ElementTypeExt.IsValueOrNullableOfValue()) {
                _il.WhileLoop(il => { // While condition
                    _il.InvokeMethod(_visitorVariable, Members.VisitorTryVisitValue[collectionMembers.ElementType], Members.VisitArgsCollectionItem, valueLocal);

                    var valueNotFoundLabel = _il.NewLabel();
                    valueNotFoundLabel.TransferLongIfFalse();

                    if (isValueType) {
                        _il.InvokeMethod(valueLocal, Members.Nullable[collectionMembers.ElementType].GetHasValue);
                    }
                    else {
                        _il.AreEqual(valueLocal, ILPointer.Null);
                        _il.Negate();
                    }

                    var isNullLabel = _il.NewLabel();
                    isNullLabel.TransferLong();

                    valueNotFoundLabel.Mark();
                    _il.Load(0);
                    isNullLabel.Mark();
                }, il => {
                    _il.Load(collectionLocal);

                    GenerateLoadParamValueCode(valueLocal);

                    _il.EmitCall(OpCodes.Callvirt, collectionMembers.Add, null);
                });
            }
            else if (collectionMembers.ElementTypeExt.Classification == TypeClassification.Dictionary) {
                _il.WhileLoop(il => { // Condition
                    var callTryVisit = new ILCallMethodSnippet(_visitorVariable, Members.VisitorTryVisit, Members.VisitArgsDictionaryInCollection);
                    _il.AreEqual(callTryVisit, (int)ValueState.Found);
                }, il => {
                    var contentParam = GenerateDictionaryEnumerateCode(collectionMembers.ElementType, refName);
                    _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, Members.VisitArgsDictionaryInCollection);
                    _il.InvokeMethod(collectionLocal, collectionMembers.Add, contentParam);
                });
            }
            else if (collectionMembers.ElementTypeExt.Classification == TypeClassification.Collection) {
                _il.WhileLoop(il => { // Condition
                    var callTryVisit = new ILCallMethodSnippet(_visitorVariable, Members.VisitorTryVisit, Members.VisitArgsCollectionInCollection);
                    _il.AreEqual(callTryVisit, (int)ValueState.Found);
                }, il => {
                    var contentParam = GenerateCollectionContent(collectionMembers.ElementTypeExt, refName);
                    _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, Members.VisitArgsCollectionInCollection);
                    _il.InvokeMethod(collectionLocal, collectionMembers.Add, contentParam);
                });
            }
            else {
                _il.WhileLoop(il => {
                    var callTryVisit = new ILCallMethodSnippet(_visitorVariable, Members.VisitorTryVisit, Members.VisitArgsCollectionItem);
                    _il.AreEqual(callTryVisit, (int)ValueState.Found);
                }, il => {
                    GenerateCreateAndChildCallCode(valueLocal);
                    _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, Members.VisitArgsCollectionItem);
                    _il.InvokeMethod(collectionLocal, collectionMembers.Add, valueLocal);
                });
            }
            if (target.Ref.IsArray)
                return new ILCallMethodSnippet(collectionMembers.ToArray, collectionLocal);

            return collectionLocal;
        }

        private ILLocalVariable GenerateDictionaryEnumerateCode(Type type, string refName)
        {
            var extType = _typeProvider.Extend(type);
            var dictionaryMembers = new DictionaryMembers(extType);

            var local = _il.NewLocal(dictionaryMembers.VariableType);
            _il.Construct(dictionaryMembers.Constructor);
            _il.Set(local);

            var keyType = Members.Nullable.TryGetValue(dictionaryMembers.KeyType, out var keyNullableMembers)
                ? keyNullableMembers.NullableType
                : dictionaryMembers.KeyType;

            var conditionLabel = _il.NewLabel();
            var bodyLabel = _il.NewLabel();

            var keyTypeExt = _typeProvider.Extend(keyType);

            // First of all, transfer to the condition part
            conditionLabel.TransferLong();

            // Mark that we enter the body of the loop
            bodyLabel.Mark();
            ILPointer keyParam;
            if (keyTypeExt.IsValueOrNullableOfValue()) {
                keyParam = _il.NewLocal(keyType);
            }
            else if (keyTypeExt.Classification == TypeClassification.Dictionary) {
                keyParam = GenerateDictionaryEnumerateCode(keyType, refName);

                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, Members.VisitArgsDictionaryInDictionaryKey);
            }
            else if (keyTypeExt.Classification == TypeClassification.Collection) {
                keyParam = GenerateCollectionContent(keyTypeExt, refName);

                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, Members.VisitArgsCollectionInDictionaryKey);
            }
            else {
                var keyLocal = _il.NewLocal(keyType);
                GenerateCreateAndChildCallCode(keyLocal);
                keyParam = keyLocal;
                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, Members.VisitArgsDictionaryKey);
            }

            var throwExceptionLabel = _il.NewLabel();

            var hasNullableMembers = Members.Nullable.TryGetValue(
                dictionaryMembers.ValueType, out var valueNullableMembers);
            var valueType = dictionaryMembers.ValueType;
            var extValueType = _typeProvider.Extend(valueType);

            ILPointer valueParam;
            if (extValueType.IsValueOrNullableOfValue()) {
                var loadTrueLabel = _il.NewLabel();
                var checkIfErrorLabel = _il.NewLabel();

                valueParam = _il.NewLocal(hasNullableMembers ? valueNullableMembers.NullableType : dictionaryMembers.ValueType);
                _il.InvokeMethod(_visitorVariable, Members.VisitorTryVisitValue[dictionaryMembers.ValueType], Members.VisitArgsDictionaryValue, valueParam);
                loadTrueLabel.TransferLongIfFalse();

                if (hasNullableMembers) {
                    _il.InvokeMethod(valueParam, valueNullableMembers.GetHasValue);
                    _il.Negate();
                }
                else {
                    _il.AreEqual(valueParam, null);
                }
                checkIfErrorLabel.TransferLong();
                loadTrueLabel.Mark();
                _il.Load(true);
                checkIfErrorLabel.Mark();
                throwExceptionLabel.TransferLongIfTrue();
            }
            else if (extValueType.Classification == TypeClassification.Dictionary) {
                var callTryVisitValue = new ILCallMethodSnippet(_visitorVariable, Members.VisitorTryVisit, Members.VisitArgsDictionaryInDictionaryValue);
                _il.AreEqual(callTryVisitValue, (int)ValueState.Found);
                throwExceptionLabel.TransferLongIfFalse();

                valueParam = GenerateDictionaryEnumerateCode(valueType, refName);

                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, Members.VisitArgsDictionaryInDictionaryValue);
            }
            else if (extValueType.Classification == TypeClassification.Collection) {
                var callTryVisit = new ILCallMethodSnippet(_visitorVariable, Members.VisitorTryVisit, Members.VisitArgsCollectionInDictionaryValue);
                _il.AreEqual(callTryVisit, (int)ValueState.Found);
                throwExceptionLabel.TransferLongIfFalse();

                valueParam = GenerateCollectionContent(_typeProvider.Extend(valueType), refName);

                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, Members.VisitArgsCollectionInDictionaryValue);
            }
            else {
                var callTryVisit = new ILCallMethodSnippet(_visitorVariable, Members.VisitorTryVisit, Members.VisitArgsDictionaryValue);
                _il.AreEqual(callTryVisit, (int)ValueState.Found);
                throwExceptionLabel.TransferLongIfFalse();

                var valueLocal = _il.NewLocal(hasNullableMembers ? valueNullableMembers.NullableType : dictionaryMembers.ValueType);
                GenerateCreateAndChildCallCode(valueLocal);
                valueParam = valueLocal;

                _il.InvokeMethod(_visitorVariable, Members.VisitorLeave, Members.VisitArgsDictionaryValue);
            }

            _il.Load(local);
            GenerateLoadParamValueCode(keyParam);
            GenerateLoadParamValueCode(valueParam);

            _il.EmitCall(OpCodes.Callvirt, dictionaryMembers.Add, null);
            conditionLabel.TransferLong();

            throwExceptionLabel.Mark();
            _il.Throw(new ILCallMethodSnippet(Members.ExceptionNoDictionaryValue, refName));

            conditionLabel.Mark();

            if (keyTypeExt.IsValueOrNullableOfValue()) {
                var loadZeroLabel = _il.NewLabel();
                var checkConditionLabel = _il.NewLabel();

                _il.InvokeMethod(_visitorVariable, Members.VisitorTryVisitValue[keyType], Members.VisitArgsDictionaryKey, keyParam);
                loadZeroLabel.TransferLongIfFalse();

                if (keyTypeExt.Info.IsValueType) {
                    _il.InvokeMethod(keyParam, Members.Nullable[keyType].GetHasValue);
                    checkConditionLabel.TransferLong();
                }
                else {
                    _il.AreEqual(keyParam, ILPointer.Null);
                    _il.Negate();
                    checkConditionLabel.TransferLong();
                }

                loadZeroLabel.Mark();
                _il.Load(false);
                checkConditionLabel.Mark();
                bodyLabel.TransferLongIfTrue();
            }
            else if (keyTypeExt.Classification == TypeClassification.Dictionary) {
                var callTryVisitKey = new ILCallMethodSnippet(_visitorVariable, Members.VisitorTryVisit, Members.VisitArgsDictionaryInDictionaryKey);
                _il.AreEqual(callTryVisitKey, (int)ValueState.Found);
                bodyLabel.TransferLongIfTrue();
            }
            else if (keyTypeExt.Classification == TypeClassification.Collection) {
                var callTryVisitKey = new ILCallMethodSnippet(_visitorVariable, Members.VisitorTryVisit, Members.VisitArgsCollectionInDictionaryKey);
                _il.AreEqual(callTryVisitKey, (int)ValueState.Found);
                bodyLabel.TransferLongIfTrue();
            }
            else {
                var callTryVisit = new ILCallMethodSnippet(_visitorVariable, Members.VisitorTryVisit, Members.VisitArgsDictionaryKey);
                _il.AreEqual(callTryVisit, (int)ValueState.Found);
                bodyLabel.TransferLongIfTrue();
            }

            return local;
        }

        private ILLocalVariable DeclareCollectionItemLocal(Type type)
        {
            var isValueType = type.GetTypeInfo().IsValueType;
            var valueLocal = _il.NewLocal(isValueType ? Members.Nullable[type].NullableType : type);
            return valueLocal;
        }

        private void GenerateLoadParamValueCode(ILPointer param)
        {
            var type = param.Type;
            var extType = _typeProvider.Extend(type);
            if (extType.Classification == TypeClassification.Nullable)
                type = extType.Container.AsNullable().ElementType;

            if (extType.Info.IsValueType)
                _il.InvokeMethod(param, Members.Nullable[type].GetValue);
            else
                _il.Load(param);
        }

        private void GenerateCreateAndChildCallCode(ILLocalVariable local)
        {
            var type = local.Type;

            var constructor = type.GetTypeInfo().GetConstructor(Type.EmptyTypes);
            if (constructor == null)
                throw InvalidGraphException.NoParameterLessConstructor(type);

            _il.Construct(constructor);
            _il.Set(local);

            var childTravellerInfo = _args.GetTraveller(type);

            var field = ILArgPointer.This.Field(childTravellerInfo.Field);
            _il.InvokeMethod(field, childTravellerInfo.TravelReadMethod, _visitorVariable, local);
        }
    }
}
