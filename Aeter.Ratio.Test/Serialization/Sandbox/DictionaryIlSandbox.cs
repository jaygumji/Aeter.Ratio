/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection.Emit;
using Aeter.Ratio.Serialization;
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Test.Serialization.Sandbox
{
    public class DictionaryIlSandbox
    {
        private AssemblyBuilderKit _kit;
        private ClassBuilder _classBuilder;
        private MethodBuilder _methodBuilder;

        public DictionaryIlSandbox()
        {
            _kit = new AssemblyBuilderKit();
            _classBuilder = _kit.DefineClass("SandboxClass", typeof(object), Type.EmptyTypes);
            _methodBuilder = _classBuilder.DefineMethod("Execute", typeof(void), new[] { typeof(IReadVisitor), typeof(Fakes.ValueDictionary) });

            _classBuilder.Seal();
        }

        public void Invoke(IReadVisitor visitor, Fakes.ValueDictionary graph)
        {
            var instance = Activator.CreateInstance(typeof(Fakes.ValueDictionary));
        }

    }
}
