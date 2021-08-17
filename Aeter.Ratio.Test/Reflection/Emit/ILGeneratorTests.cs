/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Aeter.Ratio.Reflection.Emit;
using Xunit;

namespace Aeter.Ratio.Test.Reflection.Emit
{
    public class ILGeneratorTests
    {

        [Fact]
        public void LoadConstant()
        {
            var mt = new DynamicMethod("Test$" + Guid.NewGuid(), typeof(string), Type.EmptyTypes);
            var il = mt.GetILGenerator();
            il.Load("Hello World");
            il.Emit(OpCodes.Ret);
            var m = (Func<string>)mt.CreateDelegate(typeof(Func<string>));

            Assert.Equal("Hello World", m.Invoke());
        }

        [Fact]
        public void InvokeMethod()
        {
            var mt = new DynamicMethod("Test$" + Guid.NewGuid(), typeof(string), Type.EmptyTypes);
            var il = mt.GetILGenerator();
            il.InvokeMethod(EmitTestContext.GetTestTextMethodInfo);
            il.Emit(OpCodes.Ret);
            var m = (Func<string>)mt.CreateDelegate(typeof(Func<string>));

            Assert.Equal("Hello World", m.Invoke());
        }

        [Fact]
        public void CreateTestClassInstance()
        {
            var mt = new DynamicMethod("Test$" + Guid.NewGuid(), typeof(string), Type.EmptyTypes);
            var il = mt.GetILGenerator();
            var instance = ILPointer.New(EmitTestClass.ConstructorInfo, "Hello World");
            var property = ILPointer.Property(instance, typeof(IEmitTest).GetProperty("Message"));
            il.Load(property);
            il.Emit(OpCodes.Ret);
            var m = (Func<string>)mt.CreateDelegate(typeof(Func<string>));

            Assert.Equal("Hello World", m.Invoke());
        }

        [Fact]
        public void IterateList()
        {
            var mt = new DynamicMethod("Test$" + Guid.NewGuid(), typeof(string), new [] {typeof(List<string>)});
            var il = mt.GetILGenerator();
            var list = ILPointer.Arg(0, typeof(List<string>));

            var concatMethod = typeof(string).GetMethod("Concat", new[] {typeof(object), typeof(object)});
            var res = il.NewLocal(typeof(string));
            il.Enumerate(list, cur => il.Set(res, ILSnippet.Call(concatMethod, res, cur)));
            il.Load(res);
            il.Emit(OpCodes.Ret);
            var m = (Func<List<string>, string>)mt.CreateDelegate(typeof(Func<List<string>, string>));

            Assert.Equal("HelloWorld", m.Invoke(new List<string> {"Hello", "World"}));
        }

        [Fact]
        public void Test()
        {
            
        }

    }
}
