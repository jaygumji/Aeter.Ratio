/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILCallMethodSnippet : ILSnippet
    {
        private readonly IILPointer _instance;
        private readonly MethodInfo _method;
        private readonly ILPointer[] _parameters;
        private readonly ParameterInfo[] _methodParameters;

        public ILCallMethodSnippet(MethodInfo method, params ILPointer[] parameters) : this(null, method, parameters)
        {
        }

        public ILCallMethodSnippet(ILPointer instance, MethodInfo method, params ILPointer[] parameters)
        {
            if (instance == null && !method.IsStatic)
                throw new ArgumentException("Instance must be provided for instance methods");
            if (instance != null && method.IsStatic)
                throw new ArgumentException("Static method may not be invoked with an instance");

            if (!(method is System.Reflection.Emit.MethodBuilder)) {
                _methodParameters = method.GetParameters();

                if (_methodParameters.Length < parameters.Length)
                    throw new ArgumentException("The parameter length supplied is greater than the method supports");
            }

            _instance = instance;
            _method = method;
            _parameters = parameters;
            ReturnType = method.ReturnType;
        }

        public Type ReturnType { get; }

        protected override void OnGenerate(ILGenerator il)
        {
            var instanceIsValueType = _instance?.Type != null
                && _instance.Type.GetTypeInfo().IsValueType;

            if (_instance != null) {
                if (instanceIsValueType)
                    _instance.LoadAddress(il);
                else
                    _instance.Load(il);
            }

            for (var i = 0; i < _parameters.Length; i++) {
                var parameter = (IILPointer) _parameters[i] ?? ILPointer.Null;

                if (_methodParameters != null && _methodParameters[i].IsOut) {
                    parameter.LoadAddress(il);
                }
                else {
                    parameter.Load(il);
                }
            }

            if (_method.IsStatic || instanceIsValueType) {
                il.EmitCall(OpCodes.Call, _method, null);
            }
            else {
                il.EmitCall(OpCodes.Callvirt, _method, null);
            }
        }
    }
}