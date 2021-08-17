/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection;
using Aeter.Ratio.Reflection.Emit;
using System;
using System.Reflection;
using System.Reflection.Emit;
using Aeter.Ratio.Reflection.Emit.Pointers;

namespace Aeter.Ratio
{
    public class DynamicActivator
    {

        private static readonly Type[] DynConstructorTypes = new[] { typeof(object[]) };

        private readonly Func<object[], object> _activate;

        public Type Type { get; }

        public DynamicActivator(Type type, params Type[] parameterTypes)
            : this(type.GetTypeInfo().GetConstructor(parameterTypes))
        {
        }

        public DynamicActivator(Type type, ITypeProvider provider, params Type[] parameterTypes)
            : this(type.GetTypeInfo().GetConstructor(parameterTypes), provider)
        {
        }

        public DynamicActivator(ConstructorInfo constructor)
            : this(constructor, new FactoryTypeProvider())
        {
        }

        public DynamicActivator(ConstructorInfo constructor, ITypeProvider provider)
        {
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            Type = constructor.DeclaringType;

            var methodName = string.Concat(
                "D$Activator$",
                Type.FullName.Replace(".", "_"),
                "$",
                constructor.GetParameters().Length,
                Guid.NewGuid());
            var method = new DynamicMethod(methodName, typeof(object), DynConstructorTypes);

            var il = method.GetILGenerator();
            var methodParams = ILPointer.Arg(0, typeof(object[]));

            var parameters = constructor.GetParameters();

            var constructParameters = new ILPointer[parameters.Length];
            for (var i = 0; i < parameters.Length; i++) {
                var parameter = parameters[i];
                var constructParam = methodParams
                    .ElementAt(i)
                    .Cast(parameter.ParameterType);

                constructParameters[i] = constructParam;
            }

            il.Construct(constructor, constructParameters);
            il.Emit(OpCodes.Ret);

            _activate = (Func<object[], object>)
                method.CreateDelegate(typeof(Func<object[], object>));
        }

        public object Activate(params object[] parameters)
        {
            return _activate.Invoke(parameters);
        }

    }
}
