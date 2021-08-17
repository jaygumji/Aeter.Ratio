/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public abstract class ILTypeGenerator
    {
        public Type Type { get; }

        protected ILTypeGenerator(Type type)
        {
            Type = type;
        }

        public static ILTypeGenerator For(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (type == typeof(long)) return ILLongTypeGenerator.Instance;
            throw new ArgumentOutOfRangeException($"The type '{type.FullName}' does not have a type generator");
        }

        public abstract void EmitCast(ILGenerator il, Type type);
    }

    public abstract class ILNumberTypeGenerator : ILTypeGenerator
    {
        protected ILNumberTypeGenerator(Type type) : base(type)
        {
        }

        public abstract OpCode ConversionCode { get; }

        public override void EmitCast(ILGenerator il, Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (Type == type) return;

            var target = For(type);
            if (target is ILNumberTypeGenerator stdType) {
                il.Emit(stdType.ConversionCode);
                return;
            }

            var method = type.GetMethod("op_Implicit", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { Type }, null);
            if (method != null) {
                il.Emit(OpCodes.Call, method);
                return;
            }

            throw new ArgumentException($"Could not find a way to cast '{type.FullName}' into '{Type.FullName}'");
        }
    }

    public class ILByteTypeGenerator : ILNumberTypeGenerator
    {
        public ILByteTypeGenerator() : base(typeof(byte))
        {
        }

        public static ILByteTypeGenerator Instance { get; } = new ILByteTypeGenerator();

        public override OpCode ConversionCode => OpCodes.Conv_U1;
    }

    public class ILLongTypeGenerator : ILNumberTypeGenerator
    {
        public ILLongTypeGenerator() : base(typeof(long))
        {
        }

        public static ILLongTypeGenerator Instance { get; } = new ILLongTypeGenerator();

        public override OpCode ConversionCode => OpCodes.Conv_I8;
    }

    public class ClassTypeGenerator : ILTypeGenerator
    {
        public ClassTypeGenerator(Type type) : base(type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!type.IsClass) throw new ArgumentException("Type can not be a value type");
        }

        public override void EmitCast(ILGenerator il, Type type)
        {
            il.Emit(OpCodes.Castclass, type);
        }
    }

    public class ObjectTypeGenerator : ILTypeGenerator
    {
        public static ObjectTypeGenerator Instance { get; } = new ObjectTypeGenerator();

        private ObjectTypeGenerator() : base(typeof(object))
        {
        }

        public override void EmitCast(ILGenerator il, Type type)
        {
        }
    }
}
