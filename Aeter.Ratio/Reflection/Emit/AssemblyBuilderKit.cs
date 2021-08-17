/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class AssemblyBuilderKit
    {
        private readonly string _name;
        private readonly System.Reflection.Emit.AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _module;

        public AssemblyBuilderKit()
        {
            _name = "AeterRatioDynamicEmit." + Guid.NewGuid().ToString("N");

            _assemblyBuilder = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_name), AssemblyBuilderAccess.Run);
            _module = _assemblyBuilder.DefineDynamicModule(_name);
        }

        public ClassBuilder DefineClass(string name, Type inherits, Type[] implements)
        {
            const TypeAttributes attributes = TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.AutoClass | TypeAttributes.BeforeFieldInit;
            var classFullName = string.Concat(_name, '.', name);
            return new ClassBuilder(_module.DefineType(classFullName, attributes, inherits, implements));
        }
    }
}