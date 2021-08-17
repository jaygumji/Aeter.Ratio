/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection.Emit;
using Aeter.Ratio.Reflection.Emit.Pointers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Serialization.Reflection.Emit
{

    public class DynamicTravellerBuilder
    {
        private readonly DynamicGraphTravellerFactory _factory;
        private readonly ClassBuilder _classBuilder;
        private readonly SerializableTypeProvider _typeProvider;
        private readonly ConstructorBuilder _constructorBuilder;
        private readonly MethodBuilder _travelWriteMethod;
        private readonly MethodBuilder _travelReadMethod;

        public Type Type { get; }
        public DynamicTraveller DynamicTraveller { get; }

        public DynamicTravellerBuilder(DynamicGraphTravellerFactory factory, ClassBuilder classBuilder, SerializableTypeProvider typeProvider, Type type)
        {
            _factory = factory;
            _classBuilder = classBuilder;
            _typeProvider = typeProvider;
            Type = type;
            //_classBuilder.DefinePublicReadOnlyProperty("Level", typeof(LevelType), LevelType.Single);
            _constructorBuilder = _classBuilder.DefineConstructor(typeof(IVisitArgsFactory));

            var baseConstructor = typeof(object).GetTypeInfo().GetConstructor(Type.EmptyTypes);
            var il = _constructorBuilder.GetILGenerator();
            il.LoadThis();
            il.Emit(OpCodes.Call, baseConstructor);

            _travelWriteMethod = _classBuilder.DefineOverloadMethod("Travel", typeof(void), new[] { typeof(IWriteVisitor), Type });
            _travelReadMethod = _classBuilder.DefineOverloadMethod("Travel", typeof(void), new[] { typeof(IReadVisitor), Type });

            DynamicTraveller = new DynamicTraveller(_classBuilder.Type, _constructorBuilder, _travelWriteMethod, _travelReadMethod, factory.Members);
        }

        public void BuildTraveller()
        {
            if (_classBuilder.IsSealed) throw new InvalidOperationException("Classification builder is sealed");
            var target = _typeProvider.GetOrCreate(Type);
            var members = _factory.Members;
            var factoryArgument = new ILArgPointer(members.VisitArgsFactoryType, 1);


            var childTravellers = new Dictionary<Type, PendingChildGraphTraveller>();
            var argFields = new Dictionary<SerializableProperty, FieldInfo>();

            var il = _constructorBuilder.GetILGenerator();
            var travellerIndex = 0;
            foreach (var property in target.Properties) {
                var argField = _classBuilder.DefinePrivateField("_arg" + property.Ref.Name, members.VisitArgsType);
                var visitArgsCode = new ILCallMethodSnippet(factoryArgument, members.ConstructVisitArgsMethod, property.Ref.Name);
                il.Set(ILArgPointer.This, argField, visitArgsCode);
                argFields.Add(property, argField);

                if (!ReflectionAnalyzer.TryGetComplexTypes(property.Ext, out var types)) {
                    continue;
                }

                foreach (var type in types) {
                    if (childTravellers.ContainsKey(type)) {
                        continue;
                    }

                    var dynamicTraveller = _factory.PredefineDynamicTraveller(type);
                    var interfaceType = typeof(IGraphTraveller<>).MakeGenericType(type);
                    var fieldBuilder = _classBuilder.DefinePrivateField(string.Concat("_traveller", type.Name, ++travellerIndex), interfaceType);
                    childTravellers.Add(type, new PendingChildGraphTraveller {
                        Field = fieldBuilder,
                        TravelWriteMethod = dynamicTraveller.TravelWriteMethod,
                        TravelReadMethod = dynamicTraveller.TravelReadMethod
                    });

                    var getFactoryCode = ILSnippet.Call(factoryArgument, members.ConstructVisitArgsWithTypeMethod, type);
                    var newTraveller = ILPointer.New(dynamicTraveller.Constructor, getFactoryCode);
                    il.Set(ILArgPointer.This, fieldBuilder, newTraveller);
                }
            }
            il.Emit(OpCodes.Ret);

            var args = new BuildTravellerMethodArgs(childTravellers, argFields);
            BuildWriteMethods(target, args);
            BuildReadMethods(target, args);

            _classBuilder.Seal();
            DynamicTraveller.Complete(_classBuilder.Type);
        }

        private void BuildWriteMethods(SerializableType target, BuildTravellerMethodArgs args)
        {
            var typedMethodBuilder = _travelWriteMethod;
            var writeBuilder = new DynamicWriteTravellerBuilder(typedMethodBuilder, target, args, _typeProvider.Provider);
            writeBuilder.BuildTravelWriteMethod();

            var untypedMethodBuilder = _classBuilder.DefineOverloadMethod("Travel", typeof(void), new[] { typeof(IWriteVisitor), typeof(object) });
            var il = untypedMethodBuilder.GetILGenerator();
            il.InvokeMethod(ILArgPointer.This,
                typedMethodBuilder,
                ILPointer.Arg(1, typeof(IWriteVisitor)),
                ILPointer.Arg(2, typeof(object)).Cast(target.Type));

            il.Emit(OpCodes.Ret);
        }

        private void BuildReadMethods(SerializableType target, BuildTravellerMethodArgs args)
        {
            var typedMethodBuilder = _travelReadMethod;
            var readBuilder = new DynamicReadTravellerBuilder(typedMethodBuilder, target, args, _typeProvider.Provider);
            readBuilder.BuildTravelReadMethod();

            var untypedMethodBuilder = _classBuilder.DefineOverloadMethod("Travel", typeof(void), new[] { typeof(IReadVisitor), typeof(object) });
            var il = untypedMethodBuilder.GetILGenerator();

            il.InvokeMethod(ILArgPointer.This,
                typedMethodBuilder,
                ILPointer.Arg(1, typeof(IReadVisitor)),
                ILPointer.Arg(2, typeof(object)).Cast(target.Type));

            il.Emit(OpCodes.Ret);
        }

    }
}
