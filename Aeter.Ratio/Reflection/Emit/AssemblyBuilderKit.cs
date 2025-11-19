/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    /// <summary>
    /// Helper that exposes a simplified API for defining transient dynamic assemblies and types.
    /// </summary>
    public class AssemblyBuilderKit
    {
        private readonly string _name;
        private readonly System.Reflection.Emit.AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _module;

        /// <summary>
        /// Initializes a new dynamic assembly + module pair with a random name.
        /// </summary>
        public AssemblyBuilderKit()
        {
            _name = "AeterRatioDynamicEmit." + Guid.NewGuid().ToString("N");

            _assemblyBuilder = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_name), AssemblyBuilderAccess.Run);
            _module = _assemblyBuilder.DefineDynamicModule(_name);
        }

        /// <summary>
        /// Defines a new runtime type with the provided base class and interface implementations.
        /// </summary>
        /// <param name="name">Short name of the type (the module name is prefixed automatically).</param>
        /// <param name="inherits">Base class for the type.</param>
        /// <param name="implements">Interfaces the type must implement.</param>
        public ClassBuilder DefineClass(string name, Type inherits, Type[] implements)
        {
            const TypeAttributes attributes = TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.AutoClass | TypeAttributes.BeforeFieldInit;
            var classFullName = string.Concat(_name, '.', name);
            return new ClassBuilder(_module.DefineType(classFullName, attributes, inherits, implements));
        }
    }
}
