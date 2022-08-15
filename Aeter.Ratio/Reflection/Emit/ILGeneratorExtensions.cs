/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection.Emit.Pointers;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public static class ILGeneratorExtensions
    {

        public static void Load(this ILGenerator gen, IILPointer? pointer)
        {
            if (pointer == null) pointer = ILPointer.Null;
            pointer.Load(gen);
        }

        public static void Load(this ILGenerator gen, ILPointer? pointer)
        {
            if (pointer == null) pointer = ILPointer.Null;
            ((IILPointer)pointer).Load(gen);
        }

        public static void LoadThis(this ILGenerator gen)
        {
            ((IILPointer)ILArgPointer.This).Load(gen);
        }

        public static void LoadAddress(this ILGenerator gen, IILPointer pointer)
        {
            pointer.LoadAddress(gen);
        }

        public static void LoadAddress(this ILGenerator gen, ILPointer? pointer)
        {
            if (pointer == null) pointer = ILPointer.Null;
            ((IILPointer)pointer).LoadAddress(gen);
        }

        public static void Set(this ILGenerator gen, ILLocalVariable variable)
        {
            ((IILVariable)variable).PreSet(gen);
            ((IILVariable)variable).Set(gen);
        }

        public static void Set(this ILGenerator gen, IILVariable variable, IILPointer? value)
        {
            if (value == null) value = ILPointer.Null;

            variable.PreSet(gen);
            value.Load(gen);
            variable.Set(gen);
        }

        public static void Set(this ILGenerator gen, ILVariable variable, ILPointer? value)
        {
            if (value == null) value = ILPointer.Null;

            ((IILVariable)variable).PreSet(gen);
            ((IILPointer)value).Load(gen);
            ((IILVariable)variable).Set(gen);
        }

        public static void Set(this ILGenerator il, ILPointer instance, FieldInfo field, ILPointer value)
        {
            var f = ILPointer.Field(instance, field);
            il.Set(f, value);
        }

        public static void Set(this ILGenerator il, ILPointer instance, PropertyInfo property, ILPointer? value)
        {
            var p = ILPointer.Property(instance, property);
            il.Set(p, value);
        }

        public static void Generate(this ILGenerator gen, IILSnippet code)
        {
            code.Generate(gen);
        }

        public static ILLocalVariable NewLocal(this ILGenerator gen, Type type)
        {
            return gen.DeclareLocal(type);
        }

        public static ILLabel NewLabel(this ILGenerator il)
        {
            return new ILLabel(il);
        }

        public static void InvokeMethod(this ILGenerator il, ILPointer instance, string methodName, params ILPointer[] parameters)
        {
            var call = ILSnippet.Call(instance, methodName, parameters);
            il.Generate(call);
        }

        public static void InvokeMethod(this ILGenerator il, ILPointer instance, MethodInfo method, params ILPointer[] parameters)
        {
            var call = ILSnippet.Call(instance, method, parameters);
            il.Generate(call);
        }

        public static void InvokeMethod(this ILGenerator il, MethodInfo method, params ILPointer[] parameters)
        {
            var call = ILSnippet.Call(method, parameters);
            il.Generate(call);
        }

        public static void Construct(this ILGenerator il, ConstructorInfo constructor, params ILPointer[] parameters)
        {
            il.Load(ILPointer.New(constructor, parameters));
        }

        public static void Enumerate(this ILGenerator il, ILPointer enumerable, ILGenerationHandler<ILPointer> iterateBody)
        {
            il.Generate(new ILEnumerateSnippet(enumerable, iterateBody));
        }

        public static void Enumerate(this ILGenerator il, ILPointer enumerable, ILGenerationMethodHandler<ILPointer> iterateBody)
        {
            il.Generate(new ILEnumerateSnippet(enumerable, iterateBody));
        }

        public static void WhileLoop(this ILGenerator il, ILGenerationHandler conditionHandler, ILGenerationHandler bodyHandler)
        {
            il.Generate(new ILWhileLoopSnippet(conditionHandler, bodyHandler));
        }

        public static void WhileLoop(this ILGenerator il, ILGenerationMethodHandler conditionHandler, ILGenerationMethodHandler bodyHandler)
        {
            il.Generate(new ILWhileLoopSnippet(conditionHandler, bodyHandler));
        }

        public static void AreEqual(this ILGenerator il, ILPointer? left, ILPointer? right)
        {
            if (left == null) left = ILPointer.Null;
            if (right == null) right = ILPointer.Null;

            il.Load(left.Equal(right));
        }

        public static void Throw(this ILGenerator il, ILPointer? exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            ((IILPointer)exception).Load(il);
            il.Emit(OpCodes.Throw);
        }

        public static void Increment(this ILGenerator il, ILVariable value, ILPointer valueToAdd)
        {
            il.Set(value, value.Add(valueToAdd));
        }

        public static void ForLoop(this ILGenerator il, ILPointer initialValue, ILPointer lesserThan, ILPointer increment, ILGenerationHandler<ILPointer> body)
        {
            var loop = new ILForLoopSnippet(initialValue, value =>
            {
                il.Load(value);
                il.Load(lesserThan);
                il.Emit(OpCodes.Clt);
            }, body, increment);
            il.Generate(loop);
        }

        public static ILChainIfCondition IfEqual(this ILGenerator il, ILPointer left, ILPointer right)
        {
            return new ILChainIfCondition(il, () => il.AreEqual(left, right));
        }

        public static ILChainIfCondition IfNotEqual(this ILGenerator il, ILPointer? left, ILPointer? right)
        {
            return new ILChainIfCondition(il, () =>
            {
                il.Load(left.Equal(right).Negate());
            });
        }

        public static ILChainTryCatchBlock Try(this ILGenerator il, Action<ILGenerator> tryBlockGenerator)
        {
            return new ILChainTryCatchBlock(il, tryBlockGenerator);
        }

        public static ILChainTryCatchBlock Try(this ILGenerator il, Action tryBlockGenerator)
        {
            return new ILChainTryCatchBlock(il, tryBlockGenerator);
        }

        public static void Negate(this ILGenerator il)
        {
            il.Load(0);
            il.Emit(OpCodes.Ceq);
        }

    }
}
