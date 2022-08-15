/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection;
using Aeter.Ratio.Reflection.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Aeter.Ratio.Reflection.Emit.Pointers;

namespace Aeter.Ratio.DependencyInjection
{
    public class DynamicActivatorInstanceFactory<TInstance> : IInstanceFactory<TInstance>
        where TInstance : notnull
    {
        private readonly DynamicActivator _activator;
        private readonly IEnumerable<IInstanceFactory> _conParamFactories;
        private readonly Action<TInstance, object[]>? _propertySetter;
        private readonly IEnumerable<IInstanceFactory>? _propParamFactories;

        public DynamicActivatorInstanceFactory(ConstructorInfo constructor, PropertyInfo[] properties, ITypeProvider provider, IEnumerable<IInstanceFactory> parameterFactories)
        {
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            _activator = new DynamicActivator(constructor, provider);

            if (properties != null && properties.Length > 0) {
                var cons = new IInstanceFactory[constructor.GetParameters().Length];
                var props = new IInstanceFactory[properties.Length];

                var ci = 0;
                var pi = 0;
                foreach (var factory in parameterFactories) {
                    if (ci < cons.Length) {
                        cons[ci++] = factory;
                    }
                    else {
                        props[pi++] = factory;
                    }
                }

                _conParamFactories = cons;
                _propParamFactories = props;
                _propertySetter = GetPropertySetter(properties, provider);
            }
            else {
                _conParamFactories = parameterFactories;
            }
        }

        private Action<TInstance, object[]> GetPropertySetter(PropertyInfo[] properties, ITypeProvider provider)
        {
            var type = typeof(TInstance);

            var methodName = string.Concat(
                "D$PropertySetter$",
                type.FullName!.Replace(".", "_"),
                "$",
                Guid.NewGuid());

            var method = new DynamicMethod(methodName, typeof(void), new[] { type, typeof(object[])});

            var il = method.GetILGenerator();
            var instance = ILPointer.Arg(0, type);
            var props = ILPointer.Arg(1, typeof(object[]));

            for (var i = 0; i < properties.Length; i++) {
                var property = properties[i];
                var value = props
                    .ElementAt(i)
                    .Cast(property.PropertyType);

                il.Set(instance, property, value);
            }
            il.Emit(OpCodes.Ret);

            return (Action<TInstance, object[]>)
                method.CreateDelegate(typeof(Action<TInstance, object[]>));
        }

        public TInstance GetInstance()
        {
            var conParams = _conParamFactories
                .Select(f => f.GetInstance())
                .ToArray();

            var instance = (TInstance)_activator.Activate(conParams);
            if (_propertySetter != null && _propParamFactories != null) {
                var propParams = _propParamFactories
                    .Select(f => f.GetInstance())
                    .ToArray();
                _propertySetter.Invoke(instance, propParams);
            }
            return instance;
        }

        object IInstanceFactory.GetInstance()
        {
            return GetInstance();
        }
    }
}
